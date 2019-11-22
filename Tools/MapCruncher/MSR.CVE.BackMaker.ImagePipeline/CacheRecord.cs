using System;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal abstract class CacheRecord
    {
        private IFuture future;
        private EventWaitHandle wait;
        internal Present _present;
        private int refs;

        private static ResourceCounter cacheRecordsCompletedResourceCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("cacheRecordsCompleted", -1);

        private static ResourceCounter cacheRecordsExtant =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("cacheRecordsExtant", -1);

        internal Present present
        {
            get
            {
                return _present;
            }
            set
            {
                int num = _present != null ? 1 : 0;
                int num2 = value != null ? 1 : 0;
                cacheRecordsCompletedResourceCounter.crement(num2 - num);
                _present = value;
            }
        }

        public CacheRecord(IFuture future)
        {
            this.future = future;
            wait = new CountedEventWaitHandle(false, EventResetMode.ManualReset, "CacheRecord.Wait");
            refs = 1;
            cacheRecordsExtant.crement(1);
        }

        public void Dispose()
        {
            if (present != null)
            {
                present.Dispose();
                present = null;
            }

            Monitor.Enter(this);
            try
            {
                if (wait != null)
                {
                    wait.Close();
                    wait = null;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            cacheRecordsExtant.crement(-1);
        }

        public void Process()
        {
            try
            {
                present = future.Realize("CacheRecord.Process");
            }
            catch (Exception ex)
            {
                present = new PresentFailureCode(ex);
            }

            D.Assert(present != null);
            Monitor.Enter(this);
            try
            {
                wait.Set();
                wait.Close();
                wait = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public Present WaitResult(string refCredit, string debugCacheName)
        {
            Monitor.Enter(this);
            WaitHandle waitHandle;
            try
            {
                waitHandle = wait;
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (waitHandle != null)
            {
                waitHandle.WaitOne();
            }

            return Duplicate(refCredit);
        }

        public Present Duplicate(string refCredit)
        {
            return present.Duplicate(refCredit);
        }

        internal IFuture GetFuture()
        {
            return future;
        }

        public override string ToString()
        {
            if (present != null)
            {
                return "CacheRecord(present)";
            }

            return "CacheRecord(processing)";
        }

        internal void AddReference()
        {
            Monitor.Enter(this);
            try
            {
                refs++;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal void DropReference()
        {
            Monitor.Enter(this);
            bool flag;
            try
            {
                refs--;
                flag = refs == 0;
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (flag)
            {
                Dispose();
            }
        }
    }
}
