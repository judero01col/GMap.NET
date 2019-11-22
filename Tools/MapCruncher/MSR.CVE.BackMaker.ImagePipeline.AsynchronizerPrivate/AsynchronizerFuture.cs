namespace MSR.CVE.BackMaker.ImagePipeline.AsynchronizerPrivate
{
    internal class AsynchronizerFuture : FutureBase
    {
        private AsyncScheduler scheduler;
        private IFuture innerFuture;

        public AsynchronizerFuture(AsyncScheduler scheduler, IFuture innerFuture)
        {
            this.scheduler = scheduler;
            this.innerFuture = innerFuture;
        }

        public override Present Realize(string refCredit)
        {
            return new AsyncRef(new AsyncRecord(scheduler, this, innerFuture), refCredit);
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("AsynchronizerFuture(");
            innerFuture.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }
    }
}
