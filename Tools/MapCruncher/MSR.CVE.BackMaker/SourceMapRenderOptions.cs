using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapRenderOptions : IRobustlyHashable
    {
        public const int UNDEFINED_ZOOM = -1;
        private const string SourceMapRenderOptionsTag = "SourceMapRenderOptions";
        private const string MinZoomAttr = "MinZoom";
        private const string MaxZoomAttr = "MaxZoom";
        private int _minZoom = 1;
        private int _maxZoom = -1;
        public DirtyEvent dirtyEvent;

        public int minZoom
        {
            get
            {
                return _minZoom;
            }
            set
            {
                if (value != _minZoom)
                {
                    _minZoom = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public int maxZoom
        {
            get
            {
                return _maxZoom;
            }
            set
            {
                if (value != _maxZoom)
                {
                    _maxZoom = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public SourceMapRenderOptions(DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
        }

        public SourceMapRenderOptions(MashupParseContext context, DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            XMLTagReader xMLTagReader = context.NewTagReader("SourceMapRenderOptions");
            new MercatorCoordinateSystem();
            string attribute = context.reader.GetAttribute("MinZoom");
            if (attribute != null)
            {
                _minZoom = MercatorCoordinateSystem.theInstance.GetZoomRange()
                    .Parse(context, "MinZoom", attribute);
            }
            else
            {
                _minZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().min;
            }

            _maxZoom = MercatorCoordinateSystem.theInstance.GetZoomRange()
                .ParseAllowUndefinedZoom(context, "MaxZoom", context.reader.GetAttribute("MaxZoom"));
            if (_minZoom > _maxZoom)
            {
                throw new InvalidMashupFile(context,
                    string.Format("MinZoom {0} > MaxZoom {1}", _minZoom, _maxZoom));
            }

            xMLTagReader.SkipAllSubTags();
        }

        internal void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("SourceMapRenderOptions");
            writer.WriteAttributeString("MinZoom", _minZoom.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("MaxZoom", _maxZoom.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        internal static string GetXMLTag()
        {
            return "SourceMapRenderOptions";
        }

        public override int GetHashCode()
        {
            return minZoom * 131 + maxZoom;
        }

        public override bool Equals(object obj)
        {
            return minZoom == ((SourceMapRenderOptions)obj).minZoom;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(minZoom);
            hash.Accumulate(maxZoom);
        }
    }
}
