using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class RenderToS3Options : RenderToOptions
    {
        private DirtyString _s3credentialsFilename;
        private DirtyString _s3bucket;
        private DirtyString _s3pathPrefix;
        public static string xmlTag = "RenderToS3";
        private static string attr_s3credentialsFilename = "CredentialsFilename";
        private static string attr_s3bucket = "Bucket";
        private static string attr_s3pathPrefix = "PathPrefix";

        public string s3credentialsFilename
        {
            get
            {
                return _s3credentialsFilename.myValue;
            }
            set
            {
                _s3credentialsFilename.myValue = value;
            }
        }

        public string s3bucket
        {
            get
            {
                return _s3bucket.myValue;
            }
            set
            {
                _s3bucket.myValue = value;
            }
        }

        public string s3pathPrefix
        {
            get
            {
                return _s3pathPrefix.myValue;
            }
            set
            {
                _s3pathPrefix.myValue = value;
            }
        }

        public RenderToS3Options(DirtyEvent parentDirtyEvent)
        {
            _s3credentialsFilename = new DirtyString(parentDirtyEvent);
            _s3bucket = new DirtyString(parentDirtyEvent);
            _s3pathPrefix = new DirtyString(parentDirtyEvent);
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(xmlTag);
            writer.WriteAttributeString(attr_s3credentialsFilename, s3credentialsFilename);
            writer.WriteAttributeString(attr_s3bucket, s3bucket);
            writer.WriteAttributeString(attr_s3pathPrefix, s3pathPrefix);
            writer.WriteEndElement();
        }

        public RenderToS3Options(MashupParseContext context, DirtyEvent parentDirtyEvent)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(xmlTag);
            _s3credentialsFilename = new DirtyString(parentDirtyEvent);
            s3credentialsFilename = context.GetRequiredAttribute(attr_s3credentialsFilename);
            _s3bucket = new DirtyString(parentDirtyEvent);
            s3bucket = context.GetRequiredAttribute(attr_s3bucket);
            _s3pathPrefix = new DirtyString(parentDirtyEvent);
            s3pathPrefix = context.GetRequiredAttribute(attr_s3pathPrefix);
            xMLTagReader.SkipAllSubTags();
        }

        public override string ToString()
        {
            return string.Format("s3:{0}/{1}", s3bucket, s3pathPrefix);
        }
    }
}
