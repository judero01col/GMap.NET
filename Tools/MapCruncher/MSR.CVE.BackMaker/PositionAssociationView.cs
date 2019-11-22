using System;

namespace MSR.CVE.BackMaker
{
    public class PositionAssociationView
    {
        public enum WhichPosition
        {
            image,
            source,
            global
        }

        private PositionAssociation assoc;
        private WhichPosition whichPosition;

        public DisplayablePosition position
        {
            get
            {
                switch (this.whichPosition)
                {
                    case WhichPosition.image:
                        return this.assoc.imagePosition;
                    case WhichPosition.source:
                        return this.assoc.sourcePosition;
                    case WhichPosition.global:
                        return this.assoc.globalPosition;
                    default:
                        throw new Exception("booogus.");
                }
            }
        }

        public string associationName
        {
            get
            {
                return this.assoc.associationName;
            }
        }

        public int pinId
        {
            get
            {
                return this.assoc.pinId;
            }
        }

        public PositionAssociationView(PositionAssociation assoc, WhichPosition whichPosition)
        {
            this.assoc = assoc;
            this.whichPosition = whichPosition;
        }
    }
}
