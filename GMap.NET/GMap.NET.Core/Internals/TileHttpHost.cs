using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GMap.NET.MapProviders;

namespace GMap.NET.Internals
{
    internal class TileHttpHost
    {
        volatile bool _listen;
        TcpListener _server;
        int _port;

        readonly byte[] _responseHeaderBytes;

        public TileHttpHost()
        {
            string response = "HTTP/1.0 200 OK\r\nContent-Type: image\r\nConnection: close\r\n\r\n";
            _responseHeaderBytes = Encoding.ASCII.GetBytes(response);
        }

        public void Stop()
        {
            if (_listen)
            {
                _listen = false;
                if (_server != null)
                {
                    _server.Stop();
                }
            }
        }

        public void Start(int port)
        {
            if (_server == null)
            {
                this._port = port;
                _server = new TcpListener(IPAddress.Any, port);
            }
            else
            {
                if (this._port != port)
                {
                    Stop();
                    this._port = port;
                    _server = null;
                    _server = new TcpListener(IPAddress.Any, port);
                }
                else
                {
                    if (_listen)
                    {
                        return;
                    }
                }
            }

            _server.Start();
            _listen = true;

            var t = new Thread(() =>
            {
                Debug.WriteLine("TileHttpHost: " + _server.LocalEndpoint);

                while (_listen)
                {
                    try
                    {
                        if (!_server.Pending())
                        {
                            Thread.Sleep(111);
                        }
                        else
                        {
                            ThreadPool.QueueUserWorkItem(ProcessRequest, _server.AcceptTcpClient());
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("TileHttpHost: " + ex);
                    }
                }

                Debug.WriteLine("TileHttpHost: stoped");
            });

            t.Name = "TileHost";
            t.IsBackground = true;
            t.Start();
        }

        void ProcessRequest(object p)
        {
            try
            {
                using (var c = p as TcpClient)
                {
                    using (var s = c.GetStream())
                    {
                        using (var r = new StreamReader(s, Encoding.UTF8))
                        {
                            string request = r.ReadLine();

                            if (!string.IsNullOrEmpty(request) && request.StartsWith("GET"))
                            {
                                //Debug.WriteLine("TileHttpHost: " + request);

                                // http://localhost:88/88888/5/15/11
                                // GET /8888888888/5/15/11 HTTP/1.1

                                var rq = request.Split(' ');
                                if (rq.Length >= 2)
                                {
                                    var ids = rq[1].Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
                                    if (ids.Length == 4)
                                    {
                                        int dbId = int.Parse(ids[0]);
                                        int zoom = int.Parse(ids[1]);
                                        int x = int.Parse(ids[2]);
                                        int y = int.Parse(ids[3]);

                                        var pr = GMapProviders.TryGetProvider(dbId);
                                        if (pr != null)
                                        {
                                            Exception ex;
                                            var img = GMaps.Instance.GetImageFrom(pr, new GPoint(x, y), zoom, out ex);
                                            if (img != null)
                                            {
                                                using (img)
                                                {
                                                    s.Write(_responseHeaderBytes, 0, _responseHeaderBytes.Length);
                                                    img.Data.WriteTo(s);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    c.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TileHttpHost, ProcessRequest: " + ex);
            }

            //Debug.WriteLine("disconnected");
        }
    }
}
