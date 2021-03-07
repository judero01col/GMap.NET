using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace GMap.NET.ObjectModel
{
    public class ThreadSafeEnumerator<T> : IEnumerator<T>
    {
        // this is the (thread-unsafe)
        // enumerator of the underlying collection
        private readonly IEnumerator<T> m_Inner;
        // this is the object we shall lock on. 
        private readonly object m_Lock;

        public ThreadSafeEnumerator(IEnumerator<T> inner, object @lock)
        {
            m_Inner = inner;
            m_Lock = @lock;
            // entering lock in constructor
            Monitor.Enter(m_Lock);
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // .. and exiting lock on Dispose()
            // This will be called when foreach loop finishes
            Monitor.Exit(m_Lock);
        }

        #endregion

        #region Implementation of IEnumerator

        // we just delegate actual implementation
        // to the inner enumerator, that actually iterates
        // over some collection

        public bool MoveNext()
        {
            return m_Inner.MoveNext();
        }

        public void Reset()
        {
            m_Inner.Reset();
        }

        public T Current
        {
            get { return m_Inner.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
