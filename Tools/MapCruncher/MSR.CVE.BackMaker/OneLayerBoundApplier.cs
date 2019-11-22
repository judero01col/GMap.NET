using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class OneLayerBoundApplier
    {
        internal IRenderableSource source;
        internal IFuturePrototype clippedImageFuture;
        internal Layer layer;

        internal OneLayerBoundApplier(IRenderableSource source, Layer layer, CachePackage cachePackage)
        {
            this.source = source;
            this.layer = layer;
            clippedImageFuture = new MemCachePrototype(cachePackage.computeCache,
                new ApplyPrototype(new UserClipperVerb(),
                    new[]
                    {
                        source.GetImagePrototype(null, (FutureFeatures)11),
                        new UnevaluatedTerm(TermName.TileAddress), source.GetUserBounds(null, FutureFeatures.Cached)
                    }));
        }

        internal string DescribeSourceForComplaint()
        {
            return string.Format("Layer {0} Source {1}",
                layer.GetDisplayName(),
                source.GetSourceMapDisplayName());
        }
    }
}
