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
                switch (whichPosition)
                {
                    case WhichPosition.image:
                        return assoc.imagePosition;
                    case WhichPosition.source:
                        return assoc.sourcePosition;
                    case WhichPosition.global:
                        return assoc.globalPosition;
                    default:
                        throw new Exception("booogus.");
                }
            }
        }

        public string associationName
        {
            get
            {
                return assoc.associationName;
            }
        }

        public int pinId
        {
            get
            {
                return assoc.pinId;
            }
        }

        public PositionAssociationView(PositionAssociation assoc, WhichPosition whichPosition)
        {
            this.assoc = assoc;
            this.whichPosition = whichPosition;
        }
    }
}
