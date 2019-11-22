using System.Collections.Generic;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class MemoryCache : CacheBase
    {
        private const int paramCachedStuffGoal = 64;
        private LinkedList<CacheRecord> clockQueue;
        private int paramCacheMinSize = 64;
        private int nextCleanMark;
        private ResourceCounter interestingStuffResourceCounter;

        public MemoryCache(string debugName) : base(debugName)
        {
            clockQueue = new LinkedList<CacheRecord>();
            interestingStuffResourceCounter =
                DiagnosticUI.theDiagnostics.fetchResourceCounter("Cache:" + debugName + "-interestingStuff", -1);
        }

        public MemoryCache(string debugName, int paramCacheMinSize) : this(debugName)
        {
            this.paramCacheMinSize = paramCacheMinSize;
        }

        internal override CacheRecord NewRecord(IFuture future)
        {
            return new ClockCacheRecord(future);
        }

        internal override void Touch(CacheRecord record, bool recordIsNew)
        {
            if (recordIsNew)
            {
                clockQueue.AddLast(record);
            }

            ((ClockCacheRecord)record).touched = true;
        }

        internal override void Remove(CacheRecord record, RemoveExpectation removeExpectation)
        {
            base.Remove(record, removeExpectation);
        }

        protected override void Clean()
        {
            int num = 0;
            int num2 = 0;
            Monitor.Enter(this);
            try
            {
                if (cache.Count >= nextCleanMark)
                {
                    bool flag = false;
                    int num3 = 0;
                    for (int i = 0; i < clockQueue.Count; i++)
                    {
                        if (cache.Count < paramCacheMinSize)
                        {
                            flag = true;
                            break;
                        }

                        ClockCacheRecord clockCacheRecord = (ClockCacheRecord)clockQueue.First.Value;
                        clockQueue.RemoveFirst();
                        num++;
                        bool flag2 = clockCacheRecord.present is RequestInterestIfc &&
                                     ((RequestInterestIfc)clockCacheRecord.present).GetInterest() > 524291;
                        if (flag2)
                        {
                            num3++;
                            clockQueue.AddLast(clockCacheRecord);
                        }
                        else
                        {
                            if (clockCacheRecord.touched)
                            {
                                clockCacheRecord.touched = false;
                                clockQueue.AddLast(clockCacheRecord);
                            }
                            else
                            {
                                Remove(clockCacheRecord, RemoveExpectation.Unknown);
                                num2++;
                            }
                        }
                    }

                    if (!flag)
                    {
                        int num4 = num3 * 2 + 64;
                        if (num4 > paramCacheMinSize)
                        {
                            paramCacheMinSize = num4;
                            D.Sayf(0,
                                "Grew cache {0} to paramCacheMinSize={1}; cache now {2}; clock now {3}",
                                new object[]
                                {
                                    hashName, paramCacheMinSize, cache.Count, clockQueue.Count
                                });
                        }
                    }

                    interestingStuffResourceCounter.SetValue(num3);
                    nextCleanMark = cache.Count + (paramCacheMinSize >> 2);
                    D.Sayf(10, "MemoryCache Cleaner: removed {0}/{1}", new object[] {num2, num});
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
