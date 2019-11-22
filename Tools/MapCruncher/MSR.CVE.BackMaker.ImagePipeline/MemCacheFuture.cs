namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class MemCacheFuture : FutureBase
    {
        private CacheBase cache;
        private IFuture future;

        public MemCacheFuture(CacheBase cache, IFuture future)
        {
            this.cache = cache;
            this.future = future;
        }

        public override Present Realize(string refCredit)
        {
            return cache.Get(future, refCredit);
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("Cache(");
            cache.AccumulateRobustHash(hash);
            hash.Accumulate(",");
            future.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }

        internal IFuture GetOpenDocumentFuture()
        {
            if (cache is SizeSensitiveCache)
            {
                return future;
            }

            return null;
        }
    }
}
