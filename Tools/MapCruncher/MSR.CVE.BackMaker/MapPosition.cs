using System;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class MapPosition
    {
        private const string MapPositionTag = "MapPosition";
        private const string lonAttr = "lon";
        private const string latAttr = "lat";
        private const string zoomAttr = "zoom";
        private const string styleAttr = "style";
        private LatLonZoom _llz;
        private bool valid;
        private PositionUpdateIfc updateIfc;

        public LatLonZoom llz
        {
            get
            {
                return _llz;
            }
        }

        public string style
        {
            get;
            private set;
        }

        public MapPosition(PositionUpdateIfc updateIfc)
        {
            this.updateIfc = updateIfc;
        }

        public MapPosition(LatLonZoom llz, string style, PositionUpdateIfc updateIfc)
        {
            _llz = llz;
            this.style = style;
            valid = true;
            this.updateIfc = updateIfc;
        }

        public MapPosition(MapPosition prototype, PositionUpdateIfc updateIfc)
        {
            _llz = prototype._llz;
            style = prototype.style;
            valid = prototype.valid;
            this.updateIfc = updateIfc;
        }

        public override bool Equals(object o2)
        {
            if (o2 is MapPosition)
            {
                MapPosition mapPosition = (MapPosition)o2;
                return _llz == mapPosition._llz;
            }

            return false;
        }

        public override string ToString()
        {
            return _llz.ToString();
        }

        private void setBase()
        {
            if (updateIfc != null)
            {
                updateIfc.PositionUpdated(_llz);
            }

            valid = true;
        }

        public void setPosition(LatLonZoom llz, string style)
        {
            _llz = llz;
            this.style = style;
            setBase();
        }

        public void setPosition(LatLonZoom llz)
        {
            _llz = llz;
            setBase();
        }

        public void setPosition(MapPosition p)
        {
            if (p.IsValid())
            {
                setPosition(p.llz, p.style);
            }
        }

        public void setZoom(int zoom)
        {
            _llz = new LatLonZoom(llz.lat, llz.lon, zoom);
            setBase();
        }

        public void setStyle(string style)
        {
            this.style = style;
            setBase();
        }

        public bool IsValid()
        {
            return valid;
        }

        public override int GetHashCode()
        {
            return _llz.GetHashCode() ^ style.GetHashCode();
        }

        public static string GetXMLTag(MashupXMLSchemaVersion version)
        {
            if (version == MonolithicMapPositionsSchema.schema)
            {
                return "MapPosition";
            }

            return "MapPosition";
        }

        public void WriteXML(XmlWriter writer)
        {
            writer.WriteStartElement("MapPosition");
            if (style != null)
            {
                writer.WriteStartAttribute("style");
                writer.WriteValue(style);
                writer.WriteEndAttribute();
            }

            llz.WriteXML(writer);
            writer.WriteEndElement();
        }

        public MapPosition(MashupParseContext context, PositionUpdateIfc updateIfc, CoordinateSystemIfc coordSys)
        {
            this.updateIfc = updateIfc;
            if (context.version == MonolithicMapPositionsSchema.schema)
            {
                XMLTagReader xMLTagReader = context.NewTagReader("MapPosition");
                _llz = new LatLonZoom(
                    Convert.ToDouble(context.reader.GetAttribute("lat"), CultureInfo.InvariantCulture),
                    Convert.ToDouble(context.reader.GetAttribute("lon"), CultureInfo.InvariantCulture),
                    Convert.ToInt32(context.reader.GetAttribute("zoom"), CultureInfo.InvariantCulture));
                if (context.reader.GetAttribute("style") != null)
                {
                    setStyle(context.reader.GetAttribute("style"));
                }

                valid = true;
                while (xMLTagReader.FindNextStartTag())
                {
                }

                return;
            }

            XMLTagReader xMLTagReader2 = context.NewTagReader("MapPosition");
            if (context.reader.GetAttribute("style") != null)
            {
                setStyle(context.reader.GetAttribute("style"));
            }

            while (xMLTagReader2.FindNextStartTag())
            {
                if (xMLTagReader2.TagIs(LatLonZoom.GetXMLTag()))
                {
                    _llz = new LatLonZoom(context, coordSys);
                    valid = true;
                }
            }

            while (xMLTagReader2.FindNextStartTag())
            {
            }
        }

        internal void ForceInteractiveUpdate()
        {
            if (updateIfc != null)
            {
                updateIfc.ForceInteractiveUpdate();
            }
        }
    }
}
