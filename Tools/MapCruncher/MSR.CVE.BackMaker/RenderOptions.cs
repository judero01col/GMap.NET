using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderOptions
    {
        private RenderToOptions _renderToOptions;
        private bool _publishSourceData = true;
        private bool _permitComposition = true;
        public DirtyEvent dirtyEvent;
        private static string RenderOptionsTag = "RenderOptions";
        private static string PublishSourceDataTag = "PublishSourceData";
        private static string PublishSourceDataValueAttr = "Value";
        private static string PermitCompositionTag = "PermitComposition";
        private static string PermitCompositionValueAttr = "Value";
        private static string OutputTileTypeTag = "OutputTileType";
        private static string OutputTileTypeAttr = "Type";
        private static string compatibility_RenderToFileOutputTag = "Output";

        public RenderToOptions renderToOptions
        {
            get
            {
                return _renderToOptions;
            }
            set
            {
                _renderToOptions = value;
                dirtyEvent.SetDirty();
            }
        }

        public bool publishSourceData
        {
            get
            {
                return _publishSourceData;
            }
            set
            {
                if (value != _publishSourceData)
                {
                    _publishSourceData = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public bool permitComposition
        {
            get
            {
                return _permitComposition;
            }
            set
            {
                if (value != _permitComposition)
                {
                    _permitComposition = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public OutputTileType outputTileType
        {
            get;
        } = OutputTileType.JPG;

        public RenderOptions(DirtyEvent parentDirtyEvent)
        {
            dirtyEvent = new DirtyEvent(parentDirtyEvent);
            _renderToOptions = new RenderToFileOptions(parentDirtyEvent);
        }

        public static string GetXMLTag()
        {
            return RenderOptionsTag;
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(RenderOptionsTag);
            writer.WriteStartElement(PublishSourceDataTag);
            writer.WriteAttributeString(PublishSourceDataValueAttr,
                publishSourceData.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteStartElement(PermitCompositionTag);
            writer.WriteAttributeString(PermitCompositionValueAttr,
                permitComposition.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteStartElement(OutputTileTypeTag);
            writer.WriteAttributeString(OutputTileTypeAttr, outputTileType.extn);
            writer.WriteEndElement();
            renderToOptions.WriteXML(writer);
            writer.WriteEndElement();
        }

        public RenderOptions(MashupParseContext context, DirtyEvent parentDirtyEvent,
            ref SingleMaxZoomForEntireMashupCompatibilityBlob blob)
        {
            dirtyEvent = new DirtyEvent(parentDirtyEvent);
            XMLTagReader xMLTagReader = context.NewTagReader(RenderOptionsTag);
            while (xMLTagReader.FindNextStartTag())
            {
                if (context.version == SingleMaxZoomForEntireMashupSchema.schema &&
                    xMLTagReader.TagIs(SingleMaxZoomForEntireMashupSchema.ZoomLevelsTag))
                {
                    blob = new SingleMaxZoomForEntireMashupCompatibilityBlob();
                    XMLTagReader xMLTagReader2 = context.NewTagReader(SingleMaxZoomForEntireMashupSchema.ZoomLevelsTag);
                    string attribute;
                    if ((attribute = context.reader.GetAttribute(SingleMaxZoomForEntireMashupSchema.MinZoomTag)) !=
                        null)
                    {
                        blob.minZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().Parse(context,
                            SingleMaxZoomForEntireMashupSchema.MinZoomTag,
                            attribute);
                    }

                    if ((attribute = context.reader.GetAttribute(SingleMaxZoomForEntireMashupSchema.MaxZoomTag)) !=
                        null)
                    {
                        blob.maxZoom = MercatorCoordinateSystem.theInstance.GetZoomRange().Parse(context,
                            SingleMaxZoomForEntireMashupSchema.MaxZoomTag,
                            attribute);
                    }

                    xMLTagReader2.SkipAllSubTags();
                }
                else
                {
                    if (xMLTagReader.TagIs(RenderToFileOptions.xmlTag))
                    {
                        _renderToOptions = new RenderToFileOptions(context, dirtyEvent);
                    }
                    else
                    {
                        if (xMLTagReader.TagIs(compatibility_RenderToFileOutputTag))
                        {
                            _renderToOptions = new RenderToFileOptions(context,
                                dirtyEvent,
                                compatibility_RenderToFileOutputTag);
                        }
                        else
                        {
                            if (xMLTagReader.TagIs(RenderToS3Options.xmlTag))
                            {
                                _renderToOptions = new RenderToS3Options(context, dirtyEvent);
                            }
                            else
                            {
                                if (xMLTagReader.TagIs(OutputTileTypeTag))
                                {
                                    XMLTagReader xMLTagReader3 = context.NewTagReader(OutputTileTypeTag);
                                    outputTileType =
                                        OutputTileType.Parse(context.reader.GetAttribute(OutputTileTypeAttr));
                                    xMLTagReader3.SkipAllSubTags();
                                }
                                else
                                {
                                    if (xMLTagReader.TagIs(PublishSourceDataTag))
                                    {
                                        XMLTagReader xMLTagReader4 = context.NewTagReader(PublishSourceDataTag);
                                        publishSourceData =
                                            context.GetRequiredAttributeBoolean(PublishSourceDataValueAttr);
                                        xMLTagReader4.SkipAllSubTags();
                                    }
                                    else
                                    {
                                        if (xMLTagReader.TagIs(PermitCompositionTag))
                                        {
                                            XMLTagReader xMLTagReader5 = context.NewTagReader(PermitCompositionTag);
                                            permitComposition =
                                                context.GetRequiredAttributeBoolean(PermitCompositionValueAttr);
                                            xMLTagReader5.SkipAllSubTags();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal string GetOutputTileSuffix()
        {
            return "." + outputTileType.extn;
        }
    }
}
