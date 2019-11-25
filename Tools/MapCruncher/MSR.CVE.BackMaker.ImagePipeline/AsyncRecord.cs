using System;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class AsyncRecord
    {
        public delegate void CompleteCallback(AsyncRef asyncRef);

        private AsyncScheduler scheduler;
        private AsyncState asyncState;
        private int queuePriority;
        private CompleteCallback callbackEvent;
        private AsyncRef qtpRef;
        private AsyncRef notificationRef;
        private int refs;
        private bool disposed;

        public IFuture cacheKeyToEvict
        {
            get;
        }

        public IFuture future
        {
            get;
        }

        public Present present
        {
            get;
            private set;
        }

        public AsyncRecord(AsyncScheduler scheduler, IFuture cacheKeyToEvict, IFuture future)
        {
            this.cacheKeyToEvict = cacheKeyToEvict;
            this.future = future;
            this.scheduler = scheduler;
            present = null;
            asyncState = AsyncState.Prequeued;
            queuePriority = 0;
            qtpRef = new AsyncRef(this, "qRef");
        }

        public void AddCallback(CompleteCallback callback)
        {
            Monitor.Enter(this);
            try
            {
                if (present != null)
                {
                    AsyncRef asyncRef = new AsyncRef(this, "AsyncRecord.AddCallback");
                    callback(asyncRef);
                    asyncRef.Dispose();
                }
                else
                {
                    callbackEvent = (CompleteCallback)Delegate.Combine(callbackEvent, callback);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void Dispose()
        {
            D.Sayf(10, "Disposed({0})", new object[] {this});
            present.Dispose();
        }

        public void ChangePriority(int crement)
        {
            queuePriority += crement;
            if (qtpRef != null)
            {
                scheduler.ChangePriority(qtpRef);
            }
        }

        public AsyncRef GetQTPRef()
        {
            return qtpRef;
        }

        public void AddRef()
        {
            Monitor.Enter(this);
            try
            {
                D.Assert(!disposed);
                refs++;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void DropRef()
        {
            Monitor.Enter(this);
            try
            {
                D.Assert(!disposed);
                refs--;
                if (refs == 0)
                {
                    Dispose();
                    disposed = true;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal bool PrepareToQueue()
        {
            Monitor.Enter(this);
            bool result;
            try
            {
                if (asyncState != AsyncState.Prequeued)
                {
                    result = false;
                }
                else
                {
                    asyncState = AsyncState.Queued;
                    result = true;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        internal int GetPriority()
        {
            return queuePriority;
        }

        internal AsyncScheduler GetScheduler()
        {
            return scheduler;
        }

        internal void DoWork()
        {
            D.Assert(present == null);
            Present presentValue;
            try
            {
                presentValue = future.Realize("AsyncRecord.DoWork");
            }
            catch (Exception ex)
            {
                presentValue = new PresentFailureCode(ex);
            }

            Notify(presentValue);
        }

        internal void DeQueued()
        {
            D.Say(10, string.Format("DeQueued({0})", this));
            Notify(new RequestCanceledPresent());
        }

        private void Notify(Present presentValue)
        {
            Monitor.Enter(this);
            try
            {
                D.Assert(present == null);
                present = presentValue;
                notificationRef = new AsyncRef(this, "callback");
                qtpRef = null;
            }
            finally
            {
                Monitor.Exit(this);
            }

            DebugThreadInterrupter.theInstance.AddThread("AsyncRecord.NotifyThread",
                NotifyThread,
                ThreadPriority.Normal);
        }

        private void NotifyThread()
        {
            callbackEvent(notificationRef);
            notificationRef.Dispose();
        }

        public override string ToString()
        {
            return "AsyncRecord:" + RobustHashTools.DebugString(future);
        }

        internal void ProcessSynchronously()
        {
            Monitor.Enter(this);
            try
            {
                if (asyncState == AsyncState.Queued)
                {
                    return;
                }

                D.Assert(asyncState == AsyncState.Prequeued);
                asyncState = AsyncState.Queued;
            }
            finally
            {
                Monitor.Exit(this);
            }

            DoWork();
        }
    }
}
