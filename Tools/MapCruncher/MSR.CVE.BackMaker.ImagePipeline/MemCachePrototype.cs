namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class MemCachePrototype : IFuturePrototype
    {
        private CacheBase cache;
        private IFuturePrototype prototype;

        public MemCachePrototype(CacheBase cache, IFuturePrototype prototype)
        {
            this.cache = cache;
            this.prototype = prototype;
        }

        public IFuture Curry(ParamDict paramDict)
        {
            return new MemCacheFuture(cache, prototype.Curry(paramDict));
        }
    }
}
