using System;
using System.Collections.Generic;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RegistrationDefinition : IRobustlyHashable
    {
        private List<PositionAssociation> associationList = new List<PositionAssociation>();
        private int nextPinId;
        public bool isLocked;
        private TransformationStyle _warpStyle = TransformationStyleFactory.getDefaultTransformationStyle();
        public DirtyEvent dirtyEvent;
        private static string RegistrationDefinitionTag = "RegistrationDefinition";

        public TransformationStyle warpStyle
        {
            get
            {
                return _warpStyle;
            }
            set
            {
                if (_warpStyle != value)
                {
                    _warpStyle = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public RegistrationDefinition(DirtyEvent dirtyEvent)
        {
            this.dirtyEvent = new DirtyEvent(dirtyEvent);
        }

        public RegistrationDefinition(RegistrationDefinition prototype, DirtyEvent dirtyEvent)
        {
            this.dirtyEvent = new DirtyEvent(dirtyEvent);
            if (prototype != null)
            {
                associationList.AddRange(prototype.associationList);
                isLocked = prototype.isLocked;
            }

            SetNextPinID();
        }

        private void SetNextPinID()
        {
            int num = -1;
            foreach (PositionAssociation current in associationList)
            {
                num = Math.Max(num, current.pinId);
            }

            nextPinId = num + 1;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            foreach (PositionAssociation current in associationList)
            {
                current.AccumulateRobustHash(hash);
            }

            warpStyle.AccumulateRobustHash(hash);
        }

        public override int GetHashCode()
        {
            return RobustHashTools.GetHashCode(this);
        }

        public void AddAssociation(PositionAssociation positionAssociaton)
        {
            if (positionAssociaton.pinId == -1)
            {
                positionAssociaton.pinId = nextPinId;
            }

            if (positionAssociaton.associationName == "")
            {
                positionAssociaton.associationName = string.Format("Pin{0}", positionAssociaton.pinId);
            }

            nextPinId = Math.Max(nextPinId, positionAssociaton.pinId) + 1;
            associationList.Add(positionAssociaton);
            dirtyEvent.SetDirty();
        }

        public void RemoveAssociation(PositionAssociation assoc)
        {
            associationList.Remove(assoc);
            dirtyEvent.SetDirty();
        }

        public List<PositionAssociation> GetAssociationList()
        {
            return associationList;
        }

        internal PositionAssociation GetAssocByName(string name)
        {
            foreach (PositionAssociation current in associationList)
            {
                if (current.associationName == name)
                {
                    return current;
                }
            }

            return null;
        }

        internal bool ReadyToLock()
        {
            return associationList.Count >= warpStyle.getCorrespondencesRequired();
        }

        internal string[] GetLockStatusText()
        {
            return warpStyle.getDescriptionStrings(associationList.Count).ToArray();
        }

        public static string GetXMLTag()
        {
            return RegistrationDefinitionTag;
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(RegistrationDefinitionTag);
            warpStyle.WriteXML(writer);
            foreach (PositionAssociation current in GetAssociationList())
            {
                current.WriteXML(writer);
            }

            writer.WriteEndElement();
        }

        public RegistrationDefinition(MashupParseContext context, DirtyEvent dirtyEvent)
        {
            this.dirtyEvent = new DirtyEvent(dirtyEvent);
            XMLTagReader xMLTagReader = context.NewTagReader(RegistrationDefinitionTag);
            warpStyle = TransformationStyleFactory.ReadFromXMLAttribute(context);
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(PositionAssociation.XMLTag()))
                {
                    AddAssociation(new PositionAssociation(context, dirtyEvent));
                }
            }
        }
    }
}
