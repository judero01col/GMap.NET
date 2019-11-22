using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class MapRectangle : IRobustlyHashable
    {
        public LatLon ll0;
        public LatLon ll1;
        private const string MapRectangleTag = "MapRectangle";

        public MapRectangle()
        {
        }

        public MapRectangle(MapRectangle mr)
        {
            ll0 = mr.ll0;
            ll1 = mr.ll1;
            AssertOrder();
        }

        public MapRectangle(LatLon NW, LatLon SE)
        {
            ll0 = new LatLon(SE.lat, NW.lon);
            ll1 = new LatLon(NW.lat, SE.lon);
            AssertOrder();
        }

        public MapRectangle(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            XMLTagReader reader = context.NewTagReader("MapRectangle");
            List<LatLon> list = new List<LatLon>();
            while (reader.FindNextStartTag())
            {
                if (reader.TagIs(LatLon.GetXMLTag()))
                {
                    list.Add(new LatLon(context, coordSys));
                }
            }

            reader.SkipAllSubTags();
            if (list.Count != 2)
            {
                throw new InvalidMashupFile(context,
                    string.Format("{0} should contain exactly 2 {1} subtags", "MapRectangle", LatLon.GetXMLTag()));
            }

            ll0 = list[0];
            ll1 = list[1];
            AssertOrder();
        }

        public MapRectangle(double lat0, double lon0, double lat1, double lon1)
        {
            ll0 = new LatLon(lat0, lon0);
            ll1 = new LatLon(lat1, lon1);
            AssertOrder();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            ll0.AccumulateRobustHash(hash);
            ll1.AccumulateRobustHash(hash);
        }

        internal static MapRectangle AddToBoundingBox(MapRectangle box, LatLon ll)
        {
            if (box == null)
            {
                return new MapRectangle(ll.lat, ll.lon, ll.lat, ll.lon);
            }

            return new MapRectangle(Math.Min(ll.lat, box.lat0),
                Math.Min(ll.lon, box.lon0),
                Math.Max(ll.lat, box.lat1),
                Math.Max(ll.lon, box.lon1));
        }

        private void AssertOrder()
        {
            D.Assert(ll0.lat <= ll1.lat);
        }

        private static bool betweenInclusive(double subject, double low, double high)
        {
            D.Assert(low <= high);
            return low <= subject && subject <= high;
        }

        internal MapRectangle ClipTo(MapRectangle clipRect)
        {
            double num = Math.Max(lat0, clipRect.lat0);
            double num2 = Math.Max(lon0, clipRect.lon0);
            double num3 = Math.Max(num, Math.Min(lat1, clipRect.lat1));
            return new MapRectangle(num, num2, num3, Math.Max(num2, Math.Min(lon1, clipRect.lon1)));
        }

        public override bool Equals(object o2)
        {
            MapRectangle rectangle = (MapRectangle)o2;
            if (rectangle == null)
            {
                return false;
            }

            return this == rectangle;
        }

        public string FilenameString()
        {
            return string.Format("rect.{0}.{1}.{2}.{3}", new object[] {lat0, lon0, lat1, lon1});
        }

        internal LatLon GetCenter()
        {
            return new LatLon((lat0 + lat1) * 0.5, (lon0 + lon1) * 0.5);
        }

        public override int GetHashCode()
        {
            return ll0.GetHashCode() ^ ll1.GetHashCode();
        }

        internal LatLon GetNE()
        {
            return new LatLon(lat1, lon1);
        }

        internal LatLon GetNW()
        {
            return new LatLon(lat1, lon0);
        }

        internal LatLon GetSE()
        {
            return new LatLon(lat0, lon1);
        }

        internal LatLon GetSW()
        {
            return new LatLon(lat0, lon0);
        }

        public static string GetXMLTag()
        {
            return "MapRectangle";
        }

        internal MapRectangle GrowFraction(double p)
        {
            return new MapRectangle(lat0 - p * (lat1 - lat0),
                lon0 - p * (lon1 - lon0),
                lat1 + p * (lat1 - lat0),
                lon1 + p * (lon1 - lon0));
        }

        internal MapRectangle Intersect(MapRectangle other)
        {
            return new MapRectangle
            {
                ll0 = new LatLon(Math.Max(lat0, other.lat0), Math.Max(lon0, other.lon0)),
                ll1 = new LatLon(Math.Min(lat1, other.lat1), Math.Min(lon1, other.lon1))
            };
        }

        public bool intersects(MapRectangle othr)
        {
            bool flag = betweenInclusive(ll0.lat, othr.ll0.lat, othr.ll1.lat) ||
                        betweenInclusive(ll1.lat, othr.ll0.lat, othr.ll1.lat) ||
                        betweenInclusive(othr.ll0.lat, ll0.lat, ll1.lat) ||
                        betweenInclusive(othr.ll1.lat, ll0.lat, ll1.lat);
            bool flag2 = betweenInclusive(ll0.lon, othr.ll0.lon, othr.ll1.lon) ||
                         betweenInclusive(ll1.lon, othr.ll0.lon, othr.ll1.lon) ||
                         betweenInclusive(othr.ll0.lon, ll0.lon, ll1.lon) ||
                         betweenInclusive(othr.ll1.lon, ll0.lon, ll1.lon);
            return flag && flag2;
        }

        public bool IsEmpty()
        {
            if (lat1 > lat0)
            {
                return lon1 <= lon0;
            }

            return true;
        }

        public static MapRectangle MapRectangleIgnoreOrder(LatLon NW, LatLon SE)
        {
            return new MapRectangle {ll0 = new LatLon(SE.lat, NW.lon), ll1 = new LatLon(NW.lat, SE.lon)};
        }

        public static bool operator ==(MapRectangle mr1, MapRectangle mr2)
        {
            if (ReferenceEquals(mr1, null) && ReferenceEquals(mr2, null))
            {
                return true;
            }

            if (ReferenceEquals(mr1, null) || ReferenceEquals(mr2, null))
            {
                return false;
            }

            return mr1.ll0 == mr2.ll0 && mr1.ll1 == mr2.ll1;
        }

        public static bool operator !=(MapRectangle mr1, MapRectangle mr2)
        {
            return !(mr1 == mr2);
        }

        public Size SizeWithAspectRatio(int longDimension)
        {
            double num = (lat1 - lat0) / (lon1 - lon0);
            if (num > 1.0)
            {
                return new Size((int)(longDimension / num), longDimension);
            }

            return new Size(longDimension, (int)(longDimension * num));
        }

        public MapRectangle SquareOff()
        {
            double num = Math.Max(lon1 - lon0, lat1 - lat0);
            return new MapRectangle(lat0, lon0, lat0 + num, lon0 + num);
        }

        public override string ToString()
        {
            return string.Format("MapRectangle({0},{1},{2},{3})",
                new object[] {lat0, lon0, lat1, lon1});
        }

        internal MapRectangle Transform(IPointTransformer transformer)
        {
            MapRectangle box = null;
            return AddToBoundingBox(
                AddToBoundingBox(
                    AddToBoundingBox(AddToBoundingBox(box, transformer.getTransformedPoint(GetSW())),
                        transformer.getTransformedPoint(GetSE())),
                    transformer.getTransformedPoint(GetNW())),
                transformer.getTransformedPoint(GetNE()));
        }

        internal static MapRectangle Union(MapRectangle box1, MapRectangle box2)
        {
            if (box1 == null)
            {
                return box2;
            }

            if (box2 == null)
            {
                return box1;
            }

            return AddToBoundingBox(AddToBoundingBox(box1, box2.GetSW()), box2.GetNE());
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("MapRectangle");
            ll0.WriteXML(writer);
            ll1.WriteXML(writer);
            writer.WriteEndElement();
        }

        public double lat0
        {
            get
            {
                return ll0.lat;
            }
        }

        public double lat1
        {
            get
            {
                return ll1.lat;
            }
        }

        public double LatExtent
        {
            get
            {
                return ll1.lat - ll0.lat;
            }
        }

        public double lon0
        {
            get
            {
                return ll0.lon;
            }
        }

        public double lon1
        {
            get
            {
                return ll1.lon;
            }
        }

        public double LonExtent
        {
            get
            {
                return ll1.lon - ll0.lon;
            }
        }
    }
}
