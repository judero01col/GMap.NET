using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class SynchronizingFuture : FutureBase
    {
        private IFuture asyncFuture;
        private int interestValue;

        private EventWaitHandle asyncReadyEvent =
            new CountedEventWaitHandle(false, EventResetMode.ManualReset, "SynchronizingFuture.ReadyEvent");

        public SynchronizingFuture(IFuture asyncFuture, int interestValue)
        {
            this.asyncFuture = asyncFuture;
            this.interestValue = interestValue;
        }

        public void Dispose()
        {
            if (asyncReadyEvent != null)
            {
                asyncReadyEvent.Close();
                asyncReadyEvent = null;
            }
        }

        public override Present Realize(string refCredit)
        {
            Present present = asyncFuture.Realize(refCredit);
            if (present is AsyncRef)
            {
                AsyncRef asyncRef = (AsyncRef)present;
                asyncRef.AddCallback(PresentReadyCallback);
                asyncRef.SetInterest(interestValue);
                AsyncRef asyncRef2 = (AsyncRef)asyncRef.Duplicate(refCredit + "2");
                new PersistentInterest(asyncRef);
                asyncReadyEvent.WaitOne();
                return asyncRef2.present;
            }

            return present;
        }

        private void PresentReadyCallback(AsyncRef asyncRef)
        {
            asyncReadyEvent.Set();
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("SynchronizingFuture(");
            asyncFuture.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }
    }
}
