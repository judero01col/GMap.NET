using System.Drawing;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class Pixel
    {
        private const string PixelValuesTag = "PixelValues";
        private const string RedAttr = "r";
        private const string GreenAttr = "g";
        private const string BlueAttr = "b";
        private const string AAttr = "a";
        private PixelStruct p;
        private static RangeInt byteRange = new RangeInt(0, 255);

        public byte a
        {
            get
            {
                return p.a;
            }
        }

        public byte r
        {
            get
            {
                return p.r;
            }
        }

        public byte g
        {
            get
            {
                return p.g;
            }
        }

        public byte b
        {
            get
            {
                return p.b;
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            p.AccumulateRobustHash(hash);
        }

        public Color ToColor()
        {
            return p.ToColor();
        }

        public static bool operator ==(Pixel p1, Pixel p2)
        {
            return p1.p == p2.p;
        }

        public static bool operator !=(Pixel p1, Pixel p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            return obj is Pixel && this == (Pixel)obj;
        }

        public override int GetHashCode()
        {
            return p.GetHashCode();
        }

        public Pixel()
        {
            p = default(PixelStruct);
        }

        public Pixel(byte r, byte g, byte b, byte a)
        {
            p.a = a;
            p.r = r;
            p.g = g;
            p.b = b;
        }

        public Pixel(Color c)
        {
            p.a = c.A;
            p.r = c.R;
            p.g = c.G;
            p.b = c.B;
        }

        internal void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("PixelValues");
            writer.WriteAttributeString("r", p.r.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("g", p.g.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("b", p.b.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("a", p.a.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        public Pixel(MashupParseContext context)
        {
            p.r = 0;
            p.g = 0;
            p.b = 0;
            p.a = 0;
            XMLTagReader xMLTagReader = context.NewTagReader("PixelValues");
            p.r = (byte)byteRange.Parse(context, "r");
            p.g = (byte)byteRange.Parse(context, "g");
            p.b = (byte)byteRange.Parse(context, "b");
            p.a = (byte)byteRange.Parse(context, "a");
            xMLTagReader.SkipAllSubTags();
        }

        public static string GetXMLTag()
        {
            return "PixelValues";
        }
    }
}
