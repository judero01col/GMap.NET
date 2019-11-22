namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class TransparencyPrototype : IFuturePrototype
    {
        private TransparencyOptions transparencyOptions;
        private IFuturePrototype antialiasedPrototype;
        private IFuturePrototype exactColorPrototype;

        public TransparencyPrototype(TransparencyOptions transparencyOptions, IFuturePrototype antialiasedPrototype,
            IFuturePrototype exactColorPrototype)
        {
            this.transparencyOptions = new TransparencyOptions(transparencyOptions);
            this.antialiasedPrototype = antialiasedPrototype;
            this.exactColorPrototype = exactColorPrototype;
        }

        public IFuture Curry(ParamDict paramDict)
        {
            return new TransparencyFuture(transparencyOptions,
                antialiasedPrototype.Curry(paramDict),
                exactColorPrototype.Curry(paramDict));
        }
    }
}
