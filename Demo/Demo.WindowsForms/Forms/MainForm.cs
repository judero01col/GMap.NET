using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Demo.WindowsForms.CustomMarkers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.Reflection;
using BSE.Windows.Forms;

namespace Demo.WindowsForms
{
    public partial class MainForm : Form
    {
        // layers
        readonly GMapOverlay _top = new GMapOverlay();
        internal readonly GMapOverlay Objects = new GMapOverlay("objects");
        internal readonly GMapOverlay Routes = new GMapOverlay("routes");
        internal readonly GMapOverlay Polygons = new GMapOverlay("polygons");

        // marker
        GMapMarker _currentMarker;

        // polygons
        GMapPolygon _polygon;

        // etc
        readonly Random _rnd = new Random();
        readonly DescendingComparer _comparerIpStatus = new DescendingComparer();
        GMapMarkerRect _curentRectMarker;
        string _mobileGpsLog = string.Empty;
        bool _isMouseDown;
        PointLatLng _start;
        PointLatLng _end;

        public MainForm()
        {
            InitializeComponent();

            if (!GMapControl.IsDesignerHosted)
            {
                // add your custom map db provider
                //MsSQLPureImageCache ch = new MsSQLPureImageCache();
                //ch.ConnectionString = @"data source = sql5040.site4now.net;User Id=DB_A3B2C9_GMapNET_admin; initial catalog = DB_A3B2C9_GMapNET; password = Usuario@2018;";                
                //MainMap.Manager.SecondaryCache = ch;

                // set your proxy here if need
                //GMapProvider.IsSocksProxy = true;
                //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                //GMapProvider.WebProxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");
                // or
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                //                          

                // set cache mode only if no internet avaible
                if (!Stuff.PingNetwork("pingtest.com"))
                {
                    MainMap.Manager.Mode = AccessMode.CacheOnly;
                    MessageBox.Show("No internet connection available, going to CacheOnly mode.", "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                //Config Map
                MainMap.MapProvider = GMapProviders.GoogleMap;

                // Custom Map Provider
                //MainMap.MapProvider = GMapProviders.CustomMap;
                //GMapProviders.CustomMap.CustomServerUrl = "https://{l}.tile.openstreetmap.org/{z}/{x}/{y}.png";
                //GMapProviders.CustomMap.CustomServerLetters = "abc";

                MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
                MainMap.MinZoom = 0;
                MainMap.MaxZoom = 24;
                MainMap.Zoom = 9;

                GoogleMapProvider.Instance.ApiKey = Stuff.GoogleMapsApiKey;

                //MainMap.ScaleMode = ScaleModes.Fractional;

                // map events
                {
                    MainMap.OnPositionChanged += MainMap_OnPositionChanged;
                    MainMap.OnTileLoadStart += MainMap_OnTileLoadStart;
                    MainMap.OnTileLoadComplete += MainMap_OnTileLoadComplete;
                    MainMap.OnMapClick += MainMap_OnMapClick;

                    MainMap.OnMapZoomChanged += MainMap_OnMapZoomChanged;
                    MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;

                    MainMap.OnMarkerClick += MainMap_OnMarkerClick;
                    MainMap.OnMarkerDoubleClick += MainMap_OnMarkerDoubleClick;

                    MainMap.OnMarkerEnter += MainMap_OnMarkerEnter;
                    MainMap.OnMarkerLeave += MainMap_OnMarkerLeave;

                    MainMap.OnPolygonEnter += MainMap_OnPolygonEnter;
                    MainMap.OnPolygonLeave += MainMap_OnPolygonLeave;

                    MainMap.OnPolygonClick += MainMap_OnPolygonClick;
                    MainMap.OnPolygonDoubleClick += MainMap_OnPolygonDoubleClick;

                    MainMap.OnRouteEnter += MainMap_OnRouteEnter;
                    MainMap.OnRouteLeave += MainMap_OnRouteLeave;

                    MainMap.OnRouteClick += MainMap_OnRouteClick;
                    MainMap.OnRouteDoubleClick += MainMap_OnRouteDoubleClick;

                    MainMap.Manager.OnTileCacheComplete += OnTileCacheComplete;
                    MainMap.Manager.OnTileCacheStart += OnTileCacheStart;
                    MainMap.Manager.OnTileCacheProgress += OnTileCacheProgress;
                }

                MainMap.MouseMove += MainMap_MouseMove;
                MainMap.MouseDown += MainMap_MouseDown;
                MainMap.MouseUp += MainMap_MouseUp;
                MainMap.MouseDoubleClick += MainMap_MouseDoubleClick;

                // get map types
            #if !MONO   // mono doesn't handle it, so we 'lost' provider list ;]
                comboBoxMapType.ValueMember = "Name";
                comboBoxMapType.DataSource = GMapProviders.List;
                comboBoxMapType.SelectedItem = MainMap.MapProvider;
            #endif
                // acccess mode
                comboBoxMode.DataSource = Enum.GetValues(typeof(AccessMode));
                comboBoxMode.SelectedItem = MainMap.Manager.Mode;

                // get position
                textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
                textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);

                // get cache modes
                checkBoxUseRouteCache.Checked = MainMap.Manager.UseRouteCache;

                MobileLogFrom.Value = DateTime.Today;
                MobileLogTo.Value = DateTime.Now;

                // get zoom  
                trackBar1.Minimum = MainMap.MinZoom * 100;
                trackBar1.Maximum = MainMap.MaxZoom * 100;
                trackBar1.TickFrequency = 100;

            #if DEBUG
                    checkBoxDebug.Checked = true;
            #endif

                ToolStripManager.Renderer = new Office2007Renderer();

                #region -- demo workers --
                // flight demo
                {
                    _flightWorker.DoWork += flight_DoWork;
                    _flightWorker.ProgressChanged += flight_ProgressChanged;
                    _flightWorker.WorkerSupportsCancellation = true;
                    _flightWorker.WorkerReportsProgress = true;
                }

                // vehicle demo
                {
                    _transportWorker.DoWork += transport_DoWork;
                    _transportWorker.ProgressChanged += transport_ProgressChanged;
                    _transportWorker.WorkerSupportsCancellation = true;
                    _transportWorker.WorkerReportsProgress = true;
                }

                // Connections
                {
                    _connectionsWorker.DoWork += connectionsWorker_DoWork;
                    _connectionsWorker.ProgressChanged += connectionsWorker_ProgressChanged;
                    _connectionsWorker.WorkerSupportsCancellation = true;
                    _connectionsWorker.WorkerReportsProgress = true;

                    _ipInfoSearchWorker.DoWork += ipInfoSearchWorker_DoWork;
                    _ipInfoSearchWorker.WorkerSupportsCancellation = true;

                    _iptracerWorker.DoWork += iptracerWorker_DoWork;
                    _iptracerWorker.WorkerSupportsCancellation = true;

                    GridConnections.AutoGenerateColumns = false;

#if SQLite
                    _ipCache.CacheLocation = MainMap.CacheLocation;
#endif
                }

                // perf
                _timerPerf.Tick += timer_Tick;
                #endregion

                // add custom layers  
                {
                    MainMap.Overlays.Add(Routes);
                    MainMap.Overlays.Add(Polygons);
                    MainMap.Overlays.Add(Objects);
                    MainMap.Overlays.Add(_top);

                    Routes.Routes.CollectionChanged += Routes_CollectionChanged;
                    Objects.Markers.CollectionChanged += Markers_CollectionChanged;
                }

                // set current marker
                _currentMarker = new GMarkerGoogle(MainMap.Position, GMarkerGoogleType.arrow);
                _currentMarker.IsHitTestVisible = false;
                _top.Markers.Add(_currentMarker);

                //MainMap.VirtualSizeEnabled = true;
                //if(false)
                {
                    // add my city location for demo
                    GeoCoderStatusCode status;
                    var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);

                    if (pos != null && status == GeoCoderStatusCode.OK)
                    {
                        _currentMarker.Position = pos.Value;

                        GMapMarker myCity = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green_small);
                        myCity.ToolTipMode = MarkerTooltipMode.Always;
                        myCity.ToolTipText = "Welcome to Lithuania! ;}";
                        Objects.Markers.Add(myCity);
                    }

                    // add some points in lithuania
                    AddLocationLithuania("Kaunas");
                    AddLocationLithuania("Klaipėda");
                    AddLocationLithuania("Šiauliai");
                    AddLocationLithuania("Panevėžys");

                    if (Objects.Markers.Count > 0)
                    {
                        MainMap.ZoomAndCenterMarkers(null);
                    }

                    RegeneratePolygon();

                    try
                    {
                        var overlay = DeepClone(Objects);
                        Debug.WriteLine("ISerializable status for markers: OK");

                        var overlay2 = DeepClone(Polygons);
                        Debug.WriteLine("ISerializable status for polygons: OK");

                        var overlay3 = DeepClone(Routes);
                        Debug.WriteLine("ISerializable status for routes: OK");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ISerializable failure: " + ex.Message);

#if DEBUG
                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
#endif
                    }
                }
            }
        }

