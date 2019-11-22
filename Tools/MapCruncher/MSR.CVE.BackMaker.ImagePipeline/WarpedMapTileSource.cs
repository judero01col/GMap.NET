using System;
using System.Drawing;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class WarpedMapTileSource : IRenderableSource, IComparable, IDisplayableSource
    {
        private UnwarpedMapTileSource unwarpedMapTileSource;
        private CachePackage cachePackage;
        private MercatorCoordinateSystem coordinateSystem;
        private IImageTransformer imageTransformer;
        private static Size sourceImageOversampleSize = new Size(512, 512);

        public WarpedMapTileSource(UnwarpedMapTileSource unwarpedTileSource, CachePackage cachePackage,
            SourceMap sourceMap)
        {
            unwarpedMapTileSource = unwarpedTileSource;
            this.cachePackage = cachePackage;
            coordinateSystem = new MercatorCoordinateSystem();
            if (sourceMap.registration.GetAssociationList().Count <
                sourceMap.registration.warpStyle.getCorrespondencesRequired())
            {
                throw new InsufficientCorrespondencesException();
            }

            imageTransformer = sourceMap.registration.warpStyle.getImageTransformer(sourceMap.registration,
                RenderQualityStyle.theStyle.warpInterpolationMode);
        }

        public CoordinateSystemIfc GetDefaultCoordinateSystem()
        {
            return coordinateSystem;
        }

        public string GetRendererCredit()
        {
            return unwarpedMapTileSource.GetRendererCredit();
        }

        public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
        {
            IFuture future = new ApplyFuture(new WarpBoundsVerb(imageTransformer),
                new[] {unwarpedMapTileSource.GetUserBounds(latentRegionHolder, FutureFeatures.Cached)});
            future = new MemCacheFuture(cachePackage.boundsCache, future);
            return unwarpedMapTileSource.AddAsynchrony(future, features);
        }

        public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
        {
            if (parameterType == null)
            {
                parameterType = new ImageParameterFromTileAddress(GetDefaultCoordinateSystem());
            }

            FutureFeatures futureFeatures = FutureFeatures.Raw;
            if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.Transparency))
            {
                futureFeatures |= FutureFeatures.Transparency;
            }

            if (UnwarpedMapTileSource.HasFeature(features, FutureFeatures.MemoryCached))
            {
                futureFeatures |= FutureFeatures.MemoryCached;
            }

            IFuturePrototype futurePrototype = new ApplyPrototype(
                new WarpImageVerb(imageTransformer,
                    GetImageBounds(FutureFeatures.Cached),
                    unwarpedMapTileSource.GetImagePrototype(
                        new ImageParameterFromRawBounds(sourceImageOversampleSize),
                        futureFeatures)),
                new[] {parameterType.GetBoundsParameter(), parameterType.GetSizeParameter()});
            if (parameterType is ImageParameterFromTileAddress)
            {
                futurePrototype =
                    new ApplyPrototype(
                        new FadeVerb(unwarpedMapTileSource.GetTransparencyOptions().GetFadeOptions()),
                        new[] {futurePrototype, new UnevaluatedTerm(TermName.TileAddress)});
            }
            else
            {
                D.Say(2, "Warning: Ignoring fade options because I don't have a tile address.");
            }

            futurePrototype = unwarpedMapTileSource.AddCaching(futurePrototype, features);
            return unwarpedMapTileSource.AddAsynchrony(futurePrototype, features);
        }

        public string GetSourceMapDisplayName()
        {
            return unwarpedMapTileSource.GetSourceMapDisplayName();
        }

        public IFuture GetOpenDocumentFuture(FutureFeatures features)
        {
            return unwarpedMapTileSource.GetOpenDocumentFuture(features);
        }

        public int CompareTo(object obj)
        {
            if (!(obj is WarpedMapTileSource))
            {
                return GetType().FullName.CompareTo(obj.GetType().FullName);
            }

            return GetDocumentFilename().CompareTo(((WarpedMapTileSource)obj).GetDocumentFilename());
        }

        private string GetDocumentFilename()
        {
            return unwarpedMapTileSource.GetDocumentFilename();
        }

        public IFuture GetImageBounds(FutureFeatures features)
        {
            IFuture future = new ApplyFuture(new WarpBoundsVerb(imageTransformer),
                new[] {unwarpedMapTileSource.GetImageBounds(features)});
            future = new MemCacheFuture(cachePackage.boundsCache, future);
            return unwarpedMapTileSource.AddAsynchrony(future, features);
        }

        internal RegistrationDefinition ComputeWarpedRegistration()
        {
            return imageTransformer.getWarpedRegistration();
        }

        internal IPointTransformer GetDestLatLonToSourceTransformer()
        {
            return imageTransformer.getDestLatLonToSourceTransformer();
        }

        internal IPointTransformer GetSourceToDestLatLonTransformer()
        {
            return imageTransformer.getSourceToDestLatLonTransformer();
        }
    }
}
