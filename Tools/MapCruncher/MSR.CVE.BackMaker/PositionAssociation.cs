using System;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class PositionAssociation : IRobustlyHashable
    {
        private int _pinId;
        private string _associationName;
        private DirtyEvent dirtyEvent;
        private static string PositionAssociationTag = "PositionAssociation";
        private static string associationNameAttr = "associationName";
        private static string pinIdAttr = "pinId";
        private static string SourcePositionTag = "SourcePosition";
        private static string GlobalPositionTag = "GlobalPosition";

        public int pinId
        {
            get
            {
                return _pinId;
            }
            set
            {
                _pinId = value;
                dirtyEvent.SetDirty();
            }
        }

        public string associationName
        {
            get
            {
                return _associationName;
            }
            set
            {
                _associationName = value;
                dirtyEvent.SetDirty();
            }
        }

        public string qualityMessage
        {
            get;
            set;
        }

        public DisplayablePosition imagePosition
        {
            get;
        }

        public DisplayablePosition sourcePosition
        {
            get;
            private set;
        }

        public DisplayablePosition globalPosition
        {
            get;
            private set;
        }

        public PositionAssociation(string associationName, LatLonZoom imagePosition, LatLonZoom sourcePosition,
            LatLonZoom globalPosition, DirtyEvent dirtyEvent)
        {
            this.dirtyEvent = dirtyEvent;
            _pinId = -1;
            _associationName = associationName;
            this.imagePosition = new DisplayablePosition(imagePosition);
            this.sourcePosition = new DisplayablePosition(sourcePosition);
            this.globalPosition = new DisplayablePosition(globalPosition);
        }

        public override int GetHashCode()
        {
            return sourcePosition.GetHashCode() ^ globalPosition.GetHashCode() ^ _pinId ^
                   _associationName.GetHashCode();
        }

        public void UpdateAssociation(LatLonZoom sourceLLZ, LatLonZoom globalLLZ)
        {
            sourcePosition = new DisplayablePosition(sourceLLZ);
            globalPosition = new DisplayablePosition(globalLLZ);
            dirtyEvent.SetDirty();
        }

        public static string XMLTag()
        {
            return PositionAssociationTag;
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(PositionAssociationTag);
            writer.WriteAttributeString(pinIdAttr, _pinId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(associationNameAttr, _associationName);
            writer.WriteStartElement(SourcePositionTag);
            sourcePosition.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteStartElement(GlobalPositionTag);
            globalPosition.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public PositionAssociation(MashupParseContext context, DirtyEvent dirtyEvent)
        {
            this.dirtyEvent = dirtyEvent;
            XMLTagReader xMLTagReader = context.NewTagReader(PositionAssociationTag);
            _pinId = -1;
            context.GetAttributeInt(pinIdAttr, ref _pinId);
            if ((associationName = context.reader.GetAttribute(associationNameAttr)) == null)
            {
                associationName = "";
            }

            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(SourcePositionTag))
                {
                    XMLTagReader xMLTagReader2 = context.NewTagReader(SourcePositionTag);
                    while (xMLTagReader2.FindNextStartTag())
                    {
                        if (xMLTagReader2.TagIs(DisplayablePosition.GetXMLTag(context.version)))
                        {
                            sourcePosition =
                                new DisplayablePosition(context, ContinuousCoordinateSystem.theInstance);
                            imagePosition = new DisplayablePosition(sourcePosition.pinPosition);
                        }
                    }
                }
                else
                {
                    if (xMLTagReader.TagIs(GlobalPositionTag))
                    {
                        XMLTagReader xMLTagReader3 = context.NewTagReader(GlobalPositionTag);
                        while (xMLTagReader3.FindNextStartTag())
                        {
                            if (xMLTagReader3.TagIs(DisplayablePosition.GetXMLTag(context.version)))
                            {
                                globalPosition =
                                    new DisplayablePosition(context, MercatorCoordinateSystem.theInstance);
                            }
                        }
                    }
                }
            }

            if (sourcePosition == null || globalPosition == null)
            {
                throw new Exception(string.Format("Pin {0} does not have a source and/or global position defined",
                    associationName));
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            sourcePosition.pinPosition.AccumulateRobustHash(hash);
            globalPosition.pinPosition.AccumulateRobustHash(hash);
        }
    }
}
