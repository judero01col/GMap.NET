using System.Collections.Generic;
using System.Threading;

namespace MSR.CVE.BackMaker
{
    internal class BiSortedDictionary<TKey, TValue>
    {
        private class BackwardsComparer : IComparer<TKey>
        {
            private IComparer<TKey> comparer;

            public BackwardsComparer(IComparer<TKey> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(TKey x, TKey y)
            {
                return comparer.Compare(y, x);
            }
        }

        private SortedDictionary<TKey, TValue> forwardDict;
        private SortedDictionary<TKey, TValue> backwardDict;

        public int Count
        {
            get
            {
                return forwardDict.Count;
            }
        }

        public TKey FirstKey
        {
            get
            {
                SortedDictionary<TKey, TValue>.Enumerator enumerator = forwardDict.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    return current.Key;
                }

                return default(TKey);
            }
        }

        public TKey LastKey
        {
            get
            {
                SortedDictionary<TKey, TValue>.Enumerator enumerator = backwardDict.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    KeyValuePair<TKey, TValue> current = enumerator.Current;
                    return current.Key;
                }

                return default(TKey);
            }
        }

        public SortedDictionary<TKey, TValue>.KeyCollection Keys
        {
            get
            {
                return forwardDict.Keys;
            }
        }

        public BiSortedDictionary()
        {
            forwardDict = new SortedDictionary<TKey, TValue>();
            backwardDict = new SortedDictionary<TKey, TValue>(new BackwardsComparer(forwardDict.Comparer));
        }

        public void Add(TKey key, TValue value)
        {
            Monitor.Enter(this);
            try
            {
                forwardDict.Add(key, value);
                backwardDict.Add(key, value);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void Remove(TKey key)
        {
            Monitor.Enter(this);
            try
            {
                forwardDict.Remove(key);
                backwardDict.Remove(key);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public SortedDictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator()
        {
            return forwardDict.Keys.GetEnumerator();
        }
    }
}
