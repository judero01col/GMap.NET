namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class RenderedTileSource : IDisplayableSource
    {
        private CachePackage cachePackage;
        private RenderedTileNamingScheme namingScheme;
        private CoordinateSystemIfc coordinateSystem;

        public RenderedTileSource(CachePackage cachePackage, RenderedTileNamingScheme namingScheme)
        {
            this.cachePackage = cachePackage;
            this.namingScheme = namingScheme;
            coordinateSystem = new MercatorCoordinateSystem();
        }

        public CoordinateSystemIfc GetDefaultCoordinateSystem()
        {
            return coordinateSystem;
        }

        public string GetRendererCredit()
        {
            return null;
        }

        public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
        {
            D.Assert(UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Async));
            return new MemCacheFuture(cachePackage.asyncCache,
                Asynchronizer.MakeFuture(cachePackage.computeAsyncScheduler,
                    new MemCacheFuture(cachePackage.boundsCache,
                        new ApplyFuture(new ConstantVerb(new BoundsPresent(
                                new RenderRegion(new MapRectangle(-85.0, -5000.0, 85.0, 5000.0), new DirtyEvent()))),
                            new IFuture[0]))));
        }

        public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
        {
            D.Assert(parameterType == null);
            IFuturePrototype prototype = new ApplyPrototype(new RenderedTileFetch(namingScheme),
                new IFuturePrototype[] {new UnevaluatedTerm(TermName.TileAddress)});
            return VETileSource.AddFeatures(prototype, features & (FutureFeatures)(-3), cachePackage);
        }
    }
}
