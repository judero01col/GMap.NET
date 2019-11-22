
namespace GMap.NET
{
   using System;
   using System.Globalization;

   /// <summary>
   /// the rect of coordinates
   /// </summary>
   public struct RectLatLng
   {
      public static readonly RectLatLng Empty;

      public RectLatLng(double lat, double lng, double widthLng, double heightLat)
      {
         Lng = lng;
         Lat = lat;
         WidthLng = widthLng;
         HeightLat = heightLat;
         _notEmpty = true;
      }

      public RectLatLng(PointLatLng location, SizeLatLng size)
      {
         Lng = location.Lng;
         Lat = location.Lat;
         WidthLng = size.WidthLng;
         HeightLat = size.HeightLat;
         _notEmpty = true;
      }

      public static RectLatLng FromLTRB(double leftLng, double topLat, double rightLng, double bottomLat)
      {
         return new RectLatLng(topLat, leftLng, rightLng - leftLng, topLat - bottomLat);
      }

      public PointLatLng LocationTopLeft
      {
         get
         {
            return new PointLatLng(this.Lat, this.Lng);
         }
         set
         {
            Lng = value.Lng;
            Lat = value.Lat;
         }
      }

      public PointLatLng LocationRightBottom
      {
         get
         {
            var ret = new PointLatLng(this.Lat, this.Lng);
            ret.Offset(HeightLat, WidthLng);
            return ret;
         }
      }

      public PointLatLng LocationMiddle
      {
         get
         {
            var ret = new PointLatLng(this.Lat, this.Lng);
            ret.Offset(HeightLat / 2, WidthLng / 2);
            return ret;
         }
      }

      public SizeLatLng Size
      {
         get
         {
            return new SizeLatLng(this.HeightLat, this.WidthLng);
         }
         set
         {
            WidthLng = value.WidthLng;
            HeightLat = value.HeightLat;
         }
      }

      public double Lng { get; set; }

      public double Lat { get; set; }

      public double WidthLng { get; set; }

      public double HeightLat { get; set; }

      public double Left
      {
         get
         {
            return Lng;
         }
      }

      public double Top
      {
         get
         {
            return Lat;
         }
      }

      public double Right
      {
         get
         {
            return Lng + WidthLng;
         }
      }

      public double Bottom
      {
         get
         {
            return Lat - HeightLat;
         }
      }

      private bool _notEmpty;

      /// <summary>
      /// returns true if coordinates wasn't assigned
      /// </summary>
      public bool IsEmpty
      {
          get
          {
              return !_notEmpty;
          }
      }

      public override bool Equals(object obj)
      {
         if(!(obj is RectLatLng))
         {
            return false;
         }

         var ef = (RectLatLng)obj;
         return ((((ef.Lng == this.Lng) && (ef.Lat == this.Lat)) && (ef.WidthLng == this.WidthLng)) && (ef.HeightLat == this.HeightLat));
      }

      public static bool operator ==(RectLatLng left, RectLatLng right)
      {
         return ((((left.Lng == right.Lng) && (left.Lat == right.Lat)) && (left.WidthLng == right.WidthLng)) && (left.HeightLat == right.HeightLat));
      }

      public static bool operator !=(RectLatLng left, RectLatLng right)
      {
         return !(left == right);
      }

      public bool Contains(double lat, double lng)
      {
         return ((((this.Lng <= lng) && (lng < (this.Lng + this.WidthLng))) && (this.Lat >= lat)) && (lat > (this.Lat - this.HeightLat)));
      }

      public bool Contains(PointLatLng pt)
      {
         return this.Contains(pt.Lat, pt.Lng);
      }

      public bool Contains(RectLatLng rect)
      {
         return ((((this.Lng <= rect.Lng) && ((rect.Lng + rect.WidthLng) <= (this.Lng + this.WidthLng))) && (this.Lat >= rect.Lat)) && ((rect.Lat - rect.HeightLat) >= (this.Lat - this.HeightLat)));
      }

      public override int GetHashCode()
      {
         if(this.IsEmpty)
         {
            return 0;
         }
         return (((this.Lng.GetHashCode() ^ this.Lat.GetHashCode()) ^ this.WidthLng.GetHashCode()) ^ this.HeightLat.GetHashCode());
      }

      // from here down need to test each function to be sure they work good
      // |
      // .

      #region -- unsure --
      public void Inflate(double lat, double lng)
      {
         this.Lng -= lng;
         this.Lat += lat;
         this.WidthLng += 2d * lng;
         this.HeightLat += 2d * lat;
      }

      public void Inflate(SizeLatLng size)
      {
         this.Inflate(size.HeightLat, size.WidthLng);
      }

      public static RectLatLng Inflate(RectLatLng rect, double lat, double lng)
      {
         RectLatLng ef = rect;
         ef.Inflate(lat, lng);
         return ef;
      }

      public void Intersect(RectLatLng rect)
      {
         RectLatLng ef = Intersect(rect, this);
         this.Lng = ef.Lng;
         this.Lat = ef.Lat;
         this.WidthLng = ef.WidthLng;
         this.HeightLat = ef.HeightLat;
      }

      // ok ???
      public static RectLatLng Intersect(RectLatLng a, RectLatLng b)
      {
         double lng = Math.Max(a.Lng, b.Lng);
         double num2 = Math.Min((double)(a.Lng + a.WidthLng), (double)(b.Lng + b.WidthLng));

         double lat = Math.Max(a.Lat, b.Lat);
         double num4 = Math.Min((double)(a.Lat + a.HeightLat), (double)(b.Lat + b.HeightLat));

         if((num2 >= lng) && (num4 >= lat))
         {
            return new RectLatLng(lat, lng, num2 - lng, num4 - lat);
         }
         return Empty;
      }

      // ok ???
      // http://greatmaps.codeplex.com/workitem/15981
      public bool IntersectsWith(RectLatLng a)
      {
         return this.Left < a.Right && this.Top > a.Bottom && this.Right > a.Left && this.Bottom < a.Top;
      }

      // ok ???
      // http://greatmaps.codeplex.com/workitem/15981
      public static RectLatLng Union(RectLatLng a, RectLatLng b)
      {
         return RectLatLng.FromLTRB(
            Math.Min(a.Left, b.Left),
            Math.Max(a.Top, b.Top),
            Math.Max(a.Right, b.Right),
            Math.Min(a.Bottom, b.Bottom));
      }
      #endregion

      // .
      // |
      // unsure ends here

      public void Offset(PointLatLng pos)
      {
         this.Offset(pos.Lat, pos.Lng);
      }

      public void Offset(double lat, double lng)
      {
         this.Lng += lng;
         this.Lat -= lat;
      }

      public override string ToString()
      {
         return ("{Lat=" + this.Lat.ToString(CultureInfo.CurrentCulture) + ",Lng=" + this.Lng.ToString(CultureInfo.CurrentCulture) + ",WidthLng=" + this.WidthLng.ToString(CultureInfo.CurrentCulture) + ",HeightLat=" + this.HeightLat.ToString(CultureInfo.CurrentCulture) + "}");
      }

      static RectLatLng()
      {
         Empty = new RectLatLng();
      }
   }
}
