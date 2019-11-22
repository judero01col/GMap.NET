using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public abstract class IImageTransformer
    {
        private class QualitySortPair : IComparable
        {
            public PositionAssociation assoc;
            public double fitQuality;

            public QualitySortPair(PositionAssociation assoc, double fitQuality)
            {
                this.assoc = assoc;
                this.fitQuality = fitQuality;
            }

            public int CompareTo(object obj)
            {
                if (!(obj is QualitySortPair))
                {
                    return 1;
                }

                QualitySortPair qualitySortPair = (QualitySortPair)obj;
                int num = -fitQuality.CompareTo(qualitySortPair.fitQuality);
                if (num != 0)
                {
                    return num;
                }

                return assoc.associationName.CompareTo(qualitySortPair.assoc.associationName);
            }
        }

        protected RegistrationDefinition registration;
        protected InterpolationMode interpolationMode;
        protected IPointTransformer destLatLonToSourceTransformer;
        protected IPointTransformer sourceToDestLatLonTransformer;

        public IImageTransformer(RegistrationDefinition registration, InterpolationMode interpolationMode)
        {
            this.registration = new RegistrationDefinition(registration, new DirtyEvent());
            this.interpolationMode = interpolationMode;
        }

        internal abstract void doTransformImage(GDIBigLockedImage sourceImage, MapRectangle sourceBounds,
            GDIBigLockedImage destImage, MapRectangle destBounds);

        internal IPointTransformer getDestLatLonToSourceTransformer()
        {
            return destLatLonToSourceTransformer;
        }

        internal IPointTransformer getSourceToDestLatLonTransformer()
        {
            return sourceToDestLatLonTransformer;
        }

        public RegistrationDefinition getWarpedRegistration()
        {
            IPointTransformer pointTransformer = getSourceToDestLatLonTransformer();
            List<PositionAssociation> associationList = registration.GetAssociationList();
            List<QualitySortPair> list = new List<QualitySortPair>();
            for (int i = 0; i < associationList.Count; i++)
            {
                PositionAssociation positionAssociation = associationList[i];
                bool invertError;
                LatLon p = pointTransformer.getTransformedPoint(positionAssociation.sourcePosition.pinPosition.latlon,
                    out invertError);
                PositionAssociation positionAssociation2 = new PositionAssociation(positionAssociation.associationName,
                    positionAssociation.imagePosition.pinPosition,
                    new LatLonZoom(p.lat, p.lon, positionAssociation.sourcePosition.pinPosition.zoom),
                    positionAssociation.globalPosition.pinPosition,
                    new DirtyEvent());
                positionAssociation2.sourcePosition.invertError = invertError;
                positionAssociation2.sourcePosition.SetErrorPosition(DisplayablePosition.ErrorMarker.AsContributor,
                    positionAssociation.globalPosition.pinPosition.latlon);
                positionAssociation2.pinId = positionAssociation.pinId;
                double num = LatLon.DistanceInMeters(p, positionAssociation.globalPosition.pinPosition.latlon);
                positionAssociation2.qualityMessage = LatLon.PrettyDistance(num);
                list.Add(new QualitySortPair(positionAssociation2, num));
            }

            list.Sort();
            RegistrationDefinition registrationDefinition = new RegistrationDefinition(new DirtyEvent());
            registrationDefinition.warpStyle = registration.warpStyle;
            foreach (QualitySortPair current in list)
            {
                registrationDefinition.AddAssociation(current.assoc);
            }

            registrationDefinition.isLocked = true;
            return registrationDefinition;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            registration.AccumulateRobustHash(hash);
            hash.Accumulate((int)interpolationMode);
        }

        internal abstract void writeToXml(XmlTextWriter writer);
    }
}
