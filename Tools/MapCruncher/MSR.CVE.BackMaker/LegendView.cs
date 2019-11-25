using System.Globalization;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LegendView : IMapView, ICurrentView
    {
        private LatLonZoom sourceMapView;
        private MapPosition referenceMapView;
        private static string previewAttr = "ShowingPreview";
        private static string sourceMapViewTag = "SourceMapPosition";
        private static string referenceMapViewTag = "ReferenceMapPosition";

        public Legend legend
        {
            get;
        }

        public bool showingPreview
        {
            get;
        }

        public object GetViewedObject()
        {
            return legend;
        }

        public LegendView(Legend legend, bool showingPreview, LatLonZoom sourceMapView, MapPosition referenceMapView)
        {
            this.legend = legend;
            this.showingPreview = showingPreview;
            this.sourceMapView = sourceMapView;
            this.referenceMapView = referenceMapView;
        }

        public LegendView(Legend legend, MashupParseContext context)
        {
            this.legend = legend;
            object obj = null;
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            showingPreview = context.GetRequiredAttributeBoolean(previewAttr);
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
                            context.AssertUnique(obj);
                            obj = new object();
                        }
                    }
                }
                else
                {
                    if (xMLTagReader.TagIs(referenceMapViewTag))
                    {
                        context.AssertUnique(referenceMapView);
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

            context.AssertPresent(obj, sourceMapViewTag);
            context.AssertPresent(referenceMapView, referenceMapViewTag);
        }

        public static string GetXMLTag()
        {
            return "LegendView";
        }

        public void WriteXML(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement(GetXMLTag());
            wc.writer.WriteAttributeString(previewAttr, showingPreview.ToString(CultureInfo.InvariantCulture));
            wc.writer.WriteStartElement(sourceMapViewTag);
            sourceMapView.WriteXML(wc.writer);
            wc.writer.WriteEndElement();
            wc.writer.WriteStartElement(referenceMapViewTag);
            referenceMapView.WriteXML(wc.writer);
            wc.writer.WriteEndElement();
            wc.writer.WriteEndElement();
        }

        public MapPosition GetReferenceMapView()
        {
            return referenceMapView;
        }

        public LatLonZoom GetSourceMapView()
        {
            return sourceMapView;
        }
    }
}
