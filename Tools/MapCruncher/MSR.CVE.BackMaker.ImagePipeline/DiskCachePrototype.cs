namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class DiskCachePrototype : IFuturePrototype
    {
        private DiskCache cache;
        private IFuturePrototype prototype;

        public DiskCachePrototype(DiskCache cache, IFuturePrototype prototype)
        {
            this.cache = cache;
            this.prototype = prototype;
        }

        public IFuture Curry(ParamDict paramDict)
        {
            return new DiskCacheFuture(cache, prototype.Curry(paramDict));
        }
    }
}