        private void MainMap_OnMapClick(PointLatLng pointClick, MouseEventArgs e)
        {
            MainMap.FromLocalToLatLng(e.X, e.Y);
        }

        private void MainMap_OnRouteDoubleClick(GMapRoute item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnRouteClick(GMapRoute item, MouseEventArgs e)
        {
           
        }

        private void MainMap_OnPolygonDoubleClick(GMapPolygon item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnMarkerDoubleClick(GMapMarker item, MouseEventArgs e)
        {
            
        }

        public T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                formatter.Serialize(ms, obj);

                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        void Markers_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
        {
            textBoxMarkerCount.Text = Objects.Markers.Count.ToString();
        }

        void Routes_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
        {
            textBoxrouteCount.Text = Routes.Routes.Count.ToString();
        }

        #region -- performance test --

        double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        int _tt;
        void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(_rnd, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(_rnd, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
            GMapMarker m = new GMarkerGoogle(pos, GMarkerGoogleType.green_pushpin);
            {
                m.ToolTipText = (_tt++).ToString();
                m.ToolTipMode = MarkerTooltipMode.Always;
            }

            Objects.Markers.Add(m);

            if (_tt >= 333)
            {
                _timerPerf.Stop();
                _tt = 0;
            }
        }

        System.Windows.Forms.Timer _timerPerf = new System.Windows.Forms.Timer();
        #endregion

        #region -- flight demo --
        BackgroundWorker _flightWorker = new BackgroundWorker();

        readonly List<FlightRadarData> _flights = new List<FlightRadarData>();
        readonly Dictionary<int, GMapMarker> _flightMarkers = new Dictionary<int, GMapMarker>();

        bool _firstLoadFlight = true;
        GMapMarker _currentFlight;
        RectLatLng _flightBounds = new RectLatLng(54.4955675218741, -0.966796875, 28.916015625, 13.3830987326932);

        void flight_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // stops immediate marker/route/polygon invalidations;
            // call Refresh to perform single refresh and reset invalidation state
            MainMap.HoldInvalidation = true;

            lock (_flights)
            {
                if (_flightBounds != MainMap.ViewArea)
                {
                    _flightBounds = MainMap.ViewArea;
                    foreach (var m in Objects.Markers)
                    {
                        if (!_flightBounds.Contains(m.Position))
                        {
                            m.IsVisible = false;
                        }
                        else
                        {
                            m.IsVisible = true;
                        }
                    }
                }

                foreach (var d in _flights)
                {
                    GMapMarker marker;

                    if (!_flightMarkers.TryGetValue(d.Id, out marker))
                    {
                        marker = new GMarkerArrow(d.Point);
                        marker.Tag = d.Id;
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                        (marker as GMarkerArrow).Bearing = d.Bearing;

                        _flightMarkers[d.Id] = marker;
                        Objects.Markers.Add(marker);
                    }
                    else
                    {
                        marker.Position = d.Point;
                        (marker as GMarkerArrow).Bearing = d.Bearing;
                    }
                    marker.ToolTipText = d.Name + ", " + d.Altitude + ", " + d.Speed;

                    if (_currentFlight != null && _currentFlight == marker)
                    {
                        MainMap.Position = marker.Position;
                        MainMap.Bearing = d.Bearing;
                    }
                }
            }

            if (_firstLoadFlight)
            {
                MainMap.Zoom = 5;
                MainMap.SetZoomToFitRect(new RectLatLng(54.4955675218741, -0.966796875, 28.916015625, 13.3830987326932));
                _firstLoadFlight = false;
            }
            MainMap.Refresh();
        }

        void flight_DoWork(object sender, DoWorkEventArgs e)
        {
            //bool restartSesion = true;

            while (!_flightWorker.CancellationPending)
            {
                try
                {
                    lock (_flights)
                    {
                        //Stuff.GetFlightRadarData(flights, lastPosition, lastZoom, restartSesion);

                        Stuff.GetFlightRadarData(_flights, _flightBounds);

                        //if(flights.Count > 0 && restartSesion)
                        //{
                        //   restartSesion = false;
                        //}
                    }

                    _flightWorker.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("flight_DoWork: " + ex.ToString());
                }
                Thread.Sleep(5 * 1000);
            }

            _flightMarkers.Clear();
        }

        #endregion

        #region -- transport demo --
        BackgroundWorker _transportWorker = new BackgroundWorker();

        #region -- old vehicle demo --
        readonly List<VehicleData> _trolleybus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _trolleybusMarkers = new Dictionary<int, GMapMarker>();

        readonly List<VehicleData> _bus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _busMarkers = new Dictionary<int, GMapMarker>();
        #endregion

        bool _firstLoadTrasport = true;
        GMapMarker _currentTransport;

        void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // stops immediate marker/route/polygon invalidations;
            // call Refresh to perform single refresh and reset invalidation state
            MainMap.HoldInvalidation = true;

            #region -- vehicle demo --
            lock (_trolleybus)
            {
                foreach (var d in _trolleybus)
                {
                    GMapMarker marker;

                    if (!_trolleybusMarkers.TryGetValue(d.Id, out marker))
                    {
                        marker = new GMarkerGoogle(new PointLatLng(d.Lat, d.Lng), GMarkerGoogleType.red_small);
                        marker.Tag = d.Id;
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                        _trolleybusMarkers[d.Id] = marker;
                        Objects.Markers.Add(marker);
                    }
                    else
                    {
                        marker.Position = new PointLatLng(d.Lat, d.Lng);
                        //(marker as GMarkerGoogle).Bearing = (float?) d.Bearing;
                    }
                    marker.ToolTipText = "Trolley " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

                    if (_currentTransport != null && _currentTransport == marker)
                    {
                        MainMap.Position = marker.Position;
                        if (d.Bearing.HasValue)
                        {
                            MainMap.Bearing = (float)d.Bearing.Value;
                        }
                    }
                }
            }

            lock (_bus)
            {
                foreach (var d in _bus)
                {
                    GMapMarker marker;

                    if (!_busMarkers.TryGetValue(d.Id, out marker))
                    {
                        marker = new GMarkerGoogle(new PointLatLng(d.Lat, d.Lng), GMarkerGoogleType.green_small);
                        marker.Tag = d.Id;
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                        _busMarkers[d.Id] = marker;
                        Objects.Markers.Add(marker);
                    }
                    else
                    {
                        marker.Position = new PointLatLng(d.Lat, d.Lng);
                        //(marker as GMarkerGoogle).Bearing = (float?) d.Bearing;
                    }
                    marker.ToolTipText = "Bus " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

                    if (_currentTransport != null && _currentTransport == marker)
                    {
                        MainMap.Position = marker.Position;
                        if (d.Bearing.HasValue)
                        {
                            MainMap.Bearing = (float)d.Bearing.Value;
                        }
                    }
                }
            }
            #endregion

            if (_firstLoadTrasport)
            {
                MainMap.Zoom = 5;
                MainMap.ZoomAndCenterMarkers("objects");
                _firstLoadTrasport = false;
            }
            MainMap.Refresh();
        }

        void transport_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_transportWorker.CancellationPending)
            {
                try
                {
                    #region -- old vehicle demo --
                    lock (_trolleybus)
                    {
                        Stuff.GetVilniusTransportData(TransportType.TrolleyBus, string.Empty, _trolleybus);
                    }

                    lock (_bus)
                    {
                        Stuff.GetVilniusTransportData(TransportType.Bus, string.Empty, _bus);
                    }
                    #endregion

                    _transportWorker.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("transport_DoWork: " + ex.ToString());
                }
                Thread.Sleep(2 * 1000);
            }

