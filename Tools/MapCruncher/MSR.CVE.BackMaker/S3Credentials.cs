using System.IO;
using System.Text;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    internal class S3Credentials
    {
        private const string S3CredentialsFileTag = "S3Credentials";
        private const string S3VersionAttr = "Version";
        private const string S3VersionValue = "1.0";
        private const string AccessKeyIdTag = "AccessKeyId";
        private const string SecretAccessKeyTag = "SecretAccessKeyTag";
        private const string ValueAttr = "Value";
        private string _fileName;
        private string _accessKeyId;
        private string _secretAccessKey;

        public string fileName
        {
            get
            {
                return _fileName;
            }
        }

        public string accessKeyId
        {
            get
            {
                return _accessKeyId;
            }
            set
            {
                _accessKeyId = value;
            }
        }

        public string secretAccessKey
        {
            get
            {
                return _secretAccessKey;
            }
            set
            {
                _secretAccessKey = value;
            }
        }

        public S3Credentials(string fileName, bool createIfFileAbsent)
        {
            _fileName = fileName;
            Stream input;
            try
            {
                input = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            }
            catch (FileNotFoundException)
            {
                if (createIfFileAbsent)
                {
                    _accessKeyId = "";
                    _secretAccessKey = "";
                    return;
                }

                throw;
            }

            D.Assert(fileName == null || Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
            bool flag = false;
            XmlTextReader reader = new XmlTextReader(input);
            MashupParseContext mashupParseContext = new MashupParseContext(reader);
            using (mashupParseContext)
            {
                while (mashupParseContext.reader.Read() && !flag)
                {
                    if (mashupParseContext.reader.NodeType == XmlNodeType.Element &&
                        mashupParseContext.reader.Name == "S3Credentials")
                    {
                        flag = true;
                        ReadXML(mashupParseContext);
                    }
                }

                mashupParseContext.Dispose();
            }

            if (!flag)
            {
                throw new InvalidMashupFile(mashupParseContext,
                    string.Format("{0} doesn't appear to be a valid {1} file.", fileName, "S3Credentials"));
            }
        }

        public void ReadXML(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("S3Credentials");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs("AccessKeyId"))
                {
                    context.AssertUnique(_accessKeyId);
                    XMLTagReader xMLTagReader2 = context.NewTagReader("AccessKeyId");
                    _accessKeyId = context.reader.GetAttribute("Value");
                    xMLTagReader2.SkipAllSubTags();
                }
                else
                {
                    if (xMLTagReader.TagIs("SecretAccessKeyTag"))
                    {
                        context.AssertUnique(_secretAccessKey);
                        XMLTagReader xMLTagReader3 = context.NewTagReader("SecretAccessKeyTag");
                        _secretAccessKey = context.reader.GetAttribute("Value");
                        xMLTagReader3.SkipAllSubTags();
                    }
                }
            }

            context.AssertPresent(_accessKeyId, "AccessKeyId");
            context.AssertPresent(_secretAccessKey, "SecretAccessKeyTag");
        }

        public void WriteXML()
        {
            D.Assert(_fileName != null);
            WriteXML(_fileName);
        }

        private void WriteXML(string saveName)
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(saveName, Encoding.UTF8);
            using (xmlTextWriter)
            {
                MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
                WriteXML(wc);
            }
        }

        private void WriteXML(MashupWriteContext wc)
        {
            XmlTextWriter writer = wc.writer;
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument(true);
            writer.WriteStartElement("S3Credentials");
            writer.WriteAttributeString("Version", "1.0");
            writer.WriteStartElement("AccessKeyId");
            writer.WriteAttributeString("Value", _accessKeyId);
            writer.WriteEndElement();
            writer.WriteStartElement("SecretAccessKeyTag");
            writer.WriteAttributeString("Value", _secretAccessKey);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Close();
        }
    }
}
