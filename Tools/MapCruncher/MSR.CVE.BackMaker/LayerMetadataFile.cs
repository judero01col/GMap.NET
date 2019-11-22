using System.IO;
using System.Text;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LayerMetadataFile
    {
        private const string filename = "LayerMetadata.xml";
        private RenderOutputMethod renderOutputMethod;
        private EncodableHash _encodableHash;
        private static string LayerMetadataTag = "LayerMetadata";

        public EncodableHash encodableHash
        {
            get
            {
                return _encodableHash;
            }
        }

        public LayerMetadataFile(RenderOutputMethod renderOutputMethod, EncodableHash encodableHash)
        {
            this.renderOutputMethod = renderOutputMethod;
            _encodableHash = encodableHash;
        }

        public static LayerMetadataFile Read(RenderOutputMethod outputMethod)
        {
            LayerMetadataFile layerMetadataFile = null;
            Stream input = outputMethod.ReadFile("LayerMetadata.xml");
            XmlTextReader reader = new XmlTextReader(input);
            MashupParseContext mashupParseContext = new MashupParseContext(reader);
            using (mashupParseContext)
            {
                while (mashupParseContext.reader.Read())
                {
                    if (mashupParseContext.reader.NodeType == XmlNodeType.Element &&
                        mashupParseContext.reader.Name == LayerMetadataTag)
                    {
                        layerMetadataFile = new LayerMetadataFile(outputMethod, mashupParseContext);
                        break;
                    }
                }

                mashupParseContext.Dispose();
            }

            if (layerMetadataFile == null)
            {
                throw new InvalidMashupFile(mashupParseContext,
                    string.Format("{0} doesn't appear to be a valid {1}",
                        outputMethod.GetUri("LayerMetadata.xml"),
                        LayerMetadataTag));
            }

            return layerMetadataFile;
        }

        private LayerMetadataFile(RenderOutputMethod renderOutputMethod, MashupParseContext context)
        {
            this.renderOutputMethod = renderOutputMethod;
            XMLTagReader xMLTagReader = context.NewTagReader(LayerMetadataTag);
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs("StrongHash"))
                {
                    context.AssertUnique(_encodableHash);
                    _encodableHash = new EncodableHash(context);
                }
            }

            context.AssertPresent(_encodableHash, "StrongHash");
        }

        public void Write()
        {
            XmlTextWriter xmlTextWriter =
                new XmlTextWriter(renderOutputMethod.CreateFile("LayerMetadata.xml", "text/xml"), Encoding.UTF8);
            using (xmlTextWriter)
            {
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteStartDocument(true);
                WriteXML(xmlTextWriter);
                xmlTextWriter.Close();
            }
        }

        private void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(LayerMetadataTag);
            _encodableHash.WriteXML(writer);
            writer.WriteEndElement();
        }
    }
}
