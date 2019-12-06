using System;
using System.Collections.Generic;
using System.Threading;

namespace GMap.NET.Internals
{
    /// <summary>
    ///     represent tile
    /// </summary>
    public struct Tile : IDisposable
    {
        public static readonly Tile Empty = new Tile();

        GPoint _pos;
        PureImage[] _overlays;
        long _overlaysCount;

        public readonly bool NotEmpty;

        public Tile(int zoom, GPoint pos)
        {
            NotEmpty = true;
            this.Zoom = zoom;
            this._pos = pos;
            _overlays = null;
            _overlaysCount = 0;
        }

        public IEnumerable<PureImage> Overlays
        {
            get
            {
                for (long i = 0, size = Interlocked.Read(ref _overlaysCount); i < size; i++)
                {
                    yield return _overlays[i];
                }
            }
        }

        internal void AddOverlay(PureImage i)
        {
            if (_overlays == null)
            {
                _overlays = new PureImage[4];
            }

            _overlays[Interlocked.Increment(ref _overlaysCount) - 1] = i;
        }

        internal bool HasAnyOverlays
        {
            get
            {
                return Interlocked.Read(ref _overlaysCount) > 0;
            }
        }

        public int Zoom
        {
            get;
            private set;
        }

        public GPoint Pos
        {
            get
            {
                return _pos;
            }
            private set
            {
                _pos = value;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_overlays != null)
            {
                for (long i = Interlocked.Read(ref _overlaysCount) - 1; i >= 0; i--)
                {
                    Interlocked.Decrement(ref _overlaysCount);
                    _overlays[i].Dispose();
                    _overlays[i] = null;
                }

                _overlays = null;
            }
        }

        #endregion

        public static bool operator ==(Tile m1, Tile m2)
        {
            return m1._pos == m2._pos && m1.Zoom == m2.Zoom;
        }

        public static bool operator !=(Tile m1, Tile m2)
        {
            return !(m1 == m2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Tile))
                return false;

            Tile comp = (Tile)obj;
            return comp.Zoom == Zoom && comp.Pos == Pos;
        }

        public override int GetHashCode()
        {
            return Zoom ^ _pos.GetHashCode();
        }
    }
}
