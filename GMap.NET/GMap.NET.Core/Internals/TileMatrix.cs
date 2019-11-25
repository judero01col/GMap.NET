using System;
using System.Collections.Generic;

namespace GMap.NET.Internals
{
    /// <summary>
    ///     matrix for tiles
    /// </summary>
    internal class TileMatrix : IDisposable
    {
        List<Dictionary<GPoint, Tile>> _levels = new List<Dictionary<GPoint, Tile>>(33);
        FastReaderWriterLock _lock = new FastReaderWriterLock();

        public TileMatrix()
        {
            for (int i = 0; i < _levels.Capacity; i++)
            {
                _levels.Add(new Dictionary<GPoint, Tile>(55, new GPointComparer()));
            }
        }

        public void ClearAllLevels()
        {
            _lock.AcquireWriterLock();
            try
            {
                foreach (var matrix in _levels)
                {
                    foreach (var t in matrix)
                    {
                        t.Value.Dispose();
                    }

                    matrix.Clear();
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void ClearLevel(int zoom)
        {
            _lock.AcquireWriterLock();
            try
            {
                if (zoom < _levels.Count)
                {
                    var l = _levels[zoom];

                    foreach (var t in l)
                    {
                        t.Value.Dispose();
                    }

                    l.Clear();
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        List<KeyValuePair<GPoint, Tile>> _tmp = new List<KeyValuePair<GPoint, Tile>>(44);

        public void ClearLevelAndPointsNotIn(int zoom, List<DrawTile> list)
        {
            _lock.AcquireWriterLock();
            try
            {
                if (zoom < _levels.Count)
                {
                    var l = _levels[zoom];

                    _tmp.Clear();

                    foreach (var t in l)
                    {
                        if (!list.Exists(p => p.PosXY == t.Key))
                        {
                            _tmp.Add(t);
                        }
                    }

                    foreach (var r in _tmp)
                    {
                        l.Remove(r.Key);
                        r.Value.Dispose();
                    }

                    _tmp.Clear();
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void ClearLevelsBelove(int zoom)
        {
            _lock.AcquireWriterLock();
            try
            {
                if (zoom - 1 < _levels.Count)
                {
                    for (int i = zoom - 1; i >= 0; i--)
                    {
                        var l = _levels[i];

                        foreach (var t in l)
                        {
                            t.Value.Dispose();
                        }

                        l.Clear();
                    }
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void ClearLevelsAbove(int zoom)
        {
            _lock.AcquireWriterLock();
            try
            {
                if (zoom + 1 < _levels.Count)
                {
                    for (int i = zoom + 1; i < _levels.Count; i++)
                    {
                        var l = _levels[i];

                        foreach (var t in l)
                        {
                            t.Value.Dispose();
                        }

                        l.Clear();
                    }
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void EnterReadLock()
        {
            _lock.AcquireReaderLock();
        }

        public void LeaveReadLock()
        {
            _lock.ReleaseReaderLock();
        }

        public Tile GetTileWithNoLock(int zoom, GPoint p)
        {
            var ret = Tile.Empty;

            //if(zoom < Levels.Count)
            {
                _levels[zoom].TryGetValue(p, out ret);
            }

            return ret;
        }

        public Tile GetTileWithReadLock(int zoom, GPoint p)
        {
            var ret = Tile.Empty;

            _lock.AcquireReaderLock();
            try
            {
                ret = GetTileWithNoLock(zoom, p);
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }

            return ret;
        }

        public void SetTile(Tile t)
        {
            _lock.AcquireWriterLock();
            try
            {
                if (t.Zoom < _levels.Count)
                {
                    _levels[t.Zoom][t.Pos] = t;
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        #region IDisposable Members

        ~TileMatrix()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_lock != null)
            {
                if (disposing)
                {
                    ClearAllLevels();
                }

                _levels.Clear();
                _levels = null;

                _tmp.Clear();
                _tmp = null;

                _lock.Dispose();
                _lock = null;
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
