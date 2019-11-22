namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class PersistentInterest
    {
        private InterestList interestList;

        public PersistentInterest(AsyncRef asyncRef)
        {
            interestList = new InterestList();
            interestList.Add(asyncRef);
            interestList.Activate();
            asyncRef.AddCallback(AsyncCompleteCallback);
        }

        public void AsyncCompleteCallback(AsyncRef boundsAsyncRef)
        {
            interestList.Dispose();
        }
    }
}
