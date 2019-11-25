using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderClip
    {
        public MapRectangle rect
        {
            get;
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
                    context.AssertUnique(rect);
                    rect = new MapRectangle(context, MercatorCoordinateSystem.theInstance);
                }
            }
        }

        public void WriteXML(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement(GetXMLTag());
            if (rect != null)
            {
                rect.WriteXML(wc.writer);
            }

            wc.writer.WriteEndElement();
        }
    }
}
