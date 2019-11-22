using System;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class TransparencyColor
    {
        private const string TransparencyColorTag = "TransparencyColor";
        private const string HaloSizeAttr = "HaloSize";
        private const string FuzzAttr = "Fuzz";
        private Pixel _color;
        private int _fuzz;
        private int _halo;

        public Pixel color
        {
            get
            {
                return _color;
            }
        }

        public int fuzz
        {
            get
            {
                return _fuzz;
            }
        }

        public int halo
        {
            get
            {
                return _halo;
            }
        }

        public TransparencyColor(Pixel color, int fuzz, int halo)
        {
            _color = new Pixel(color.r, color.g, color.b, 255);
            _fuzz = Math.Max(0, fuzz);
            _halo = Math.Max(0, halo);
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(fuzz);
            hash.Accumulate(halo);
            color.AccumulateRobustHash(hash);
        }

        internal void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("TransparencyColor");
            writer.WriteAttributeString("Fuzz", fuzz.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("HaloSize", halo.ToString(CultureInfo.InvariantCulture));
            color.WriteXML(writer);
            writer.WriteEndElement();
        }

        public TransparencyColor(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("TransparencyColor");
            _fuzz = TransparencyOptions.FuzzRange.Parse(context, "Fuzz");
            _halo = TransparencyOptions.HaloSizeRange.Parse(context, "HaloSize");
            bool flag = false;
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(Pixel.GetXMLTag()))
                {
                    Pixel pixel = new Pixel(context);
                    _color = new Pixel(pixel.r, pixel.g, pixel.b, 255);
                    flag = true;
                }
            }

            if (!flag)
            {
                throw new InvalidMashupFile(context,
                    string.Format("TransparencyColor has no %1 tag", Pixel.GetXMLTag()));
            }
        }

        public static string GetXMLTag()
        {
            return "TransparencyColor";
        }
    }
}
