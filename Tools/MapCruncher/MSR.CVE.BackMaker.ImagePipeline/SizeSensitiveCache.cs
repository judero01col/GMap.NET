using System.Collections.Generic;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class SizeSensitiveCache : CacheBase
    {
        public const long paramUnknownObjectSizeWAG = 83886080L;
        private LinkedList<CacheRecord> lruQueue;
        private List<OpenDocumentStateObserverIfc> observers = new List<OpenDocumentStateObserverIfc>();
        private static bool oneEntryAtATime = false;
        private static long paramCacheMaxSize = (oneEntryAtATime ? 1 : 400) * 1048576;
        private long memoryUsed;
        private int spillCount;

        public SizeSensitiveCache(string debugName) : base(debugName)
        {
            lruQueue = new LinkedList<CacheRecord>();
        }

        internal override CacheRecord NewRecord(IFuture future)
        {
            return new SizedCacheRecord(future);
        }

        private void UpdateSizeForRecord(SizedCacheRecord record)
        {
            if (record.knowCorrectSize)
            {
                return;
            }

            memoryUsed -= record.memoryCharge;
            if (record.present != null && record.present is SizedObject)
            {
                record.memoryCharge = ((SizedObject)record.present).GetSize();
            }
            else
            {
                record.memoryCharge = 83886080L;
            }

            memoryUsed += record.memoryCharge;
        }

        internal override void Touch(CacheRecord record, bool recordIsNew)
        {
            if (!recordIsNew)
            {
                lruQueue.Remove(record);
            }

            UpdateSizeForRecord((SizedCacheRecord)record);
            lruQueue.AddLast(record);
        }

        internal override void TouchAfterUnlocked(CacheRecord record, bool recordIsNew)
        {
            if (recordIsNew)
            {
                NotifyObservers(record.GetFuture(), true);
            }
        }

        internal override void Remove(CacheRecord record, RemoveExpectation removeExpectation)
        {
            memoryUsed -= ((SizedCacheRecord)record).memoryCharge;
            lruQueue.Remove(record);
            base.Remove(record, removeExpectation);
        }

        protected override void Clean()
        {
            Clean(false);
        }

        public override Present Get(IFuture future, string refCredit)
        {
            if (oneEntryAtATime)
            {
                Present present = Lookup(future);
                if (present != null)
                {
                    return present;
                }
            }

            return base.Get(future, refCredit);
        }

        private void Clean(bool purge)
        {
            int num = 0;
            long num2 = memoryUsed;
            List<CacheRecord> list = new List<CacheRecord>();
            Monitor.Enter(this);
            try
            {
                while (purge && lruQueue.Count > 0 || memoryUsed > paramCacheMaxSize &&
                       (oneEntryAtATime && lruQueue.Count > 0 || !oneEntryAtATime && lruQueue.Count > 1))
                {
                    CacheRecord value = lruQueue.First.Value;
                    num++;
                    list.Add(value);
                    Remove(value, RemoveExpectation.Present);
                    spillCount++;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            foreach (CacheRecord current in list)
            {
                NotifyObservers(current.GetFuture(), false);
            }

            D.Sayf(10,
                "SizeSensitive Cleaner: removed {0} objects; from {1} to {2} MB",
                new object[] {num, num2 >> 20, memoryUsed >> 20});
        }

        internal int GetSpillCount()
        {
            return spillCount;
        }

        internal void Purge()
        {
            Clean(true);
            spillCount = 0;
        }

        internal void AddCallback(OpenDocumentStateObserverIfc openDocumentStateObserver)
        {
            observers.Add(openDocumentStateObserver);
        }

        private void NotifyObservers(IFuture openDocumentFuture, bool documentState)
        {
            foreach (OpenDocumentStateObserverIfc current in observers)
            {
                current.DocumentStateChanged(openDocumentFuture, documentState);
            }
        }
    }
}
