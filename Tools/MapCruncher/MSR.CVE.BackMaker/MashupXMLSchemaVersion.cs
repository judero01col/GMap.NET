using System.Collections.Generic;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class MashupXMLSchemaVersion
    {
        private const string mashupFileVersionNumberTag = "Version";
        public const string llLatAttr = "lat";
        public const string llLonAttr = "lon";
        public const string llzZoomAttr = "zoom";
        public const string mapPositionTag = "MapPosition";
        public const string mpStyleAttr = "style";
        private string _versionNumberString;
        private static List<MashupXMLSchemaVersion> _AcceptedVersions;

        public string versionNumberString
        {
            get
            {
                return _versionNumberString;
            }
        }

        public static List<MashupXMLSchemaVersion> AcceptedVersions
        {
            get
            {
                return _AcceptedVersions;
            }
        }

        protected MashupXMLSchemaVersion(string versionNumberString)
        {
            _versionNumberString = versionNumberString;
        }

        public void WriteXMLAttribute(XmlTextWriter writer)
        {
            writer.WriteAttributeString("Version", versionNumberString);
        }

        public static MashupXMLSchemaVersion ReadXMLAttribute(XmlTextReader reader)
        {
            string versionString = reader.GetAttribute("Version");
            MashupXMLSchemaVersion mashupXMLSchemaVersion =
                AcceptedVersions.Find((MashupXMLSchemaVersion vi) => vi._versionNumberString == versionString);
            if (mashupXMLSchemaVersion == null)
            {
                throw new InvalidMashupFile(reader, string.Format("Unknown mashup file version {0}", versionString));
            }

            return mashupXMLSchemaVersion;
        }

        static MashupXMLSchemaVersion()
        {
            _AcceptedVersions = new List<MashupXMLSchemaVersion>();
            _AcceptedVersions.Add(CurrentSchema.schema);
            _AcceptedVersions.Add(NoTagIdentities.schema);
            _AcceptedVersions.Add(ViewsNotAsWellPreservedSchema.schema);
            _AcceptedVersions.Add(SingleMaxZoomForEntireMashupSchema.schema);
            _AcceptedVersions.Add(SourceMapInfoAsCharDataSchema.schema);
            _AcceptedVersions.Add(InlineSourceMapInfoSchema.schema);
            _AcceptedVersions.Add(MonolithicMapPositionsSchema.schema);
        }
    }
}
