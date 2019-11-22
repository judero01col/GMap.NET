using System.Collections.Generic;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public abstract class CacheBase : IRobustlyHashable
    {
        public enum RemoveExpectation
        {
            Absent,
            Present,
            Unknown
        }

        internal string hashName;
        internal Dictionary<IFuture, CacheRecord> cache;
        private ResourceCounter resourceCounter;

        public CacheBase(string hashName)
        {
            this.hashName = hashName;
            cache = new Dictionary<IFuture, CacheRecord>();
            resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("Cache:" + hashName, -1);
        }

        public void Dispose()
        {
            Dictionary<IFuture, CacheRecord> dictionary = cache;
            cache = new Dictionary<IFuture, CacheRecord>();
            foreach (CacheRecord current in dictionary.Values)
            {
                Remove(current, RemoveExpectation.Absent);
            }
        }

        internal abstract void Touch(CacheRecord record, bool recordIsNew);

        internal virtual void TouchAfterUnlocked(CacheRecord record, bool recordIsNew)
        {
        }

        protected abstract void Clean();
        internal abstract CacheRecord NewRecord(IFuture future);

        internal virtual void Remove(CacheRecord record, RemoveExpectation removeExpectation)
        {
            Monitor.Enter(this);
            try
            {
                bool flag = cache.Remove(record.GetFuture());
                D.Assert(removeExpectation == RemoveExpectation.Unknown ||
                         removeExpectation == RemoveExpectation.Present == flag,
                    "Remove didn't meet expectations. That could suggest a mutating hash.");
                resourceCounter.crement(-1);
                record.DropReference();
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public Present Lookup(IFuture future)
        {
            Monitor.Enter(this);
            CacheRecord cacheRecord;
            try
            {
                if (!cache.ContainsKey(future))
                {
                    return null;
                }

                cacheRecord = cache[future];
                Touch(cacheRecord, false);
                cacheRecord.AddReference();
            }
            finally
            {
                Monitor.Exit(this);
            }

            Present result = cacheRecord.WaitResult("lookup", hashName);
            cacheRecord.DropReference();
            return result;
        }

        public bool Contains(IFuture future)
        {
            Monitor.Enter(this);
            bool result;
            try
            {
                if (cache.ContainsKey(future))
                {
                    Touch(cache[future], false);
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        public virtual Present Get(IFuture future, string refCredit)
        {
            bool flag = false;
            bool flag2 = false;
            Present result;
            try
            {
                Clean();
                Monitor.Enter(this);
                CacheRecord cacheRecord;
                bool recordIsNew;
                try
                {
                    if (!cache.ContainsKey(future))
                    {
                        cacheRecord = NewRecord(future);
                        cache[future] = cacheRecord;
                        resourceCounter.crement(1);
                        flag = true;
                        recordIsNew = true;
                    }
                    else
                    {
                        cacheRecord = cache[future];
                        recordIsNew = false;
                    }

                    Touch(cacheRecord, recordIsNew);
                    cacheRecord.AddReference();
                }
                finally
                {
                    Monitor.Exit(this);
                }

                TouchAfterUnlocked(cacheRecord, recordIsNew);
                if (flag)
                {
                    cacheRecord.Process();
                    flag2 = true;
                }

                Present present = cacheRecord.WaitResult(refCredit, hashName);
                cacheRecord.DropReference();
                if (present is IEvictable && ((IEvictable)present).EvictMeNow())
                {
                    D.Sayf(0, "Evicting canceled request for {0}", new object[] {future});
                    Evict(future);
                }

                result = present;
            }
            finally
            {
                D.Assert(!flag || flag2);
            }

            return result;
        }

        internal void Evict(IFuture future)
        {
            Monitor.Enter(this);
            try
            {
                bool flag = cache.ContainsKey(future);
                if (flag)
                {
                    CacheRecord record = cache[future];
                    Remove(record, RemoveExpectation.Unknown);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(string.Format("Cache({0})", hashName));
        }
    }
}
