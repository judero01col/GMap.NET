using System;
using System.Collections.Generic;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class AsyncScheduler : IDisposable
    {
        private MemoryCache asyncRecordCache;
        private QueuedTileProvider qtp;

        public AsyncScheduler(int numWorkerThreads, string debugName)
        {
            asyncRecordCache = new AsyncRecordCache(debugName + "-Coalesce", false);
            qtp = new QueuedTileProvider(numWorkerThreads, debugName);
        }

        public void Dispose()
        {
            qtp.Dispose();
        }

        internal void Activate(LinkedList<AsyncRef> refs)
        {
            Monitor.Enter(this);
            try
            {
                List<QueueRequestIfc> list = new List<QueueRequestIfc>();
                foreach (AsyncRef current in refs)
                {
                    if (current.asyncRecord.PrepareToQueue())
                    {
                        current.asyncRecord.AddCallback(EvictFromCache);
                        list.Add(current.asyncRecord.GetQTPRef());
                    }
                }

                qtp.enqueueTileRequests(list.ToArray());
                D.Sayf(10, "PriQueue: {0}", new object[] {qtp});
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal MemoryCache GetCache()
        {
            return asyncRecordCache;
        }

        internal void EvictFromCache(AsyncRef aref)
        {
            asyncRecordCache.Evict(aref.asyncRecord.cacheKeyToEvict);
        }

        internal void ChangePriority(AsyncRef asyncRef)
        {
            qtp.ChangePriority(asyncRef);
        }

        public void Clear()
        {
            qtp.Clear();
        }
    }
}
