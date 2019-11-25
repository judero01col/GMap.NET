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

        public string versionNumberString
        {
            get;
        }

        public static List<MashupXMLSchemaVersion> AcceptedVersions
        {
            get;
        }

        protected MashupXMLSchemaVersion(string versionNumberString)
        {
            this.versionNumberString = versionNumberString;
        }

        public void WriteXMLAttribute(XmlTextWriter writer)
        {
            writer.WriteAttributeString("Version", versionNumberString);
        }

        public static MashupXMLSchemaVersion ReadXMLAttribute(XmlTextReader reader)
        {
            string versionString = reader.GetAttribute("Version");
            MashupXMLSchemaVersion mashupXMLSchemaVersion =
                AcceptedVersions.Find((MashupXMLSchemaVersion vi) => vi.versionNumberString == versionString);
            if (mashupXMLSchemaVersion == null)
            {
                throw new InvalidMashupFile(reader, string.Format("Unknown mashup file version {0}", versionString));
            }

            return mashupXMLSchemaVersion;
        }

        static MashupXMLSchemaVersion()
        {
            AcceptedVersions = new List<MashupXMLSchemaVersion>();
            AcceptedVersions.Add(CurrentSchema.schema);
            AcceptedVersions.Add(NoTagIdentities.schema);
            AcceptedVersions.Add(ViewsNotAsWellPreservedSchema.schema);
            AcceptedVersions.Add(SingleMaxZoomForEntireMashupSchema.schema);
            AcceptedVersions.Add(SourceMapInfoAsCharDataSchema.schema);
            AcceptedVersions.Add(InlineSourceMapInfoSchema.schema);
            AcceptedVersions.Add(MonolithicMapPositionsSchema.schema);
        }
    }
}
