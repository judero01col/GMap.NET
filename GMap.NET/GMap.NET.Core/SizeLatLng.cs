using System.Globalization;

namespace GMap.NET
{
    /// <summary>
    ///     the size of coordinates
    /// </summary>
    public struct SizeLatLng
    {
        public static readonly SizeLatLng Empty;

        public SizeLatLng(SizeLatLng size)
        {
            WidthLng = size.WidthLng;
            HeightLat = size.HeightLat;
        }

        public SizeLatLng(PointLatLng pt)
        {
            HeightLat = pt.Lat;
            WidthLng = pt.Lng;
        }

        public SizeLatLng(double heightLat, double widthLng)
        {
            HeightLat = heightLat;
            WidthLng = widthLng;
        }

        public static SizeLatLng operator +(SizeLatLng sz1, SizeLatLng sz2)
        {
            return Add(sz1, sz2);
        }

        public static SizeLatLng operator -(SizeLatLng sz1, SizeLatLng sz2)
        {
            return Subtract(sz1, sz2);
        }

        public static bool operator ==(SizeLatLng sz1, SizeLatLng sz2)
        {
            return sz1.WidthLng == sz2.WidthLng && sz1.HeightLat == sz2.HeightLat;
        }

        public static bool operator !=(SizeLatLng sz1, SizeLatLng sz2)
        {
            return !(sz1 == sz2);
        }

        public static explicit operator PointLatLng(SizeLatLng size)
        {
            return new PointLatLng(size.HeightLat, size.WidthLng);
        }

        public bool IsEmpty
        {
            get
            {
                return WidthLng == 0d && HeightLat == 0d;
            }
        }

        public double WidthLng
        {
            get;
            set;
        }

        public double HeightLat
        {
            get;
            set;
        }

        public static SizeLatLng Add(SizeLatLng sz1, SizeLatLng sz2)
        {
            return new SizeLatLng(sz1.HeightLat + sz2.HeightLat, sz1.WidthLng + sz2.WidthLng);
        }

        public static SizeLatLng Subtract(SizeLatLng sz1, SizeLatLng sz2)
        {
            return new SizeLatLng(sz1.HeightLat - sz2.HeightLat, sz1.WidthLng - sz2.WidthLng);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SizeLatLng))
            {
                return false;
            }

            SizeLatLng ef = (SizeLatLng)obj;
            return ef.WidthLng == WidthLng && ef.HeightLat == HeightLat &&
                   ef.GetType().Equals(GetType());
        }

        public override int GetHashCode()
        {
            if (IsEmpty)
            {
                return 0;
            }

            return WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
        }

        public PointLatLng ToPointLatLng()
        {
            return (PointLatLng)this;
        }

        public override string ToString()
        {
            return "{WidthLng=" + WidthLng.ToString(CultureInfo.CurrentCulture) + ", HeightLng=" +
                   HeightLat.ToString(CultureInfo.CurrentCulture) + "}";
        }

        static SizeLatLng()
        {
            Empty = new SizeLatLng();
        }
    }
}
