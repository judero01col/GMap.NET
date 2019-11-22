using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class RangeDescriptor
    {
        public const string RangeDescriptorTag = "RangeDescriptor";
        public const string QuadTreeLocationAttr = "QuadTreeLocation";
        public TileAddress tileAddress;

        public RangeDescriptor(TileAddress tileAddress)
        {
            this.tileAddress = tileAddress;
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("RangeDescriptor");
            tileAddress.WriteXMLToAttributes(writer);
            string quadKey = VENamingScheme.GetQuadKey(tileAddress);
            writer.WriteAttributeString("QuadTreeLocation", quadKey);
            FodderSupport.WriteQuadTreeFodderString(writer, FodderSupport.DigitsToLetters(quadKey));
            writer.WriteEndElement();
        }
    }
}
