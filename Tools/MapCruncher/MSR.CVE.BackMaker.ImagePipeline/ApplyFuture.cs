using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ApplyFuture : FutureBase
    {
        private Verb verb;
        private IFuture[] futureParams;

        public ApplyFuture(Verb verb, params IFuture[] futureParams)
        {
            this.verb = verb;
            this.futureParams = futureParams;
        }

        public override Present Realize(string refCredit)
        {
            Present[] paramList =
                Array.ConvertAll(futureParams, (IFuture f) => f.Realize(refCredit));
            return verb.Evaluate(paramList);
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("Apply(");
            verb.AccumulateRobustHash(hash);
            IFuture[] array = futureParams;
            for (int i = 0; i < array.Length; i++)
            {
                IFuture future = array[i];
                future.AccumulateRobustHash(hash);
                hash.Accumulate(",");
            }

            hash.Accumulate(")");
        }
    }
}
