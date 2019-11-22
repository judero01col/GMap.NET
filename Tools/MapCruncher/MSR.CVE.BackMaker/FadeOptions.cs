using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class FadeOptions
    {
        private const string FadeOptionsTag = "FadeOptions";
        private const string DefaultValueAttr = "DefaultValue";
        private const string AtZoomTag = "FadeAtZoom";
        private const string AtZoomLevelAttr = "ZoomLevel";
        private const string AtZoomValueAttr = "FadeValue";
        private DirtyEvent dirty;
        private double _fadeBase = 1.0;
        private Dictionary<int, double> _zoomToFadeMap = new Dictionary<int, double>();
        public static RangeDouble FadeRange = new RangeDouble(0.0, 1.0);

        public FadeOptions(DirtyEvent dirty)
        {
            this.dirty = dirty;
        }

        public FadeOptions(FadeOptions prototype)
        {
            _fadeBase = prototype._fadeBase;
            _zoomToFadeMap = new Dictionary<int, double>(prototype._zoomToFadeMap);
        }

        public FadeOptions(MashupParseContext context, DirtyEvent dirty)
        {
            this.dirty = dirty;
            XMLTagReader xMLTagReader = context.NewTagReader("FadeOptions");
            _fadeBase = FadeRange.Parse(context, "DefaultValue");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs("FadeAtZoom"))
                {
                    XMLTagReader xMLTagReader2 = context.NewTagReader("FadeAtZoom");
                    int requiredAttributeInt = context.GetRequiredAttributeInt("ZoomLevel");
                    double value = FadeRange.Parse(context, "FadeValue");
                    if (_zoomToFadeMap.ContainsKey(requiredAttributeInt))
                    {
                        throw new InvalidMashupFile(context,
                            string.Format("Fade specified twice for zoom level {0}", requiredAttributeInt));
                    }

                    _zoomToFadeMap[requiredAttributeInt] = value;
                    xMLTagReader2.SkipAllSubTags();
                }
            }
        }

        internal static string GetXMLTag()
        {
            return "FadeOptions";
        }

        internal void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("FadeOptions");
            writer.WriteAttributeString("DefaultValue", _fadeBase.ToString(CultureInfo.InvariantCulture));
            foreach (int current in _zoomToFadeMap.Keys)
            {
                writer.WriteStartElement("FadeAtZoom");
                writer.WriteAttributeString("ZoomLevel", current.ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("FadeValue",
                    _zoomToFadeMap[current].ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        internal double GetFadeForZoomLevel(int zoomLevel)
        {
            if (_zoomToFadeMap.ContainsKey(zoomLevel))
            {
                return _zoomToFadeMap[zoomLevel];
            }

            return _fadeBase;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(_fadeBase);
            foreach (int current in _zoomToFadeMap.Keys)
            {
                hash.Accumulate(current);
                hash.Accumulate(_zoomToFadeMap[current]);
            }
        }
    }
}
