using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapRegistrationView : IMapView, ICurrentView
    {
        private SourceMap _sourceMap;
        private bool _locked;
        private LatLonZoom sourceMapView;
        private MapPosition referenceMapView;
        private static string lockedAttribute = "locked";
        private static string sourceMapViewTag = "SourceMapPosition";
        private static string referenceMapViewTag = "ReferenceMapPosition";

        public SourceMap sourceMap
        {
            get
            {
                return _sourceMap;
            }
        }

        public bool locked
        {
            get
            {
                return _locked;
            }
        }

        public object GetViewedObject()
        {
            return sourceMap;
        }

        public SourceMapRegistrationView(SourceMap sourceMap, MapPosition lockedMapView)
        {
            _sourceMap = sourceMap;
            _locked = true;
            referenceMapView = new MapPosition(lockedMapView, null);
            sourceMapView = lockedMapView.llz;
        }

        public SourceMapRegistrationView(SourceMap sourceMap, LatLonZoom sourceMapView, MapPosition referenceMapView)
        {
            _sourceMap = sourceMap;
            _locked = false;
            this.sourceMapView = sourceMapView;
            this.referenceMapView = new MapPosition(referenceMapView, null);
        }

        public static string GetXMLTag()
        {
            return "SourceMapView";
        }

        public SourceMapRegistrationView(SourceMap sourceMap, MashupParseContext context)
        {
            _sourceMap = sourceMap;
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            _locked = context.GetRequiredAttributeBoolean(lockedAttribute);
            bool flag = false;
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(sourceMapViewTag))
                {
                    XMLTagReader xMLTagReader2 = context.NewTagReader(sourceMapViewTag);
                    while (xMLTagReader2.FindNextStartTag())
                    {
                        if (xMLTagReader2.TagIs(LatLonZoom.GetXMLTag()))
                        {
                            sourceMapView = new LatLonZoom(context, ContinuousCoordinateSystem.theInstance);
                            flag = true;
                        }
                    }
                }
                else
                {
                    if (xMLTagReader.TagIs(referenceMapViewTag))
                    {
                        XMLTagReader xMLTagReader3 = context.NewTagReader(referenceMapViewTag);
                        while (xMLTagReader3.FindNextStartTag())
                        {
                            if (xMLTagReader3.TagIs(MapPosition.GetXMLTag(context.version)))
                            {
                                referenceMapView =
                                    new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
                            }
                        }
                    }
                }
            }

            if (referenceMapView == null)
            {
                throw new InvalidMashupFile(context, "No " + referenceMapViewTag + " tag in LayerView.");
            }

            if (flag == _locked)
            {
                throw new InvalidMashupFile(context, "locked flag disagrees with " + sourceMapViewTag + " element.");
            }
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(GetXMLTag());
            writer.WriteAttributeString(lockedAttribute, locked.ToString(CultureInfo.InvariantCulture));
            if (!locked)
            {
                writer.WriteStartElement(sourceMapViewTag);
                sourceMapView.WriteXML(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(referenceMapViewTag);
            referenceMapView.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public MapPosition GetReferenceMapView()
        {
            return referenceMapView;
        }

        public LatLonZoom GetSourceMapView()
        {
            if (locked)
            {
                return referenceMapView.llz;
            }

            return sourceMapView;
        }
    }
}
