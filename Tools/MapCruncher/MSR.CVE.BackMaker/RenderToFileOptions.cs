using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class RenderToFileOptions : RenderToOptions
    {
        private DirtyString _outputFolder;
        public static string xmlTag = "RenderToFile";
        private static string OutputFolderAttr = "Folder";

        public string outputFolder
        {
            get
            {
                return _outputFolder.myValue;
            }
            set
            {
                _outputFolder.myValue = value;
            }
        }

        public RenderToFileOptions(DirtyEvent parentDirtyEvent)
        {
            _outputFolder = new DirtyString(parentDirtyEvent);
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(xmlTag);
            writer.WriteAttributeString(OutputFolderAttr, outputFolder);
            writer.WriteEndElement();
        }

        public RenderToFileOptions(MashupParseContext context, DirtyEvent parentDirtyEvent, string byTagName)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(byTagName);
            _outputFolder = new DirtyString(parentDirtyEvent);
            outputFolder = context.GetRequiredAttribute(OutputFolderAttr);
            xMLTagReader.SkipAllSubTags();
        }

        public RenderToFileOptions(MashupParseContext context, DirtyEvent parentDirtyEvent) : this(context,
            parentDirtyEvent,
            xmlTag)
        {
        }

        public override string ToString()
        {
            return string.Format("file:{0}", outputFolder);
        }
    }
}
