using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class MapTileSourceFactory
    {
        private CachePackage cachePackage;

        public MapTileSourceFactory(CachePackage cachePackage)
        {
            this.cachePackage = cachePackage;
        }

        public IDisplayableSource CreateDisplayableUnwarpedSource(SourceMap sourceMap)
        {
            return CreateUnwarpedSource(sourceMap);
        }

        public UnwarpedMapTileSource CreateUnwarpedSource(SourceMap sourceMap)
        {
            return new UnwarpedMapTileSource(cachePackage,
                sourceMap.documentFuture.GetSynchronousFuture(cachePackage),
                sourceMap);
        }

        public IDisplayableSource CreateDisplayableWarpedSource(SourceMap sourceMap)
        {
            if (!sourceMap.ReadyToLock())
            {
                return null;
            }

            return CreateWarpedSource(sourceMap);
        }

        public IRenderableSource CreateRenderableWarpedSource(SourceMap sourceMap)
        {
            D.Assert(sourceMap.ReadyToLock());
            return CreateWarpedSource(sourceMap);
        }

        public WarpedMapTileSource CreateWarpedSource(SourceMap sourceMap)
        {
            return new WarpedMapTileSource(CreateUnwarpedSource(sourceMap), cachePackage, sourceMap);
        }

        public int GetOpenSourceDocumentCacheSpillCount()
        {
            return cachePackage.openSourceDocumentCache.GetSpillCount();
        }

        internal void PurgeOpenSourceDocumentCache()
        {
            cachePackage.openSourceDocumentCache.Purge();
        }

        internal CachePackage GetCachePackage()
        {
            return cachePackage;
        }

        public string[] GetKnownFileTypes()
        {
            return FetchDocumentFuture.GetKnownFileTypes();
        }
    }
}
