using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class SourceMapInfo
    {
        private const string SourceMapInfoTag = "SourceMapInfo";
        public const string SourceMapFileURLTag = "MapFileURL";
        public const string SourceMapHomePageTag = "MapHomePage";
        public const string SourceMapDescriptionTag = "MapDescription";
        public const string UrlAttr = "url";
        private DirtyEvent dirtyEvent;
        private string _mapFileURL = "";
        private string _mapHomePage = "";
        private string _mapDescription = "";

        public string mapFileURL
        {
            get
            {
                return _mapFileURL;
            }
            set
            {
                if (_mapFileURL != value)
                {
                    _mapFileURL = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public string mapHomePage
        {
            get
            {
                return _mapHomePage;
            }
            set
            {
                if (_mapHomePage != value)
                {
                    _mapHomePage = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public string mapDescription
        {
            get
            {
                return _mapDescription;
            }
            set
            {
                if (_mapDescription != value)
                {
                    _mapDescription = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public SourceMapInfo(DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
        }

        public static string GetXMLTag()
        {
            return "SourceMapInfo";
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("SourceMapInfo");
            writer.WriteStartElement("MapFileURL");
            writer.WriteAttributeString("url", mapFileURL);
            writer.WriteEndElement();
            writer.WriteStartElement("MapHomePage");
            writer.WriteAttributeString("url", mapHomePage);
            writer.WriteEndElement();
            XMLUtils.WriteStringXml(writer, "MapDescription", mapDescription);
            writer.WriteEndElement();
        }

        public SourceMapInfo(MashupParseContext context, DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs("MapFileURL"))
                {
                    if (context.version == SourceMapInfoAsCharDataSchema.schema)
                    {
                        _mapFileURL = XMLUtils.ReadStringXml(context, "MapFileURL");
                    }
                    else
                    {
                        XMLTagReader xMLTagReader2 = context.NewTagReader("MapFileURL");
                        _mapFileURL = context.GetRequiredAttribute("url");
                        xMLTagReader2.SkipAllSubTags();
                    }
                }
                else
                {
                    if (xMLTagReader.TagIs("MapHomePage"))
                    {
                        if (context.version == SourceMapInfoAsCharDataSchema.schema)
                        {
                            _mapHomePage = XMLUtils.ReadStringXml(context, "MapHomePage");
                        }
                        else
                        {
                            XMLTagReader xMLTagReader3 = context.NewTagReader("MapHomePage");
                            _mapHomePage = context.GetRequiredAttribute("url");
                            xMLTagReader3.SkipAllSubTags();
                        }
                    }
                    else
                    {
                        if (xMLTagReader.TagIs("MapDescription"))
                        {
                            _mapDescription = XMLUtils.ReadStringXml(context, "MapDescription");
                        }
                    }
                }
            }
        }
    }
}
