using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public abstract class ImmutableParameter<T> : Parameter, IRobustlyHashable, Present, IDisposable
    {
        public T value
        {
            get;
        }

        public ImmutableParameter(T value)
        {
            this.value = value;
        }

        public Present Duplicate(string refCredit)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public abstract void AccumulateRobustHash(IRobustHash hash);
    }
}
