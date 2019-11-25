using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public struct LatLonZoom : IRobustlyHashable
    {
        private const string LatLonZoomTag = "LatLonZoom";
        private const string zoomAttr = "zoom";
        private LatLon _latlon;

        public LatLon latlon
        {
            get
            {
                return _latlon;
            }
        }

        public int zoom
        {
            get;
        }

        public double lat
        {
            get
            {
                return _latlon.lat;
            }
        }

        public double lon
        {
            get
            {
                return _latlon.lon;
            }
        }

        public LatLonZoom(double lat, double lon, int zoom)
        {
            _latlon = new LatLon(lat, lon);
            this.zoom = zoom;
        }

        public LatLonZoom(LatLon latlon, int zoom)
        {
            _latlon = latlon;
            this.zoom = zoom;
        }

        public override bool Equals(object o)
        {
            if (o is LatLonZoom)
            {
                LatLonZoom llz = (LatLonZoom)o;
                return this == llz;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _latlon.GetHashCode() ^ zoom.GetHashCode();
        }

        public static bool operator ==(LatLonZoom llz1, LatLonZoom llz2)
        {
            return llz1._latlon == llz2._latlon && llz1.zoom == llz2.zoom;
        }

        public static bool operator !=(LatLonZoom llz1, LatLonZoom llz2)
        {
            return llz1._latlon != llz2._latlon || llz1.zoom != llz2.zoom;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}Z", _latlon, zoom);
        }

        public static string GetXMLTag()
        {
            return "LatLonZoom";
        }

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("LatLonZoom");
            writer.WriteAttributeString("zoom", zoom.ToString(CultureInfo.InvariantCulture));
            _latlon.WriteXML(writer);
            writer.WriteEndElement();
        }

        public void WriteXMLToAttributes(XmlWriter writer)
        {
            _latlon.WriteXMLToAttributes(writer);
            writer.WriteAttributeString("zoom", zoom.ToString(CultureInfo.InvariantCulture));
        }

        public LatLonZoom(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("LatLonZoom");
            try
            {
                if (context.reader.GetAttribute("zoom") == null)
                {
                    throw new InvalidLLZ(context, "Missing zoom attribute");
                }

                try
                {
                    zoom = coordSys.GetZoomRange()
                        .ParseAllowUndefinedZoom(context, "zoom", context.reader.GetAttribute("zoom"));
                }
                catch (InvalidMashupFile invalidMashupFile)
                {
                    throw new InvalidLLZ(context, invalidMashupFile.Message);
                }

                bool flag = false;
                _latlon = default(LatLon);
                while (xMLTagReader.FindNextStartTag())
                {
                    if (xMLTagReader.TagIs(LatLon.GetXMLTag()))
                    {
                        _latlon = new LatLon(context, coordSys);
                        flag = true;
                    }
                }

                if (!flag)
                {
                    throw new InvalidLLZ(context, "Missing LatLong Tag");
                }
            }
            finally
            {
                xMLTagReader.SkipAllSubTags();
            }
        }

        public static LatLonZoom ReadFromAttributes(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            int zoom = coordSys.GetZoomRange()
                .ParseAllowUndefinedZoom(context, "zoom", context.GetRequiredAttribute("zoom"));
            LatLon latLon = LatLon.ReadFromAttributes(context, coordSys);
            return new LatLonZoom(latLon.lat, latLon.lon, zoom);
        }

        public static LatLonZoom USA()
        {
            return new LatLonZoom(40.0, -96.0, 3);
        }

        public static LatLonZoom World()
        {
            return new LatLonZoom(0.0, 0.0, 1);
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            latlon.AccumulateRobustHash(hash);
            hash.Accumulate(zoom);
        }
    }
}
