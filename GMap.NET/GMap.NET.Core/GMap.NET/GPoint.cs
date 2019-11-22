using System;
using System.Collections.Generic;
using System.Globalization;

namespace GMap.NET
{
    /// <summary>
    ///     the point ;}
    /// </summary>
    [Serializable]
    public struct GPoint
    {
        public static readonly GPoint Empty = new GPoint();

        public GPoint(long x, long y)
        {
            X = x;
            Y = y;
        }

        public GPoint(GSize sz)
        {
            X = sz.Width;
            Y = sz.Height;
        }

        public bool IsEmpty
        {
            get
            {
                return X == 0 && Y == 0;
            }
        }

        public long X
        {
            get;
            set;
        }

        public long Y
        {
            get;
            set;
        }

        public static explicit operator GSize(GPoint p)
        {
            return new GSize(p.X, p.Y);
        }

        public static GPoint operator +(GPoint pt, GSize sz)
        {
            return Add(pt, sz);
        }

        public static GPoint operator -(GPoint pt, GSize sz)
        {
            return Subtract(pt, sz);
        }

        public static bool operator ==(GPoint left, GPoint right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(GPoint left, GPoint right)
        {
            return !(left == right);
        }

        public static GPoint Add(GPoint pt, GSize sz)
        {
            return new GPoint(pt.X + sz.Width, pt.Y + sz.Height);
        }

        public static GPoint Subtract(GPoint pt, GSize sz)
        {
            return new GPoint(pt.X - sz.Width, pt.Y - sz.Height);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GPoint))
                return false;

            var comp = (GPoint)obj;
            return comp.X == X && comp.Y == Y;
        }

        public override int GetHashCode()
        {
            return (int)(X ^ Y);
        }

        public void Offset(long dx, long dy)
        {
            X += dx;
            Y += dy;
        }

        public void Offset(GPoint p)
        {
            Offset(p.X, p.Y);
        }

        public void OffsetNegative(GPoint p)
        {
            Offset(-p.X, -p.Y);
        }

        public override string ToString()
        {
            return "{X=" + X.ToString(CultureInfo.CurrentCulture) + ",Y=" + Y.ToString(CultureInfo.CurrentCulture) +
                   "}";
        }

        //private static int HIWORD(int n)
        //{
        //   return (n >> 16) & 0xffff;
        //}

        //private static int LOWORD(int n)
        //{
        //   return n & 0xffff;
        //}
    }

    internal class GPointComparer : IEqualityComparer<GPoint>
    {
        public bool Equals(GPoint x, GPoint y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(GPoint obj)
        {
            return obj.GetHashCode();
        }
    }
}
