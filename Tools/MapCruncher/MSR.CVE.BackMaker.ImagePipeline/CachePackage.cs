using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class CachePackage : IDisposable
    {
        private const string RootIdentifier = "root";
        public string identifier;
        public AsyncScheduler computeAsyncScheduler;
        public AsyncScheduler networkAsyncScheduler;
        public SizeSensitiveCache openSourceDocumentCache;
        public OpenDocumentSensitivePrioritizer openDocumentPrioritizer;
        public MemoryCache computeCache;
        public MemoryCache networkCache;
        public MemoryCache boundsCache;
        public MemoryCache asyncCache;
        public MemoryCache documentFetchCache;
        public DiskCache diskCache;

        private string suffix
        {
            get
            {
                return "-" + identifier;
            }
        }

        public CachePackage()
        {
            identifier = "root";
            Flush();
            openSourceDocumentCache = new SizeSensitiveCache("openDocumentCache" + suffix);
            openDocumentPrioritizer = new OpenDocumentSensitivePrioritizer(openSourceDocumentCache);
            diskCache = new DiskCache();
        }

        private CachePackage(string identifier)
        {
            this.identifier = identifier;
        }

        public CachePackage DeriveCache(string identifier)
        {
            CachePackage cachePackage = new CachePackage(identifier);
            cachePackage.openSourceDocumentCache = openSourceDocumentCache;
            cachePackage.openDocumentPrioritizer = openDocumentPrioritizer;
            cachePackage.diskCache = diskCache;
            cachePackage.Flush();
            return cachePackage;
        }

        public void Flush()
        {
            PreflushDispose();
            computeAsyncScheduler = new AsyncScheduler(1, "computeScheduler" + suffix);
            networkAsyncScheduler = new AsyncScheduler(8, "networkScheduler" + suffix);
            computeCache = new MemoryCache("computeCache" + suffix, 100);
            networkCache = new MemoryCache("networkCache" + suffix);
            boundsCache = new MemoryCache("boundsCache" + suffix);
            asyncCache = new AsyncRecordCache("asyncCache" + suffix, true);
            documentFetchCache = new MemoryCache("documentCache" + suffix, 10000);
        }

        private void PreflushDispose()
        {
            if (computeAsyncScheduler != null)
            {
                computeAsyncScheduler.Dispose();
                networkAsyncScheduler.Dispose();
                computeCache.Dispose();
                networkCache.Dispose();
                boundsCache.Dispose();
                asyncCache.Dispose();
                documentFetchCache.Dispose();
            }
        }

        public void Dispose()
        {
            PreflushDispose();
            if (identifier == "root")
            {
                openSourceDocumentCache.Dispose();
                diskCache.Dispose();
            }
        }

        public void ClearSchedulers()
        {
            if (computeAsyncScheduler != null)
            {
                computeAsyncScheduler.Clear();
            }

            if (networkAsyncScheduler != null)
            {
                networkAsyncScheduler.Clear();
            }
        }
    }
}
