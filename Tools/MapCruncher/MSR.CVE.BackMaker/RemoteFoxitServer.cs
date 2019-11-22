using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace MSR.CVE.BackMaker
{
    public class RemoteFoxitServer
    {
        private string pipeGuid;
        private FoxitViewer foxitViewer;

        public void ConsumeArgs(List<string> args)
        {
            if (args.Count == 0)
            {
                throw new Exception("Expected PipeGuid argument");
            }

            pipeGuid = args[0];
            args.RemoveAt(0);
        }

        public int Run()
        {
            var namedPipeClient = new NamedPipeClient(pipeGuid);
            bool flag = true;
            while (!flag)
            {
                D.Sayf(0, "Waiting for debugger.", new object[0]);
                Thread.Sleep(250);
            }

            namedPipeClient.RunServer(Server);
            return 0;
        }

        internal bool Server(object genericRequest, ref ISerializable reply)
        {
            if (genericRequest is OpenRequest)
            {
                var openRequest = (OpenRequest)genericRequest;
                if (foxitViewer != null)
                {
                    reply = new ExceptionMessageRecord("Already open");
                    return true;
                }

                try
                {
                    foxitViewer = new FoxitViewer(openRequest.filename, openRequest.pageNumber);
                    reply = new RectangleFRecord(foxitViewer.GetPageSize());
                    bool result = true;
                    return result;
                }
                catch (Exception ex)
                {
                    reply = new ExceptionMessageRecord(ex.Message);
                    bool result = false;
                    return result;
                }
            }

            if (genericRequest is RenderRequest)
            {
                var renderRequest = (RenderRequest)genericRequest;
                if (foxitViewer == null)
                {
                    reply = new ExceptionMessageRecord("Not open");
                    return true;
                }

                try
                {
                    reply = foxitViewer.RenderBytes(renderRequest.outputSize,
                        renderRequest.topLeft,
                        renderRequest.pageSize,
                        renderRequest.transparentBackground);
                    bool result = true;
                    return result;
                }
                catch (Exception ex2)
                {
                    reply = new ExceptionMessageRecord(ex2.Message);
                    bool result = true;
                    return result;
                }
            }

            if (genericRequest is QuitRequest)
            {
                reply = new AckRecord();
                return false;
            }

            reply = new ExceptionMessageRecord("Unrecognized request type " + genericRequest.GetType().ToString());
            return true;
        }
    }
}
