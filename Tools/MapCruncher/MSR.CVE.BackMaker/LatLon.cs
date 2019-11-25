using System;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public struct LatLon
    {
        private const string LatLonTag = "LatLon";
        private const string lonAttr = "lon";
        private const string latAttr = "lat";

        public double lat
        {
            get;
        }

        public double lon
        {
            get;
        }

        public LatLon(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public static implicit operator PointD(LatLon p)
        {
            return new PointD(p.lon, p.lat);
        }

        public static implicit operator LatLon(PointD p)
        {
            return new LatLon(p.y, p.x);
        }

        public override bool Equals(object o)
        {
            if (o is LatLon)
            {
                LatLon ll = (LatLon)o;
                return this == ll;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return lat.GetHashCode() ^ lon.GetHashCode();
        }

        public static bool operator ==(LatLon ll1, LatLon ll2)
        {
            return ll1.lat == ll2.lat && ll1.lon == ll2.lon;
        }

        public static bool operator !=(LatLon ll1, LatLon ll2)
        {
            return ll1.lat != ll2.lat || ll1.lon != ll2.lon;
        }

        public override string ToString()
        {
            return string.Format("{0}N {1}E", lat, lon);
        }

        public static string GetXMLTag()
        {
            return "LatLon";
        }

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("LatLon");
            WriteXMLToAttributes(writer);
            writer.WriteEndElement();
        }

        public void WriteXMLToAttributes(XmlWriter writer)
        {
            writer.WriteStartAttribute("lat");
            writer.WriteValue(lat.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();
            writer.WriteStartAttribute("lon");
            writer.WriteValue(lon.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();
        }

        public LatLon(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("LatLon");
            lat = coordSys.GetLatRange().Parse(context, "lat");
            lon = coordSys.GetLonRange().Parse(context, "lon");
            while (xMLTagReader.FindNextStartTag())
            {
            }
        }

        public void CheckValid(CoordinateSystemIfc coordSys)
        {
            coordSys.GetLatRange().CheckValid(lat);
            coordSys.GetLonRange().CheckValid(lon);
        }

        public static LatLon ReadFromAttributes(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            double lat = coordSys.GetLatRange().Parse(context, "lat");
            double lon = coordSys.GetLonRange().Parse(context, "lon");
            return new LatLon(lat, lon);
        }

        public static double DistanceInMeters(LatLon p1, LatLon p2)
        {
            double num = CoordinateSystemUtilities.DegreesToRadians(p1.lon);
            double num2 = CoordinateSystemUtilities.DegreesToRadians(p1.lat);
            double num3 = CoordinateSystemUtilities.DegreesToRadians(p2.lon);
            double num4 = CoordinateSystemUtilities.DegreesToRadians(p2.lat);
            double num5 = num3 - num;
            double num6 = num4 - num2;
            double d = Math.Pow(Math.Sin(num6 / 2.0), 2.0) +
                       Math.Cos(num2) * Math.Cos(num4) * Math.Pow(Math.Sin(num5 / 2.0), 2.0);
            double num7 = 2.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(d)));
            return 6378137.0 * num7;
        }

        public static string PrettyDistance(double distanceInMeters)
        {
            if (distanceInMeters < 1.0)
            {
                return string.Format("{0:F0}cm", distanceInMeters * 100.0);
            }

            if (distanceInMeters < 10.0)
            {
                return string.Format("{0:F1}m", distanceInMeters);
            }

            if (distanceInMeters < 1000.0)
            {
                return string.Format("{0:F0}m", distanceInMeters);
            }

            if (distanceInMeters < 10000.0)
            {
                return string.Format("{0:F1}km", distanceInMeters / 1000.0);
            }

            return string.Format("{0:F0}km", distanceInMeters / 1000.0);
        }

        internal void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(lat);
            hash.Accumulate(lon);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * 3.1415926535897931 / 180.0;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / 3.1415926535897931;
        }
    }
}
