using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderClip
    {
        private MapRectangle _rect;

        public MapRectangle rect
        {
            get
            {
                return this._rect;
            }
        }

        public static string GetXMLTag()
        {
            return "RenderClip";
        }

        public RenderClip()
        {
        }

        public RenderClip(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(MapRectangle.GetXMLTag()))
                {
                    context.AssertUnique(this._rect);
                    this._rect = new MapRectangle(context, MercatorCoordinateSystem.theInstance);
                }
            }
        }

        public void WriteXML(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement(GetXMLTag());
            if (this._rect != null)
            {
                this._rect.WriteXML(wc.writer);
            }

            wc.writer.WriteEndElement();
        }
    }
}
