using System;

namespace MSR.CVE.BackMaker
{
    public class FoxitLibManager
    {
        private static FoxitLibManager _theInstance;
        public Exception loadException = new Exception("unknown exception");

        public static FoxitLibManager theInstance
        {
            get
            {
                if (_theInstance == null)
                {
                    _theInstance = new FoxitLibManager();
                }

                return _theInstance;
            }
        }

        public FoxitLibWorker foxitLib
        {
            get;
        } = new FoxitLibWorker();
    }
}
