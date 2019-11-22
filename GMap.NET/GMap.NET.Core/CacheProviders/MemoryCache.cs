using System;
using System.Diagnostics;
using GMap.NET.Internals;

namespace GMap.NET.CacheProviders
{
    public class MemoryCache : IDisposable
    {
        private readonly KiberTileCache _tilesInMemory = new KiberTileCache();

        private FastReaderWriterLock _kiberCacheLock = new FastReaderWriterLock();

        /// <summary>
        ///     the amount of tiles in MB to keep in memmory, default: 22MB, if each ~100Kb it's ~222 tiles
        /// </summary>
        public int Capacity
        {
            get
            {
                _kiberCacheLock.AcquireReaderLock();
                try
                {
                    return _tilesInMemory.MemoryCacheCapacity;
                }
                finally
                {
                    _kiberCacheLock.ReleaseReaderLock();
                }
            }
            set
            {
                _kiberCacheLock.AcquireWriterLock();
                try
                {
                    _tilesInMemory.MemoryCacheCapacity = value;
                }
                finally
                {
                    _kiberCacheLock.ReleaseWriterLock();
                }
            }
        }

        /// <summary>
        ///     current memmory cache size in MB
        /// </summary>
        public double Size
        {
            get
            {
                _kiberCacheLock.AcquireReaderLock();
                try
                {
                    return _tilesInMemory.MemoryCacheSize;
                }
                finally
                {
                    _kiberCacheLock.ReleaseReaderLock();
                }
            }
        }

        public void Clear()
        {
            _kiberCacheLock.AcquireWriterLock();
            try
            {
                _tilesInMemory.Clear();
            }
            finally
            {
                _kiberCacheLock.ReleaseWriterLock();
            }
        }

        // ...

        internal byte[] GetTileFromMemoryCache(RawTile tile)
        {
            _kiberCacheLock.AcquireReaderLock();
            try
            {
                if (_tilesInMemory.TryGetValue(tile, out var ret))
                {
                    return ret;
                }
            }
            finally
            {
                _kiberCacheLock.ReleaseReaderLock();
            }

            return null;
        }

        internal void AddTileToMemoryCache(RawTile tile, byte[] data)
        {
            if (data != null)
            {
                _kiberCacheLock.AcquireWriterLock();
                try
                {
                    if (!_tilesInMemory.ContainsKey(tile))
                    {
                        _tilesInMemory.Add(tile, data);
                    }
                }
                finally
                {
                    _kiberCacheLock.ReleaseWriterLock();
                }
            }
#if DEBUG
            else
            {
                Debug.WriteLine("adding empty data to MemoryCache ;} ");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
#endif
        }

        internal void RemoveOverload()
        {
            _kiberCacheLock.AcquireWriterLock();
            try
            {
                _tilesInMemory.RemoveMemoryOverload();
            }
            finally
            {
                _kiberCacheLock.ReleaseWriterLock();
            }
        }

        #region IDisposable Members

        ~MemoryCache()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_kiberCacheLock != null)
            {
                if (disposing)
                {
                    Clear();
                }

                _kiberCacheLock.Dispose();
                _kiberCacheLock = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
