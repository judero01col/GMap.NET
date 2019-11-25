using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LayerView : IMapView, ICurrentView
    {
        private MapPosition lockedView;

        public Layer layer
        {
            get;
        }

        public object GetViewedObject()
        {
            return layer;
        }

        public LayerView(Layer layer, MapPosition lockedView)
        {
            this.layer = layer;
            this.lockedView = lockedView;
        }

        public static string GetXMLTag()
        {
            return "LayerView";
        }

        public LayerView(Layer layer, MashupParseContext context)
        {
            this.layer = layer;
            bool flag = false;
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(MapPosition.GetXMLTag(context.version)))
                {
                    lockedView = new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
                    flag = true;
                }
            }

            if (!flag)
            {
                throw new InvalidMashupFile(context, "No LatLonZoom tag in LayerView.");
            }
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(GetXMLTag());
            lockedView.WriteXML(writer);
            writer.WriteEndElement();
        }

        public MapPosition GetReferenceMapView()
        {
            return lockedView;
        }

        public LatLonZoom GetSourceMapView()
        {
            return lockedView.llz;
        }
    }
}
