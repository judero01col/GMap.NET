using System;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class OpenDocumentSensitivePrioritizedFuture : FutureBase, IDisposable
    {
        private OpenDocumentSensitivePrioritizer prioritizer;
        private IFuture future;
        private IFuture openDocumentFuture;
        private bool realizing;
        private static int nextIdentity;
        private static object nextIdentityMutex = new object();
        private AsyncRef activeAsyncRef;

        public int identity
        {
            get;
        }

        public OpenDocumentSensitivePrioritizedFuture(OpenDocumentSensitivePrioritizer prioritizer, IFuture future,
            IFuture openDocumentFuture)
        {
            this.prioritizer = prioritizer;
            this.future = future;
            this.openDocumentFuture = openDocumentFuture;
            object obj;
            Monitor.Enter(obj = nextIdentityMutex);
            try
            {
                identity = nextIdentity;
                nextIdentity++;
            }
            finally
            {
                Monitor.Exit(obj);
            }
        }

        public override Present Realize(string refCredit)
        {
            Monitor.Enter(this);
            Present result;
            try
            {
                if (activeAsyncRef != null)
                {
                    result = activeAsyncRef.Duplicate(refCredit);
                }
                else
                {
                    D.Assert(!realizing);
                    realizing = true;
                    activeAsyncRef = (AsyncRef)future.Realize("ODSPF");
                    AsyncRef asyncRef = (AsyncRef)activeAsyncRef.Duplicate(refCredit);
                    prioritizer.Realizing(this);
                    activeAsyncRef.AddCallback(AsyncCompleteCallback);
                    result = asyncRef;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("ODSPF(");
            future.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }

        private void AsyncCompleteCallback(AsyncRef asyncRef)
        {
            Monitor.Enter(this);
            try
            {
                realizing = false;
                prioritizer.Complete(this);
                activeAsyncRef.Dispose();
                activeAsyncRef = null;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal IFuture GetOpenDocumentFuture()
        {
            return openDocumentFuture;
        }

        internal void DocumentStateChanged(bool isOpen)
        {
            activeAsyncRef.SetInterest(isOpen ? 524291 : 0);
        }

        public void Dispose()
        {
            Monitor.Enter(this);
            try
            {
                if (activeAsyncRef != null)
                {
                    activeAsyncRef.Dispose();
                    activeAsyncRef = null;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
