using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ConstantVerb : Verb
    {
        private Present constantPresent;

        public ConstantVerb()
        {
            constantPresent = new PresentFailureCode(new Exception("Null ConstantVerb"));
        }

        public ConstantVerb(Present constantPresent)
        {
            this.constantPresent = constantPresent;
        }

        public Present Evaluate(Present[] paramList)
        {
            return constantPresent;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("Constant");
        }
    }
}
