﻿using System;
using System.Globalization;

namespace GMap.NET
{
    /// <summary>
    ///     the point of coordinates
    /// </summary>
    [Serializable]
    public struct PointLatLng
    {
        public static readonly PointLatLng Empty = new PointLatLng();
        private double _lat;
        private double _lng;

        bool _notEmpty;

        public PointLatLng(double lat, double lng)
         : this(Guid.Empty, lat, lng)
        {
            _lat = lat;
            _lng = lng;
            _notEmpty = true;
        }

        public PointLatLng(Guid id, double lat, double lng)
        {
            Id = id;
            _lat = lat;
            _lng = lng;
            _notEmpty = true;
        }

        public Guid Id { get; }

        /// <summary>
        ///     returns true if coordinates wasn't assigned
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return !_notEmpty;
            }
        }

        public double Lat
        {
            get
            {
                return _lat;
            }
            set
            {
                _lat = value;
                _notEmpty = true;
            }
        }

        public double Lng
        {
            get
            {
                return _lng;
            }
            set
            {
                _lng = value;
                _notEmpty = true;
            }
        }

        public static PointLatLng operator +(PointLatLng pt, SizeLatLng sz)
        {
            return Add(pt, sz);
        }

        public static PointLatLng operator -(PointLatLng pt, SizeLatLng sz)
        {
            return Subtract(pt, sz);
        }

        public static SizeLatLng operator -(PointLatLng pt1, PointLatLng pt2)
        {
            return new SizeLatLng(pt1.Lat - pt2.Lat, pt2.Lng - pt1.Lng);
        }

        public static bool operator ==(PointLatLng left, PointLatLng right)
        {
            return left.Id == right.Id && left.Lng == right.Lng && left.Lat == right.Lat;
        }

        public static bool operator !=(PointLatLng left, PointLatLng right)
        {
            return !(left == right);
        }

        public static PointLatLng Add(PointLatLng pt, SizeLatLng sz)
        {
            return new PointLatLng(pt.Lat - sz.HeightLat, pt.Lng + sz.WidthLng);
        }

        public static PointLatLng Subtract(PointLatLng pt, SizeLatLng sz)
        {
            return new PointLatLng(pt.Lat + sz.HeightLat, pt.Lng - sz.WidthLng);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PointLatLng))
            {
                return false;
            }

            var tf = (PointLatLng)obj;
            return tf.Id == Id && tf.Lng == Lng && tf.Lat == Lat && tf.GetType().Equals(GetType());
        }

        public void Offset(PointLatLng pos)
        {
            Offset(pos.Lat, pos.Lng);
        }

        public void Offset(double lat, double lng)
        {
            Lng += lng;
            Lat -= lat;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Lng.GetHashCode() ^ Lat.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Id={0}, Lat={1}, Lng={2}}}", Id, Lat, Lng);
        }
    }
}
