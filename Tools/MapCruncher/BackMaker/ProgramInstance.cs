using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using MSR.CVE.BackMaker;

namespace BackMaker
{
    public class ProgramInstance
    {
        public const int BadArgs = 1;
        private const int BadConfiguration = 2;
        private RemoteFoxitServer _remoteFoxitServer;
        private bool _renderOnLaunch;
        private string _startDocumentPath;
        private static int _applicationResultCode;

        private void ParseArgs(string[] argsArray)
        {
            var list = new List<string>(argsArray);
            while (list.Count > 0)
            {
                if (list[0] == "-?" || list[0] == "/?" || list[0] == "--help" || list[0] == "-h")
                {
                    throw new UsageException();
                }

                if (list[0][0] == '-')
                {
                    if (list[0] == "-render")
                    {
                        _renderOnLaunch = true;
                        list.RemoveAt(0);
                    }
                    else
                    {
                        if (!(list[0] == "-remoteServer"))
                        {
                            throw new Exception(string.Format("Unrecognized flag {0}", list[0]));
                        }

                        _remoteFoxitServer = new RemoteFoxitServer();
                        list.RemoveAt(0);
                        _remoteFoxitServer.ConsumeArgs(list);
                    }
                }
                else
                {
                    _startDocumentPath = list[0];
                    list.RemoveAt(0);
                }
            }
        }

        public int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            DebugThreadInterrupter.theInstance.RegisterThread(Thread.CurrentThread);
            MainAppForm mainAppForm = null;
            try
            {
                ParseArgs(args);
                try
                {
                    BuildConfig.Initialize();
                    if (BuildConfig.theConfig.buildConfiguration == "Broken")
                    {
                        throw new ConfigurationException(
                            "MapCruncher configuration is broken. Please reinstall MapCruncher.");
                    }

                    if (_remoteFoxitServer != null)
                    {
                        int result = _remoteFoxitServer.Run();
                        return result;
                    }

                    Application.EnableVisualStyles();
                    mainAppForm = new MainAppForm(_startDocumentPath, _renderOnLaunch);
                    mainAppForm.StartUpApplication();
                }
                catch (ConfigurationException ex)
                {
                    D.Say(0, ex.ToString());
                    HTMLMessageBox.Show(ex.Message, "MapCruncher Configuration Problem");
                    int result = 2;
                    return result;
                }

                Application.Run(mainAppForm);
            }
            finally
            {
                DebugThreadInterrupter.theInstance.Quit();
                if (mainAppForm != null)
                {
                    mainAppForm.UndoConstruction();
                }
            }

            return _applicationResultCode;
        }

        public static void SetApplicationResultCode(int rc)
        {
            _applicationResultCode = rc;
        }
    }
}
