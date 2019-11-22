using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class LegendDisplayableSourceWrapper : IDisplayableSource
    {
        private IDisplayableSource underSource;
        private LatentRegionHolder replacementLatentRegionHolder;

        public LegendDisplayableSourceWrapper(IDisplayableSource underSource,
            LatentRegionHolder replacementLatentRegionHolder)
        {
            this.underSource = underSource;
            this.replacementLatentRegionHolder = replacementLatentRegionHolder;
        }

        public CoordinateSystemIfc GetDefaultCoordinateSystem()
        {
            return underSource.GetDefaultCoordinateSystem();
        }

        public string GetRendererCredit()
        {
            return underSource.GetRendererCredit();
        }

        public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
        {
            if (latentRegionHolder == null)
            {
                latentRegionHolder = replacementLatentRegionHolder;
            }

            return underSource.GetUserBounds(latentRegionHolder, features);
        }

        public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
        {
            FutureFeatures features2 = features & (FutureFeatures)(-9);
            return underSource.GetImagePrototype(parameterType, features2);
        }
    }
}
