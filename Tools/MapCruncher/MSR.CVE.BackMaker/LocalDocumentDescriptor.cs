using System.Globalization;
using System.IO;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LocalDocumentDescriptor
    {
        private string _filename;
        private int _pageNumber;
        private static string LocalDocumentFilenameAttr = "Filename";
        private static string LocalDocumentPageNumberAttr = "PageNumber";

        public LocalDocumentDescriptor(string filename, int pageNumber)
        {
            _filename = filename;
            _pageNumber = pageNumber;
        }

        public int GetPageNumber()
        {
            return _pageNumber;
        }

        public string GetDefaultDisplayName()
        {
            return Path.GetFileNameWithoutExtension(_filename);
        }

        public string GetFilesystemAbsolutePath()
        {
            return Path.GetFullPath(_filename);
        }

        public LocalDocumentDescriptor GetLocalDocumentDescriptor()
        {
            return this;
        }

        public void ValidateFilename()
        {
            if (!File.Exists(_filename))
            {
                throw new InvalidFileContentsException(string.Format("SourceMap file reference {0} invalid",
                    _filename));
            }
        }

        public static string GetXMLTag()
        {
            return "LocalDocumentDescriptor";
        }

        public void WriteXML(MashupWriteContext wc, string pathBase)
        {
            string value = _filename;
            string directoryName = Path.GetDirectoryName(_filename);
            if (pathBase != null && pathBase.ToLower().Equals(directoryName.ToLower()))
            {
                value = Path.GetFileName(_filename);
            }

            wc.writer.WriteStartElement(GetXMLTag());
            wc.writer.WriteAttributeString(LocalDocumentFilenameAttr, value);
            wc.writer.WriteAttributeString(LocalDocumentPageNumberAttr,
                _pageNumber.ToString(CultureInfo.InvariantCulture));
            wc.writer.WriteEndElement();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(_filename);
            hash.Accumulate(_pageNumber);
        }

        public LocalDocumentDescriptor(MashupParseContext context, string pathBase)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            string requiredAttribute = context.GetRequiredAttribute(LocalDocumentFilenameAttr);
            _filename = Path.Combine(pathBase, requiredAttribute);
            _pageNumber = context.GetRequiredAttributeInt(LocalDocumentPageNumberAttr);
            xMLTagReader.SkipAllSubTags();
        }
    }
}
