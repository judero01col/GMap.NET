namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class DisplayableSourceCache : IDisplayableSource
    {
        private IDisplayableSource backingSource;
        private CoordinateSystemIfc cachedCoordSys;
        private string cachedRendererCredit;
        private LatentRegionHolder lastUserBoundsRequest_latentRegionHolder;
        private FutureFeatures lastUserBoundsRequest_features;
        private IFuture cachedUserBounds;
        private ImageParameterTypeIfc lastImageRequest_parameterType;
        private FutureFeatures lastImageRequest_features;
        private IFuturePrototype cachedImageRequest;

        public DisplayableSourceCache(IDisplayableSource backingSource)
        {
            this.backingSource = backingSource;
        }

        internal bool BackingStoreIs(IDisplayableSource backingSource)
        {
            return this.backingSource == backingSource;
        }

        public void Flush()
        {
            cachedCoordSys = null;
            cachedRendererCredit = null;
            cachedUserBounds = null;
            cachedImageRequest = null;
        }

        public CoordinateSystemIfc GetDefaultCoordinateSystem()
        {
            if (cachedCoordSys == null)
            {
                cachedCoordSys = backingSource.GetDefaultCoordinateSystem();
            }

            return cachedCoordSys;
        }

        public string GetRendererCredit()
        {
            if (cachedRendererCredit == null)
            {
                cachedRendererCredit = backingSource.GetRendererCredit();
            }

            return cachedRendererCredit;
        }

        public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
        {
            if (cachedUserBounds == null || lastUserBoundsRequest_latentRegionHolder != latentRegionHolder ||
                lastUserBoundsRequest_features != features)
            {
                lastUserBoundsRequest_latentRegionHolder = latentRegionHolder;
                lastUserBoundsRequest_features = features;
                cachedUserBounds = backingSource.GetUserBounds(latentRegionHolder, features);
            }

            return cachedUserBounds;
        }

        public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
        {
            if (cachedImageRequest == null || lastImageRequest_parameterType != parameterType ||
                lastImageRequest_features != features)
            {
                lastImageRequest_parameterType = parameterType;
                lastImageRequest_features = features;
                cachedImageRequest = backingSource.GetImagePrototype(parameterType, features);
            }

            return cachedImageRequest;
        }
    }
}
