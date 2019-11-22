using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class UnwarpedMapTileSource : IDisplayableSource, IDocumentSource
    {
        private CachePackage cachePackage;
        private IFuture localDocumentFuture;
        private SourceMap sourceMap;

        public UnwarpedMapTileSource(CachePackage cachePackage, IFuture localDocumentFuture, SourceMap sourceMap)
        {
            this.cachePackage = cachePackage;
            this.localDocumentFuture = localDocumentFuture;
            this.sourceMap = sourceMap;
        }

        public CoordinateSystemIfc GetDefaultCoordinateSystem()
        {
            return new ContinuousCoordinateSystem();
        }

        public string GetRendererCredit()
        {
            IFuture accessFuture =
                GetAccessFuture(AccessMethod.RendererCredit, FutureFeatures.Cached, new IFuture[0]);
            Present present = accessFuture.Realize("UnwarpedMapTileSource.GetRendererCredit");
            if (present is StringParameter)
            {
                return ((StringParameter)present).value;
            }

            return null;
        }

        private IFuture GetAccessFuture(AccessMethod accessMethod, FutureFeatures openDocFeatures,
            params IFuture[] methodParams)
        {
            IFuture[] array = new IFuture[2 + methodParams.Length];
            array[0] = GetOpenDocumentFuture(openDocFeatures);
            array[1] = new ConstantFuture(new IntParameter((int)accessMethod));
            Array.Copy(methodParams, 0, array, 2, methodParams.Length);
            return new ApplyFuture(new ApplyVerbPresent(), array);
        }

        private IFuturePrototype GetAccessPrototype(AccessMethod accessMethod, FutureFeatures openDocFeatures,
            params IFuturePrototype[] methodParams)
        {
            IFuturePrototype[] array = new IFuturePrototype[2 + methodParams.Length];
            array[0] = GetOpenDocumentFuture(openDocFeatures);
            array[1] = new ConstantFuture(new IntParameter((int)accessMethod));
            Array.Copy(methodParams, 0, array, 2, methodParams.Length);
            return new ApplyPrototype(new ApplyVerbPresent(), array);
        }

        public string GetSourceMapDisplayName()
        {
            return sourceMap.displayName;
        }

        public IFuture GetOpenDocumentFuture(FutureFeatures features)
        {
            IFuture future = new FetchDocumentFuture(localDocumentFuture);
            D.Assert(HasFeature(features, FutureFeatures.MemoryCached));
            if (HasFeature(features, FutureFeatures.MemoryCached))
            {
                future = new MemCacheFuture(cachePackage.openSourceDocumentCache, future);
            }

            D.Assert(!HasFeature(features, FutureFeatures.Async));
            return future;
        }

        public IFuturePrototype GetImageDetailPrototype(FutureFeatures features)
        {
            IFuturePrototype prototype = GetAccessPrototype(AccessMethod.ImageDetail,
                FutureFeatures.Cached,
                new IFuturePrototype[] {new UnevaluatedTerm(TermName.ImageDetail)});
            if (HasFeature(features, FutureFeatures.MemoryCached))
            {
                prototype = new MemCachePrototype(cachePackage.computeCache, prototype);
            }

            return AddAsynchrony(prototype, features);
        }

        public IFuture GetImageBounds(FutureFeatures features)
        {
            IFuture future = GetAccessFuture(AccessMethod.FetchBounds, FutureFeatures.Cached, new IFuture[0]);
            future = new MemCacheFuture(cachePackage.boundsCache, future);
            return AddAsynchrony(future, features);
        }

        public IFuture GetUserBounds(LatentRegionHolder latentRegionHolder, FutureFeatures features)
        {
            D.Assert(HasFeature(features, FutureFeatures.MemoryCached));
            D.Assert(!HasFeature(features, FutureFeatures.Transparency));
            if (latentRegionHolder == null)
            {
                latentRegionHolder = sourceMap.latentRegionHolder;
            }

            latentRegionHolder.RequestRenderRegion(GetImageBounds((FutureFeatures)7));
            IFuture future = new MemCacheFuture(cachePackage.boundsCache,
                new ApplyFuture(new UserBoundsRefVerb(latentRegionHolder, GetImageBounds(FutureFeatures.Cached)),
                    new IFuture[0]));
            return AddAsynchrony(future, features);
        }

        public IFuturePrototype GetImagePrototype(ImageParameterTypeIfc parameterType, FutureFeatures features)
        {
            if (parameterType == null)
            {
                parameterType = new ImageParameterFromTileAddress(GetDefaultCoordinateSystem());
            }

            FutureFeatures openDocFeatures = FutureFeatures.Cached;
            IFuturePrototype accessPrototype = GetAccessPrototype(AccessMethod.Render,
                openDocFeatures,
                new IFuturePrototype[]
                {
                    new UnevaluatedTerm(TermName.ImageBounds), new UnevaluatedTerm(TermName.OutputSize),
                    new UnevaluatedTerm(TermName.UseDocumentTransparency), new UnevaluatedTerm(TermName.ExactColors)
                });
            Verb verb = new SourceImageDownsamplerVerb(AddCaching(accessPrototype, FutureFeatures.Cached));
            IFuturePrototype futurePrototype = new ApplyPrototype(verb,
                new[]
                {
                    parameterType.GetBoundsParameter(), parameterType.GetSizeParameter(),
                    new ConstantFuture(
                        new BoolParameter(sourceMap.transparencyOptions.useDocumentTransparency)),
                    new ConstantFuture(new BoolParameter(HasFeature(features, FutureFeatures.ExactColors)))
                });
            if (HasFeature(features, FutureFeatures.Transparency))
            {
                IFuturePrototype futurePrototype2 = new ApplyPrototype(verb,
                    new[]
                    {
                        parameterType.GetBoundsParameter(), parameterType.GetSizeParameter(),
                        new ConstantFuture(
                            new BoolParameter(sourceMap.transparencyOptions.useDocumentTransparency)),
                        new ConstantFuture(new BoolParameter(true))
                    });
                if (HasFeature(features, FutureFeatures.MemoryCached))
                {
                    futurePrototype = AddCaching(futurePrototype, FutureFeatures.MemoryCached);
                    futurePrototype2 = AddCaching(futurePrototype2, FutureFeatures.MemoryCached);
                }

                futurePrototype = new TransparencyPrototype(sourceMap.transparencyOptions,
                    futurePrototype,
                    futurePrototype2);
            }

            futurePrototype = AddCaching(futurePrototype, features);
            return AddAsynchrony(futurePrototype, features);
        }

        public static bool HasFeature(FutureFeatures features, FutureFeatures query)
        {
            return (features & query) > FutureFeatures.Raw;
        }

        internal IFuture AddAsynchrony(IFuture future, FutureFeatures features)
        {
            if (HasFeature(features, FutureFeatures.Async))
            {
                future = new MemCacheFuture(cachePackage.asyncCache,
                    new OpenDocumentSensitivePrioritizedFuture(cachePackage.openDocumentPrioritizer,
                        Asynchronizer.MakeFuture(cachePackage.computeAsyncScheduler, future),
                        GetOpenDocumentFuture(FutureFeatures.MemoryCached)));
            }

            return future;
        }

        internal IFuturePrototype AddCaching(IFuturePrototype prototype, FutureFeatures features)
        {
            if (HasFeature(features, FutureFeatures.DiskCached))
            {
                prototype = new DiskCachePrototype(cachePackage.diskCache, prototype);
            }

            if (HasFeature(features, FutureFeatures.MemoryCached))
            {
                prototype = new MemCachePrototype(cachePackage.computeCache, prototype);
            }

            return prototype;
        }

        internal IFuturePrototype AddAsynchrony(IFuturePrototype prototype, FutureFeatures features)
        {
            if (HasFeature(features, FutureFeatures.Async))
            {
                D.Assert(HasFeature(features, FutureFeatures.MemoryCached),
                    "should always cache async stuff, I think.");
                prototype = new MemCachePrototype(cachePackage.asyncCache,
                    new OpenDocumentSensitivePrioritizedPrototype(cachePackage.openDocumentPrioritizer,
                        new Asynchronizer(cachePackage.computeAsyncScheduler, prototype),
                        GetOpenDocumentFuture(FutureFeatures.MemoryCached)));
            }

            return prototype;
        }

        internal string GetDocumentFilename()
        {
            Present present = localDocumentFuture.Realize("UnwarpedMapTileSource.GetDocumentFilename");
            if (present is SourceDocument)
            {
                return ((SourceDocument)present).localDocument.GetFilesystemAbsolutePath();
            }

            throw new Exception("Unable to fetch document");
        }

        internal TransparencyOptions GetTransparencyOptions()
        {
            return sourceMap.transparencyOptions;
        }
    }
}