            _trolleybusMarkers.Clear();
            _busMarkers.Clear();
        }

        #endregion

        #region -- tcp/ip connections demo --
        BackgroundWorker _connectionsWorker = new BackgroundWorker();
        BackgroundWorker _ipInfoSearchWorker = new BackgroundWorker();
        BackgroundWorker _iptracerWorker = new BackgroundWorker();

        readonly Dictionary<string, IpInfo> _tcpState = new Dictionary<string, IpInfo>();
        readonly Dictionary<string, IpInfo> _tcpTracePoints = new Dictionary<string, IpInfo>();
        readonly Dictionary<string, List<PingReply>> _traceRoutes = new Dictionary<string, List<PingReply>>();

        readonly List<string> _tcpStateNeedLocationInfo = new List<string>();
        readonly Queue<string> _tcpStateNeedTraceInfo = new Queue<string>();

        volatile bool _tryTraceConnection;
        GMapMarker _lastTcpMarker;
#if SQLite
        readonly SQLiteIpCache _ipCache = new SQLiteIpCache();
#endif

        readonly Dictionary<string, GMapMarker> _tcpConnections = new Dictionary<string, GMapMarker>();
        readonly Dictionary<string, GMapRoute> _tcpRoutes = new Dictionary<string, GMapRoute>();

        readonly List<IpStatus> _countryStatusView = new List<IpStatus>();
        readonly SortedDictionary<string, int> _countryStatus = new SortedDictionary<string, int>();

        readonly List<string> _selectedCountries = new List<string>();
        readonly Dictionary<int, Process> _processList = new Dictionary<int, Process>();

        void ipInfoSearchWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_ipInfoSearchWorker.CancellationPending)
            {
                try
                {
                    string iplist = string.Empty;

                    lock (_tcpStateNeedLocationInfo)
                    {
                        //int count = 0;
                        foreach (string info in _tcpStateNeedLocationInfo)
                        {
                            if (iplist.Length > 0)
                            {
                                iplist += ",";
                            }
                            iplist += info;

                            //if(count++ >= 1)
                            {
                                break;
                            }
                        }
                    }

                    // fill location info
                    if (!string.IsNullOrEmpty(iplist))
                    {
                        var ips = GetIpHostInfo(iplist);
                        foreach (var i in ips)
                        {
                            lock (_tcpState)
                            {
                                IpInfo info;
                                if (_tcpState.TryGetValue(i.Ip, out info))
                                {
                                    info.CountryName = i.CountryName;
                                    info.RegionName = i.RegionName;
                                    info.City = i.City;
                                    info.Latitude = i.Latitude;
                                    info.Longitude = i.Longitude;
                                    info.TracePoint = false;

                                    if (info.CountryName != "Reserved")
                                    {
                                        info.Ip = i.Ip;

                                        // add host for tracing
                                        lock (_tcpStateNeedTraceInfo)
                                        {
                                            if (!_tcpStateNeedTraceInfo.Contains(i.Ip))
                                            {
                                                _tcpStateNeedTraceInfo.Enqueue(i.Ip);
                                            }
                                        }
                                    }

                                    lock (_tcpStateNeedLocationInfo)
                                    {
                                        _tcpStateNeedLocationInfo.Remove(i.Ip);

                                        Debug.WriteLine("TcpStateNeedLocationInfo: " + _tcpStateNeedLocationInfo.Count + " left...");
                                    }
                                }
                            }
                        }
                        ips.Clear();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ipInfoSearchWorker_DoWork: " + ex.ToString());
                }
                Thread.Sleep(1111);
            }
            Debug.WriteLine("ipInfoSearchWorker_DoWork: QUIT");
        }

        void iptracerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_iptracerWorker.CancellationPending)
            {
                try
                {
                    string ip = string.Empty;
                    int count;
                    lock (_tcpStateNeedTraceInfo)
                    {
                        count = _tcpStateNeedTraceInfo.Count;
                        if (count > 0)
                        {
                            ip = _tcpStateNeedTraceInfo.Dequeue();
                        }
                    }

                    if (!string.IsNullOrEmpty(ip))
                    {
                        string tracertIps = string.Empty;

                        List<PingReply> tracert;

                        bool contains;
                        lock (_traceRoutes)
                        {
                            contains = _traceRoutes.TryGetValue(ip, out tracert);
                        }

                        if (!contains)
                        {
                            Debug.WriteLine("GetTraceRoute: " + ip + ", left " + count);

                            tracert = TraceRoute.GetTraceRoute(ip);
                            if (tracert != null)
                            {
                                if (tracert[tracert.Count - 1].Status == IPStatus.Success)
                                {
                                    foreach (var t in tracert)
                                    {
                                        if (!t.ToString().StartsWith("192.168.") && !t.ToString().StartsWith("127.0."))
                                        {
                                            if (tracertIps.Length > 0)
                                            {
                                                tracertIps += ",";
                                            }
                                            tracertIps += t.Address.ToString();
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(tracertIps))
                                    {
                                        var tinfo = GetIpHostInfo(tracertIps);
                                        if (tinfo.Count > 0)
                                        {
                                            for (int i = 0; i < tinfo.Count; i++)
                                            {
                                                var ti = tinfo[i];
                                                ti.TracePoint = true;

                                                if (ti.CountryName != "Reserved")
                                                {
                                                    lock (_tcpTracePoints)
                                                    {
                                                        _tcpTracePoints[ti.Ip] = ti;
                                                    }
                                                }
                                            }
                                            tinfo.Clear();

                                            lock (_traceRoutes)
                                            {
                                                _traceRoutes[ip] = tracert;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // move failed, eque itself again
                                    lock (_tcpStateNeedTraceInfo)
                                    {
                                        _tcpStateNeedTraceInfo.Enqueue(ip);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("iptracerWorker_DoWork: " + ex.ToString());
                }
                Thread.Sleep(3333);
            }
            Debug.WriteLine("iptracerWorker_DoWork: QUIT");
        }

        void connectionsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
#if !MONO
            var properties = IPGlobalProperties.GetIPGlobalProperties();

            while (!_connectionsWorker.CancellationPending)
            {
                try
                {
                    #region -- xml --
                    // http://ipinfodb.com/ip_location_api.php

                    // http://ipinfodb.com/ip_query2.php?ip=74.125.45.100,206.190.60.37&timezone=false

                    //<?xml version="1.0" encoding="UTF-8"?>
                    //<Locations>
                    //  <Location id="0">
                    //    <Ip>74.125.45.100</Ip>
                    //    <Status>OK</Status>
                    //    <CountryCode>US</CountryCode>
                    //    <CountryName>United States</CountryName>
                    //    <RegionCode>06</RegionCode>
                    //    <RegionName>California</RegionName>
                    //    <City>Mountain View</City>
                    //    <ZipPostalCode>94043</ZipPostalCode>
                    //    <Latitude>37.4192</Latitude>
                    //    <Longitude>-122.057</Longitude>
                    //  </Location> 
                    #endregion

                    lock (_tcpState)
                    {
                        //TcpConnectionInformation[] tcpInfoList = properties.GetActiveTcpConnections();
                        //foreach(TcpConnectionInformation i in tcpInfoList)
                        //{

                        //}

                        _countryStatus.Clear();
                        ManagedIpHelper.UpdateExtendedTcpTable(false);

                        foreach (var i in ManagedIpHelper.TcpRows)
                        {
                            #region -- update TcpState --
                            string ip = i.RemoteEndPoint.Address.ToString();

                            // exclude local network
                            if (!ip.StartsWith("192.168.") && !ip.StartsWith("127.0."))
                            {
                                IpInfo info;
                                if (!_tcpState.TryGetValue(ip, out info))
                                {
                                    info = new IpInfo();
                                    _tcpState[ip] = info;

                                    // request location info
                                    lock (_tcpStateNeedLocationInfo)
                                    {
                                        if (!_tcpStateNeedLocationInfo.Contains(ip))
                                        {
                                            _tcpStateNeedLocationInfo.Add(ip);

                                            if (!_ipInfoSearchWorker.IsBusy)
                                            {
                                                _ipInfoSearchWorker.RunWorkerAsync();
                                            }
                                        }
                                    }
                                }

                                info.State = i.State;
                                info.Port = i.RemoteEndPoint.Port;
                                info.StatusTime = DateTime.Now;

                                try
                                {
                                    Process p;
                                    if (!_processList.TryGetValue(i.ProcessId, out p))
                                    {
                                        p = Process.GetProcessById(i.ProcessId);
                                        _processList[i.ProcessId] = p;
                                    }
                                    info.ProcessName = p.ProcessName;
                                }
                                catch
                                {
                                }

                                if (!string.IsNullOrEmpty(info.CountryName))
                                {
                                    if (!_countryStatus.ContainsKey(info.CountryName))
                                    {
                                        _countryStatus[info.CountryName] = 1;
                                    }
                                    else
                                    {
                                        _countryStatus[info.CountryName]++;
                                    }
                                }
                            }
                            #endregion
                        }
                    }

                    // launch tracer if needed
                    if (_tryTraceConnection)
                    {
                        if (!_iptracerWorker.IsBusy)
                        {
                            lock (_tcpStateNeedTraceInfo)
                            {
                                if (_tcpStateNeedTraceInfo.Count > 0)
                                {
                                    _iptracerWorker.RunWorkerAsync();
                                }
                            }
                        }
                    }

                    _connectionsWorker.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("connectionsWorker_DoWork: " + ex.ToString());
                }
                Thread.Sleep(3333);
            }
            _tcpConnections.Clear();
#endif
        }

        void connectionsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                // stops immediate marker/route/polygon invalidations;
                // call Refresh to perform single refresh and reset invalidation state
                MainMap.HoldInvalidation = true;

                _selectedCountries.Clear();
                Int32 selectedCountriesCount = GridConnections.Rows.GetRowCount(DataGridViewElementStates.Selected);
                if (selectedCountriesCount > 0)
                {
                    for (int i = 0; i < selectedCountriesCount; i++)
                    {
                        string country = GridConnections.SelectedRows[i].Cells[0].Value as string;
                        _selectedCountries.Add(country);
                    }
                }

                _comparerIpStatus.SortOnlyCountryName = !(selectedCountriesCount == 0);

                lock (_tcpState)
                {
                    bool snap = true;
                    foreach (var tcp in _tcpState)
                    {
                        GMapMarker marker;

                        if (!_tcpConnections.TryGetValue(tcp.Key, out marker))
                        {
                            if (!string.IsNullOrEmpty(tcp.Value.Ip))
                            {
                                marker = new GMarkerGoogle(new PointLatLng(tcp.Value.Latitude, tcp.Value.Longitude), GMarkerGoogleType.green_small);
                                marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                                marker.Tag = tcp.Value.CountryName;

                                _tcpConnections[tcp.Key] = marker;
                                {
                                    if (!(selectedCountriesCount > 0 && !_selectedCountries.Contains(tcp.Value.CountryName)))
                                    {
                                        Objects.Markers.Add(marker);

                                        UpdateMarkerTcpIpToolTip(marker, tcp.Value, "(" + Objects.Markers.Count + ") ");

                                        if (snap)
                                        {
                                            if (checkBoxTcpIpSnap.Checked && !MainMap.IsDragging)
                                            {
                                                MainMap.Position = marker.Position;
                                            }
                                            snap = false;

                                            if (_lastTcpMarker != null)
                                            {
                                                marker.ToolTipMode = MarkerTooltipMode.Always;
                                                _lastTcpMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                                            }

                                            _lastTcpMarker = marker;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((DateTime.Now - tcp.Value.StatusTime > TimeSpan.FromSeconds(8)) || (selectedCountriesCount > 0 && !_selectedCountries.Contains(tcp.Value.CountryName)))
                            {
                                Objects.Markers.Remove(marker);

                                GMapRoute route;
                                if (_tcpRoutes.TryGetValue(tcp.Key, out route))
                                {
                                    Routes.Routes.Remove(route);
                                }

                                lock (_tcpStateNeedLocationInfo)
                                {
                                    bool r = _tcpStateNeedLocationInfo.Remove(tcp.Key);
                                    if (r)
                                    {
                                        Debug.WriteLine("TcpStateNeedLocationInfo: removed " + tcp.Key + " " + r);
                                    }
                                }
                            }
                            else
                            {
                                marker.Position = new PointLatLng(tcp.Value.Latitude, tcp.Value.Longitude);
                                if (!Objects.Markers.Contains(marker))
                                {
                                    Objects.Markers.Add(marker);
                                }
                                UpdateMarkerTcpIpToolTip(marker, tcp.Value, string.Empty);

                                if (_tryTraceConnection)
                                {
                                    // routes
                                    GMapRoute route;
                                    if (!_tcpRoutes.TryGetValue(tcp.Key, out route))
                                    {
                                        lock (_traceRoutes)
                                        {
                                            List<PingReply> tr;
                                            if (_traceRoutes.TryGetValue(tcp.Key, out tr))
                                            {
                                                if (tr != null)
                                                {
                                                    var points = new List<PointLatLng>();
                                                    foreach (var add in tr)
                                                    {
                                                        IpInfo info;

                                                        lock (_tcpTracePoints)
                                                        {
                                                            if (_tcpTracePoints.TryGetValue(add.Address.ToString(), out info))
                                                            {
                                                                if (!string.IsNullOrEmpty(info.Ip))
                                                                {
                                                                    points.Add(new PointLatLng(info.Latitude, info.Longitude));
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (points.Count > 0)
                                                    {
                                                        route = new GMapRoute(points, tcp.Value.CountryName);

                                                        route.Stroke = new Pen(GetRandomColor());
                                                        route.Stroke.Width = 4;
                                                        route.Stroke.DashStyle = DashStyle.DashDot;

                                                        route.Stroke.StartCap = LineCap.NoAnchor;
                                                        route.Stroke.EndCap = LineCap.ArrowAnchor;
                                                        route.Stroke.LineJoin = LineJoin.Round;

                                                        Routes.Routes.Add(route);
                                                        _tcpRoutes[tcp.Key] = route;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!Routes.Routes.Contains(route))
                                        {
                                            Routes.Routes.Add(route);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // update grid data
                    if (panelMenu.Expand && xPanderPanelLive.Expand)
                    {
                        bool empty = _countryStatusView.Count == 0;

                        if (!_comparerIpStatus.SortOnlyCountryName)
                        {
                            _countryStatusView.Clear();
                        }

                        foreach (var c in _countryStatus)
                        {
                            var s = new IpStatus();
                            {
                                s.CountryName = c.Key;
                                s.ConnectionsCount = c.Value;
                            }

                            if (_comparerIpStatus.SortOnlyCountryName)
                            {
                                int idx = _countryStatusView.FindIndex(p => p.CountryName == c.Key);
                                if (idx >= 0)
                                {
                                    _countryStatusView[idx] = s;
                                }
                            }
                            else
                            {
                                _countryStatusView.Add(s);
                            }
                        }

                        _countryStatusView.Sort(_comparerIpStatus);

                        GridConnections.RowCount = _countryStatusView.Count;
                        GridConnections.Refresh();

                        if (empty)
                        {
                            GridConnections.ClearSelection();
                        }
                    }
                }

                MainMap.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        void GridConnections_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= _countryStatusView.Count)
                return;

            var val = _countryStatusView[e.RowIndex];

            switch (GridConnections.Columns[e.ColumnIndex].Name)
            {
                case "CountryName":
                    e.Value = val.CountryName;
                    break;

                case "ConnectionsCount":
                    e.Value = val.ConnectionsCount;
                    break;
            }
        }

        Color GetRandomColor()
        {
            byte r = Convert.ToByte(_rnd.Next(0, 111));
            byte g = Convert.ToByte(_rnd.Next(0, 111));
            byte b = Convert.ToByte(_rnd.Next(0, 111));

            return Color.FromArgb(144, r, g, b);
        }

        void UpdateMarkerTcpIpToolTip(GMapMarker marker, IpInfo tcp, string info)
        {
            marker.ToolTipText = tcp.State.ToString();

            if (!string.IsNullOrEmpty(tcp.ProcessName))
            {
                marker.ToolTipText += " by " + tcp.ProcessName;
            }

            if (!string.IsNullOrEmpty(tcp.CountryName))
            {
                marker.ToolTipText += "\n" + tcp.CountryName;
            }

            if (!string.IsNullOrEmpty(tcp.City))
            {
                marker.ToolTipText += ", " + tcp.City;
            }
            else
            {
                if (!string.IsNullOrEmpty(tcp.RegionName))
                {
                    marker.ToolTipText += ", " + tcp.RegionName;
                }
            }

            marker.ToolTipText += "\n" + tcp.Ip + ":" + tcp.Port + "\n" + info;
        }

        List<IpInfo> GetIpHostInfo(string iplist)
        {
            var ret = new List<IpInfo>();
            bool retry = false;

            string iplistNew = string.Empty;

            var ips = iplist.Split(',');
            foreach (string ip in ips)
            {
#if SQLite
                var val = _ipCache.GetDataFromCache(ip);
#else
                IpInfo val = null;
#endif
                if (val != null)
                {
                    ret.Add(val);
                }
                else
                {
                    if (iplistNew.Length > 0)
                    {
                        iplistNew += ",";
                    }
                    iplistNew += ip;
                }
            }

            if (!string.IsNullOrEmpty(iplistNew))
            {
                Debug.WriteLine("GetIpHostInfo: " + iplist);

                string reqUrl = string.Format("http://api.ipinfodb.com/v2/ip_query.php?key=fbea1992ab11f7125064590a417a8461ccaf06728798c718dbd2809b31a7a5e0&ip={0}&timezone=false", iplistNew);

                while (true)
                {
                    ret.Clear();
                    try
                    {
                        var httpReq = WebRequest.Create(reqUrl) as HttpWebRequest;
                        {
                            string result;
                            using (var response = httpReq.GetResponse() as HttpWebResponse)
                            {
                                using (var reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8))
                                {
                                    result = reader.ReadToEnd();
                                }

                                response.Close();
                            }

                            var x = new XmlDocument();
                            x.LoadXml(result);

                            var nodes = x.SelectNodes("/Response");
                            foreach (XmlNode node in nodes)
                            {
                                string ip = node.SelectSingleNode("Ip").InnerText;

                                var info = new IpInfo();
                                {
                                    info.Ip = ip;
                                    info.CountryName = node.SelectSingleNode("CountryName").InnerText;
                                    info.RegionName = node.SelectSingleNode("RegionName").InnerText;
                                    info.City = node.SelectSingleNode("City").InnerText;
                                    info.Latitude = double.Parse(node.SelectSingleNode("Latitude").InnerText, CultureInfo.InvariantCulture);
                                    info.Longitude = double.Parse(node.SelectSingleNode("Longitude").InnerText, CultureInfo.InvariantCulture);
                                    info.CacheTime = DateTime.Now;

                                    ret.Add(info);
                                }

#if SQLite
                                _ipCache.PutDataToCache(ip, info);
#endif
                            }
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        if (retry) // fail in retry too, full stop ;}
                        {
                            break;
                        }
                        retry = true;
                        reqUrl = string.Format("http://backup.ipinfodb.com/v2/ip_query.php?key=fbea1992ab11f7125064590a417a8461ccaf06728798c718dbd2809b31a7a5e0&ip={0}&timezone=false", iplist);
                        Debug.WriteLine("GetIpHostInfo: " + ex.Message + ", retry on backup server...");
                    }
                }
            }
            return ret;
        }

        #endregion

        #region -- some functions --

        void RegeneratePolygon()
        {
            var polygonPoints = new List<PointLatLng>();

            foreach (var m in Objects.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    m.Tag = polygonPoints.Count;
                    polygonPoints.Add(m.Position);
                }
            }

            if (_polygon == null)
            {
                _polygon = new GMapPolygon(polygonPoints, "polygon test");
                _polygon.IsHitTestVisible = true;
                Polygons.Polygons.Add(_polygon);
            }
            else
            {
                _polygon.Points.Clear();
                _polygon.Points.AddRange(polygonPoints);

                if (Polygons.Polygons.Count == 0)
                {
                    Polygons.Polygons.Add(_polygon);
                }
                else
                {
                    MainMap.UpdatePolygonLocalPosition(_polygon);
                }
            }
        }

        // load mobile gps log
        void AddGpsMobileLogRoutes(string file)
        {
            try
            {
                MainMap.HoldInvalidation = true;

                DateTime? date = null;
                DateTime? dateEnd = null;

                if (MobileLogFrom.Checked)
                {
                    date = MobileLogFrom.Value.ToUniversalTime();
                }

                if (MobileLogTo.Checked)
                {
                    dateEnd = MobileLogTo.Value.ToUniversalTime();
                }

                var log = Stuff.GetRoutesFromMobileLog(file, date, dateEnd, 10);

                if (Routes != null)
                {
                    var track = new List<PointLatLng>();

                    var sessions = new List<List<GpsLog>>(log);

                    var lastPoint = PointLatLng.Empty;

                    foreach (var session in sessions)
                    {
                        // connect to last session with direct line
                        if (!lastPoint.IsEmpty && session.Count > 0)
                        {
                            track.Clear();
                            track.Add(lastPoint);
                            track.Add(session[0].Position);

                            var grl = new GMapRoute(track, "");
                            grl.Stroke = new Pen(GMapRoute.DefaultStroke.Brush);
                            grl.Stroke.Color = Color.Red;
                            grl.Stroke.Width = 2.0f;
                            Routes.Routes.Add(grl);
                        }

                        track.Clear();

                        foreach (var point in session)
                        {
                            track.Add(point.Position);
                        }

                        if (track.Count > 0)
                        {
                            lastPoint = track[track.Count - 1];

                            var gr = new GMapRoute(track, "");
                            Routes.Routes.Add(gr);
                        }
                        else
                        {
                            lastPoint = PointLatLng.Empty;
                        }
                    }

                    sessions.Clear();

                    track.Clear();
                }

                MainMap.Refresh();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddGpsMobileLogRoutes: " + ex.ToString());
            }
        }

        /// <summary>
        /// adds marker using geocoder
        /// </summary>
        /// <param name="place"></param>
        void AddLocationLithuania(string place)
        {
            GeoCoderStatusCode status;
            var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, " + place, out status);
            if (pos != null && status == GeoCoderStatusCode.OK)
            {
                var m = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green);
                m.ToolTip = new GMapRoundedToolTip(m);

                var mBorders = new GMapMarkerRect(pos.Value);
                {
                    mBorders.InnerMarker = m;
                    mBorders.ToolTipText = place;
                    mBorders.ToolTipMode = MarkerTooltipMode.Always;
                }

                Objects.Markers.Add(m);
                Objects.Markers.Add(mBorders);
            }
        }

        bool TryExtractLeafletjs()
        {
            try
            {
                string launch = string.Empty;

                var x = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string f in x)
                {
                    if (f.Contains("leafletjs"))
                    {
                        string fName = f.Replace("Demo.WindowsForms.", string.Empty);
                        fName = fName.Replace(".", "\\");
                        int ll = fName.LastIndexOf("\\");
                        string name = fName.Substring(0, ll) + "." + fName.Substring(ll + 1, fName.Length - ll - 1);

                        //Demo.WindowsForms.leafletjs.dist.leaflet.js

                        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(f))
                        {
                            string fileFullPath = MainMap.CacheLocation + name;

                            if (fileFullPath.Contains("gmap.html"))
                            {
                                launch = fileFullPath;
                            }

                            string dir = Path.GetDirectoryName(fileFullPath);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            using (var fileStream = File.Create(fileFullPath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data
                                var bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);

                                // Use FileStream object to write to the specified file
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(launch))
                {
                    Process.Start(launch);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TryExtractLeafletjs: " + ex);
                return false;
            }
            return true;
        }

        #endregion

        #region -- map events --

        void OnTileCacheComplete()
        {
            Debug.WriteLine("OnTileCacheComplete");
            long size = 0;
            int db = 0;
            try
            {
                var di = new DirectoryInfo(MainMap.CacheLocation);
                var dbs = di.GetFiles("*.gmdb", SearchOption.AllDirectories);
                foreach (var d in dbs)
                {
                    size += d.Length;
                    db++;
                }
            }
            catch
            {
            }

            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    textBoxCacheSize.Text = string.Format(CultureInfo.InvariantCulture, "{0} db in {1:00} MB", db, size / (1024.0 * 1024.0));
                    textBoxCacheStatus.Text = "all tiles saved!";
                };

                if (!IsDisposed)
                {
                    try
                    {
                        Invoke(m);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        void OnTileCacheStart()
        {
            Debug.WriteLine("OnTileCacheStart");

            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    textBoxCacheStatus.Text = "saving tiles...";
                };
                Invoke(m);
            }
        }

        void OnTileCacheProgress(int left)
        {
            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    textBoxCacheStatus.Text = left + " tile to save...";
                };
                Invoke(m);
            }
        }

        void MainMap_OnMarkerLeave(GMapMarker item)
        {
            if (item is GMapMarkerRect)
            {
                _curentRectMarker = null;

                var rc = item as GMapMarkerRect;
                rc.Pen.Color = Color.Blue;

                Debug.WriteLine("OnMarkerLeave: " + item.Position);
            }
        }

        void MainMap_OnMarkerEnter(GMapMarker item)
        {
            if (item is GMapMarkerRect)
            {
                var rc = item as GMapMarkerRect;
                rc.Pen.Color = Color.Red;

                _curentRectMarker = rc;
            }
            Debug.WriteLine("OnMarkerEnter: " + item.Position);
        }

        GMapPolygon _currentPolygon;
        void MainMap_OnPolygonLeave(GMapPolygon item)
        {
            _currentPolygon = null;
            item.Stroke.Color = Color.MidnightBlue;
            Debug.WriteLine("OnPolygonLeave: " + item.Name);
        }

        void MainMap_OnPolygonEnter(GMapPolygon item)
        {
            _currentPolygon = item;
            item.Stroke.Color = Color.Red;
            Debug.WriteLine("OnPolygonEnter: " + item.Name);
        }

        GMapRoute _currentRoute;
        void MainMap_OnRouteLeave(GMapRoute item)
        {
            _currentRoute = null;
            item.Stroke.Color = Color.MidnightBlue;
            Debug.WriteLine("OnRouteLeave: " + item.Name);
        }

        void MainMap_OnRouteEnter(GMapRoute item)
        {
            _currentRoute = item;
            item.Stroke.Color = Color.Red;
            Debug.WriteLine("OnRouteEnter: " + item.Name);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            comboBoxMapType.SelectedItem = type;

            trackBar1.Minimum = MainMap.MinZoom * 100;
            trackBar1.Maximum = MainMap.MaxZoom * 100;

            if (radioButtonFlight.Checked)
            {
                MainMap.ZoomAndCenterMarkers("objects");
            }
        }

        void MainMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = false;
            }
        }

        // add demo circle
        void MainMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cc = new GMapMarkerCircle(MainMap.FromLocalToLatLng(e.X, e.Y));
            Objects.Markers.Add(cc);
        }

        void MainMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;

                if (_currentMarker.IsVisible)
                {
                    _currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);

                    var px = MainMap.MapProvider.Projection.FromLatLngToPixel(_currentMarker.Position.Lat, _currentMarker.Position.Lng, (int)MainMap.Zoom);
                    var tile = MainMap.MapProvider.Projection.FromPixelToTileXY(px);

                    Debug.WriteLine("MouseDown: geo: " + _currentMarker.Position + " | px: " + px + " | tile: " + tile);
                }
            }
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isMouseDown)
            {
                if (_curentRectMarker == null)
                {
                    if (_currentMarker.IsVisible)
                    {
                        _currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
                    }
                }
                else // move rect marker
                {
                    var pnew = MainMap.FromLocalToLatLng(e.X, e.Y);

                    var pIndex = (int?)_curentRectMarker.Tag;
                    if (pIndex.HasValue)
                    {
                        if (pIndex < _polygon.Points.Count)
                        {
                            _polygon.Points[pIndex.Value] = pnew;
                            MainMap.UpdatePolygonLocalPosition(_polygon);
                        }
                    }

                    if (_currentMarker.IsVisible)
                    {
                        _currentMarker.Position = pnew;
                    }
                    _curentRectMarker.Position = pnew;

                    if (_curentRectMarker.InnerMarker != null)
                    {
                        _curentRectMarker.InnerMarker.Position = pnew;
                    }
                }

                MainMap.Refresh(); // force instant invalidation
            }
        }

        // MapZoomChanged
        void MainMap_OnMapZoomChanged()
        {
            trackBar1.Value = (int)(MainMap.Zoom * 100.0);
            textBoxZoomCurrent.Text = MainMap.Zoom.ToString();
        }

        // click on some marker
        void MainMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (item is GMapMarkerRect)
                {
                    GeoCoderStatusCode status;
                    var pos = GMapProviders.GoogleMap.GetPlacemark(item.Position, out status);
                    if (status == GeoCoderStatusCode.OK && pos != null)
                    {
                        var v = item as GMapMarkerRect;
                        {
                            v.ToolTipText = pos.Value.Address;
                        }
                        MainMap.Invalidate(false);
                    }
                }
                else
                {
                    if (item.Tag != null)
                    {
                        if (_currentTransport != null)
                        {
                            _currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                            _currentTransport = null;
                        }
                        _currentTransport = item;
                        _currentTransport.ToolTipMode = MarkerTooltipMode.Always;
                    }
                }
            }
        }

        // loader start loading tiles
        void MainMap_OnTileLoadStart()
        {
            MethodInvoker m = delegate ()
            {
                panelMenu.Text = "Menu: loading tiles...";
            };
            try
            {
                BeginInvoke(m);
            }
            catch
            {
            }
        }

        // loader end loading tiles
        void MainMap_OnTileLoadComplete(long elapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = elapsedMilliseconds;

            MethodInvoker m = delegate ()
            {
                panelMenu.Text = "Menu, last load in " + MainMap.ElapsedMilliseconds + "ms";

                textBoxMemory.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.00} MB of {1:0.00} MB", MainMap.Manager.MemoryCache.Size, MainMap.Manager.MemoryCache.Capacity);
            };
            try
            {
                BeginInvoke(m);
            }
            catch
            {
            }
        }

        // current point changed
        void MainMap_OnPositionChanged(PointLatLng point)
        {
            textBoxLatCurrent.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
            textBoxLngCurrent.Text = point.Lng.ToString(CultureInfo.InvariantCulture);

            lock (_flights)
            {
                _lastPosition = point;
                _lastZoom = (int)MainMap.Zoom;
            }
        }

        PointLatLng _lastPosition;
        int _lastZoom;

        // center markers on start
        private void MainForm_Load(object sender, EventArgs e)
        {
            trackBar1.Value = (int)MainMap.Zoom * 100;
            Activate();
            TopMost = true;
            TopMost = false;
        }
        #endregion

        #region -- menu panels expanders --
        private void xPanderPanel1_Click(object sender, EventArgs e)
        {
            xPanderPanelList1.Expand(xPanderPanelMain);
        }

        private void xPanderPanelCache_Click(object sender, EventArgs e)
        {
            xPanderPanelList1.Expand(xPanderPanelCache);
        }

        private void xPanderPanelLive_Click(object sender, EventArgs e)
        {
            xPanderPanelList1.Expand(xPanderPanelLive);
        }

        private void xPanderPanelInfo_Click(object sender, EventArgs e)
        {
            xPanderPanelList1.Expand(xPanderPanelInfo);
        }
        #endregion

        #region -- ui events --

        bool _userAcceptedLicenseOnce;

        // change map type
        private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
        {
            if (!_userAcceptedLicenseOnce)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
                {
                    string txt = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
                    
                    var d = new Demo.WindowsForms.Forms.Message();
                    d.richTextBox1.Text = txt;

                    if (DialogResult.Yes == d.ShowDialog())
                    {
                        _userAcceptedLicenseOnce = true;
                        Text += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                    }
                }
                else
                {
                    // user deleted License.txt ;}
                    _userAcceptedLicenseOnce = true;
                }
            }

            if (_userAcceptedLicenseOnce)
            {
                MainMap.MapProvider = comboBoxMapType.SelectedItem as GMapProvider;
            }
            else
            {
                MainMap.MapProvider = GMapProviders.OpenStreetMap;
                comboBoxMapType.SelectedItem = MainMap.MapProvider;
            }
        }

        // change mdoe
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedValue;
            MainMap.ReloadMap();
        }

        // zoom
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            MainMap.Zoom = trackBar1.Value / 100.0;
        }



        // goto by geocoder
        private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Enter)
            {
                var status = MainMap.SetPositionByKeywords(textBoxGeo.Text);

                if (status != GeoCoderStatusCode.OK)
                {
                    MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        // go to
        private void button8_Click(object sender, EventArgs e)
        {
            var status = MainMap.SetPositionByKeywords(textBoxGeo.Text);

            if (status != GeoCoderStatusCode.OK)
            {
                MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // reload map
        private void button1_Click(object sender, EventArgs e)
        {
            MainMap.ReloadMap();
        }

        // cache config
        private void checkBoxUseCache_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.Manager.UseRouteCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UseGeocoderCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UsePlacemarkCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UseDirectionsCache = checkBoxUseRouteCache.Checked;
        }

        // clear cache
        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                try
                {
                    MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    MessageBox.Show("Done. Cache is clear.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // add test route
        private void button3_Click(object sender, EventArgs e)
        {
            var rp = MainMap.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.OpenStreetMap; // use OpenStreetMap if provider does not implement routing
            }

            var route = rp.GetRoute(_start, _end, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                // add route
                var r = new GMapRoute(route.Points, route.Name);
                r.IsHitTestVisible = true;
                Routes.Routes.Add(r);

                // add route start/end marks
                GMapMarker m1 = new GMarkerGoogle(_start, GMarkerGoogleType.green_big_go);
                m1.ToolTipText = "Start: " + route.Name;
                m1.ToolTipMode = MarkerTooltipMode.Always;

                GMapMarker m2 = new GMarkerGoogle(_end, GMarkerGoogleType.red_big_stop);
                m2.ToolTipText = "End: " + _end.ToString();
                m2.ToolTipMode = MarkerTooltipMode.Always;

                Objects.Markers.Add(m1);
                Objects.Markers.Add(m2);

                MainMap.ZoomAndCenterRoute(r);
            }
        }

        // add marker on current position
        private void button4_Click(object sender, EventArgs e)
        {
            var m = new GMarkerGoogle(_currentMarker.Position, GMarkerGoogleType.green_pushpin);
            var mBorders = new GMapMarkerRect(_currentMarker.Position);
            {
                mBorders.InnerMarker = m;
                if (_polygon != null)
                {
                    mBorders.Tag = _polygon.Points.Count;
                }
                mBorders.ToolTipMode = MarkerTooltipMode.Always;
            }

            Placemark? p = null;
            if (checkBoxPlacemarkInfo.Checked)
            {
                GeoCoderStatusCode status;
                var ret = GMapProviders.GoogleMap.GetPlacemark(_currentMarker.Position, out status);
                if (status == GeoCoderStatusCode.OK && ret != null)
                {
                    p = ret;
                }
            }

            if (p != null)
            {
                mBorders.ToolTipText = p.Value.Address;
            }
            else
            {
                mBorders.ToolTipText = _currentMarker.Position.ToString();
            }

            Objects.Markers.Add(m);
            Objects.Markers.Add(mBorders);

            RegeneratePolygon();
        }

        // clear routes
        private void button6_Click(object sender, EventArgs e)
        {
            Routes.Routes.Clear();
        }

        // clear polygons
        private void button15_Click(object sender, EventArgs e)
        {
            Polygons.Polygons.Clear();
        }

        // clear markers
        private void button5_Click(object sender, EventArgs e)
        {
            Objects.Markers.Clear();
        }

        // show current marker
        private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
        {
            _currentMarker.IsVisible = checkBoxCurrentMarker.Checked;
        }

        // can drag
        private void checkBoxCanDrag_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.CanDragMap = checkBoxCanDrag.Checked;
        }

        // set route start
        private void buttonSetStart_Click(object sender, EventArgs e)
        {
            _start = _currentMarker.Position;
        }

        // set route end
        private void buttonSetEnd_Click(object sender, EventArgs e)
        {
            _end = _currentMarker.Position;
        }

        // zoom to max for markers
        private void button7_Click(object sender, EventArgs e)
        {
            MainMap.ZoomAndCenterMarkers("objects");
        }

        // export map data
        private void button9_Click(object sender, EventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import map data
        private void button10_Click(object sender, EventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // prefetch
        private void button11_Click(object sender, EventArgs e)
        {
            var area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    var res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButtons.YesNoCancel);

                    if (res == DialogResult.Yes)
                    {
                        using (var obj = new TilePrefetcher())
                        {
                            obj.Overlay = Objects; // set overlay if you want to see cache progress on the map

                            obj.Shuffle = MainMap.Manager.Mode != AccessMode.CacheOnly;

                            obj.Owner = this;
                            obj.ShowCompleteMessage = true;
                            obj.Start(area, i, MainMap.MapProvider, MainMap.Manager.Mode == AccessMode.CacheOnly ? 0 : 100, MainMap.Manager.Mode == AccessMode.CacheOnly ? 0 : 1);
                        }
                    }
                    else if (res == DialogResult.No)
                    {
                        continue;
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // saves current map view 
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PNG (*.png)|*.png";
                    sfd.FileName = "GMap.NET image";

                    var tmpImage = MainMap.ToImage();
                    if (tmpImage != null)
                    {
                        using (tmpImage)
                        {
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                tmpImage.Save(sfd.FileName);

                                MessageBox.Show("Image saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // debug tile grid
        private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.ShowTileGridLines = checkBoxDebug.Checked;
        }

        // launch static map maker
        private void button13_Click(object sender, EventArgs e)
        {
            var st = new StaticImage(this);
            st.Owner = this;
            st.Show();
        }

        // add gps log from mobile
        private void button14_Click(object sender, EventArgs e)
        {
            using (FileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.DefaultExt = "gpsd";
                dlg.ValidateNames = true;
                dlg.Title = "GMap.NET: open gps log generated in your windows mobile";
                dlg.Filter = "GMap.NET gps log DB files (*.gpsd)|*.gpsd";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Routes.Routes.Clear();

                    _mobileGpsLog = dlg.FileName;

                    // add routes
                    AddGpsMobileLogRoutes(dlg.FileName);

                    if (Routes.Routes.Count > 0)
                    {
                        MainMap.ZoomAndCenterRoutes(null);
                    }
                }
            }
        }

        // key-up events
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            int offset = -22;

            if (e.KeyCode == Keys.Left)
            {
                MainMap.Offset(-offset, 0);
            }
            else if (e.KeyCode == Keys.Right)
            {
                MainMap.Offset(offset, 0);
            }
            else if (e.KeyCode == Keys.Up)
            {
                MainMap.Offset(0, -offset);
            }
            else if (e.KeyCode == Keys.Down)
            {
                MainMap.Offset(0, offset);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_currentPolygon != null)
                {
                    Polygons.Polygons.Remove(_currentPolygon);
                    _currentPolygon = null;
                }

                if (_currentRoute != null)
                {
                    Routes.Routes.Remove(_currentRoute);
                    _currentRoute = null;
                }

                if (_curentRectMarker != null)
                {
                    Objects.Markers.Remove(_curentRectMarker);

                    if (_curentRectMarker.InnerMarker != null)
                    {
                        Objects.Markers.Remove(_curentRectMarker.InnerMarker);
                    }
                    _curentRectMarker = null;

                    RegeneratePolygon();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                MainMap.Bearing = 0;

                if (_currentTransport != null && !MainMap.IsMouseOverMarker)
                {
                    _currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    _currentTransport = null;
                }
            }
        }

        // key-press events
        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (MainMap.Focused)
            {
                if (e.KeyChar == '+')
                {
                    MainMap.Zoom = ((int)MainMap.Zoom) + 1;
                }
                else if (e.KeyChar == '-')
                {
                    MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
                }
                else if (e.KeyChar == 'a')
                {
                    MainMap.Bearing--;
                }
                else if (e.KeyChar == 'z')
                {
                    MainMap.Bearing++;
                }
            }
        }

        private void buttonZoomUp_Click(object sender, EventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
        }

        private void buttonZoomDown_Click(object sender, EventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
        }

        // engage some live demo
        private void RealTimeChanged(object sender, EventArgs e)
        {
            Objects.Markers.Clear();
            Polygons.Polygons.Clear();
            Routes.Routes.Clear();

            // start performance test
            if (radioButtonPerf.Checked)
            {
                _timerPerf.Interval = 44;
                _timerPerf.Start();
            }
            else
            {
                // stop performance test
                _timerPerf.Stop();
            }

            // start realtime transport tracking demo
            if (radioButtonFlight.Checked)
            {
                if (!_flightWorker.IsBusy)
                {
                    _firstLoadFlight = true;
                    _flightWorker.RunWorkerAsync();
                }
            }
            else
            {
                if (_flightWorker.IsBusy)
                {
                    _flightWorker.CancelAsync();
                }
            }

            // vehicle demo
            if (radioButtonVehicle.Checked)
            {
                if (!_transportWorker.IsBusy)
                {
                    _firstLoadTrasport = true;
                    _transportWorker.RunWorkerAsync();
                }
            }
            else
            {
                if (_transportWorker.IsBusy)
                {
                    _transportWorker.CancelAsync();
                }
            }

            // start live tcp/ip connections demo
            if (radioButtonTcpIp.Checked)
            {
                GridConnections.Visible = true;
                checkBoxTcpIpSnap.Visible = true;
                checkBoxTraceRoute.Visible = true;
                GridConnections.Refresh();

                if (!_connectionsWorker.IsBusy)
                {
                    MainMap.Zoom = 5;

                    _connectionsWorker.RunWorkerAsync();
                }
            }
            else
            {
                _countryStatusView.Clear();
                GridConnections.Visible = false;
                checkBoxTcpIpSnap.Visible = false;
                checkBoxTraceRoute.Visible = false;

                if (_connectionsWorker.IsBusy)
                {
                    _connectionsWorker.CancelAsync();
                }

                if (_ipInfoSearchWorker.IsBusy)
                {
                    _ipInfoSearchWorker.CancelAsync();
                }

                if (_iptracerWorker.IsBusy)
                {
                    _iptracerWorker.CancelAsync();
                }
            }
        }

        // export mobile gps log to gpx file
        private void buttonExportToGpx_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "GPX (*.gpx)|*.gpx";
                    sfd.FileName = "mobile gps log";

                    DateTime? date = null;
                    DateTime? dateEnd = null;

                    if (MobileLogFrom.Checked)
                    {
                        date = MobileLogFrom.Value.ToUniversalTime();

                        sfd.FileName += " from " + MobileLogFrom.Value.ToString("yyyy-MM-dd HH-mm");
                    }

                    if (MobileLogTo.Checked)
                    {
                        dateEnd = MobileLogTo.Value.ToUniversalTime();

                        sfd.FileName += " to " + MobileLogTo.Value.ToString("yyyy-MM-dd HH-mm");
                    }

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var log = Stuff.GetRoutesFromMobileLog(_mobileGpsLog, date, dateEnd, 3.3);
                        if (log != null)
                        {
                            if (MainMap.Manager.ExportGPX(log, sfd.FileName))
                            {
                                MessageBox.Show("GPX saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GPX failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // load gpx file
        private void button16_Click(object sender, EventArgs e)
        {
            using (FileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.DefaultExt = "gpx";
                dlg.ValidateNames = true;
                dlg.Title = "GMap.NET: open gpx log";
                dlg.Filter = "gpx files (*.gpx)|*.gpx";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string gpx = File.ReadAllText(dlg.FileName);

                        var r = MainMap.Manager.DeserializeGPX(gpx);
                        if (r != null)
                        {
                            if (r.trk.Length > 0)
                            {
                                foreach (var trk in r.trk)
                                {
                                    var points = new List<PointLatLng>();

                                    foreach (var seg in trk.trkseg)
                                    {
                                        foreach (var p in seg.trkpt)
                                        {
                                            points.Add(new PointLatLng((double)p.lat, (double)p.lon));
                                        }
                                    }

                                    var rt = new GMapRoute(points, string.Empty);
                                    {
                                        rt.Stroke = new Pen(Color.FromArgb(144, Color.Red));
                                        rt.Stroke.Width = 5;
                                        rt.Stroke.DashStyle = DashStyle.DashDot;
                                    }
                                    Routes.Routes.Add(rt);
                                }

                                MainMap.ZoomAndCenterRoutes(null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("GPX import: " + ex.ToString());
                        MessageBox.Show("Error importing gpx: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        // enable/disable host tracing
        private void checkBoxTraceRoute_CheckedChanged(object sender, EventArgs e)
        {
            _tryTraceConnection = checkBoxTraceRoute.Checked;
            if (!_tryTraceConnection)
            {
                if (_iptracerWorker.IsBusy)
                {
                    _iptracerWorker.CancelAsync();
                }
                Routes.Routes.Clear();
            }
        }

        private void GridConnections_DoubleClick(object sender, EventArgs e)
        {
            GridConnections.ClearSelection();
        }

        // open disk cache location
        private void button17_Click(object sender, EventArgs e)
        {
            try
            {
                string argument = "/select, \"" + MainMap.CacheLocation + "TileDBv5\"";
                Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // http://leafletjs.com/
        // Leaflet is a modern open-source JavaScript library for mobile-friendly interactive maps
        // Leaflet is designed with simplicity, performance and usability in mind. It works efficiently across all major desktop and mobile platforms out of the box
        private void checkBoxTileHost_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTileHost.Checked)
            {
                try
                {
                    MainMap.Manager.EnableTileHost(8844);
                    TryExtractLeafletjs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("EnableTileHost: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MainMap.Manager.DisableTileHost();
            }
        }

        #endregion

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                string pol1 = "wjcmIoz|xC[\\WTcI`JiDvDmDxDcAlAKPGHg@t@qDfEq@v@yBhCa@d@{BbCWZkDnEeDzDkEzE}BnCyBfCsKzNoCtD}@jAgCnCSRONCBKJ_C|BiFrEuAnAwCrCGHGHkAnAaAbAg@h@GHQRk@n@mBnByAnA_AhAm@z@_CbCEDONOLEFo@n@oAnA{GrGuBnBq@l@YV{FbFoAjAGH{@~@gEjE_NhMyKfKeB`BcJpIgCdC}C`D_@\\uFpF}G~GWTyN~NyDzD{P`Qy@|@uIvI}A|A_@^]\\gDhD}DzDEFoIbIa@`@{FtFyFrF_@^qPdPkFjFoC~CuAvAo@n@g@d@a@^wAtAc@`@_@\\i@f@KLMJaJzIeHzGaC`Cg@h@_@`@yA`B_@`@m@r@eBvB{@fA]`@sGfIsC~D}A|B_AvAGLSXqBfDcEjHuGhLgAnBiArB{@zAmG|KYh@}KxRsBfDc@t@cAbBYh@mDdGeB|C_@r@kEtH[h@Yf@eAdBw@rAeB|CKNABKNoDtFEHsChEcGvHiEjFC@mHbIYXcC|B}AxAeExDwBlB}CpC_DrC]ZqDdDmAbA_@ZkC|BqBjBkAjAo@n@}@~@qGzGgEbFeB~BsBpC]b@_@f@mBvC{CvEsC|EQZ}@bBsAjCaEdIqBnEiBhE}HvQg@dAmDrH{DpHs@lA}D~G{EdIsC`EeDnE}@lAsFxGaJjK]`@a@d@yDnD_@\\cItHuKnJcA~@uF|EONw@l@}EfEaM|K_CtBEFGFIFUREDaAx@UP?@C@MJKJgAbAaBxAgBzAgBhBCB[XcCtB_BvAgGxFeD|C}C~CsBxBsGdHgAlAkIhJ_CpCMPOPyHnJW\\]b@_JnLY\\uHbKyBbDGHINSVsBvCW^kEnG[f@qAjBgLnQ}CjFYf@_E`HgHjMAB[j@gBjDYh@Q\\iBbEuItRq@fBUl@Wn@Qd@Qf@GNGNIRGNAB[z@Od@Qb@i@vAmCrHM^eG|QiEvO]lAkC|JgD|NcFrTyElSo@jCq@vCkBnHSr@}BjIs@fC{BjHCF}BbHEJOd@CFuA|Dm@~AaCnGGPaCdGoCnGq@|As@tAO^wArCkA`CmA~BqFrKqEpHMRMR_@l@a@n@CFCDOVq@dAw@lASXiL~OuCjDwIvJ{FvFqFnFiCjCyOhPEDEBIJIHA@CBEDGFEDaM|LaSfSuJvJcKfKwNtNwMzMyE|EKJ}B|B[\\ED_A|@WV";
                var points = PureProjection.PolylineDecode(pol1);
                string pol2 = PureProjection.PolylineEncode(points);

                if (pol1 == pol2)
                {

                }

                string pol3 = "";

                foreach (var item in points)
                {
                    pol3 += item.Lat + " " + item.Lng + ", ";
                }



                var route = MainMap.RoutingProvider.GetRoute(MainMap.Position, new PointLatLng(54.7261334816182, 25.2985095977783), false, false, 10);

                if (route != null && route.Status == RouteStatusCode.OK)
                {
                    var oRoute = new GMapRoute(route);
                    Routes.Routes.Add(oRoute);

                    //MapRoute Res = MainMap.RoadsProvider.GetRoadsRoute(Route.Points.GetRange(0, 50).ToList(), false);

                    //if (Res.Status == RouteStatusCode.OK)
                    //{
                    //    // Here
                    //}
                }

                //GDirections gd = null;
                //DirectionsStatusCode Res = MainMap.DirectionsProvider.GetDirections(out gd, MainMap.Position, new PointLatLng(54.7261334816182, 25.2985095977783), false, false, false, false, true);

                GeoCoderStatusCode gc;
                var point = MainMap.GeocodingProvider.GetPoint("Barranquilla", out gc);

                //GDirections gd = null;
                //DirectionsStatusCode Res = MainMap.DirectionsProvider.GetDirections(out gd, "Barranquilla", "Santa Marta", false, false, false, false, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
