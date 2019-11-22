using System.Collections.Generic;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class InterestList
    {
        private LinkedList<AsyncRef> list = new LinkedList<AsyncRef>();

        private static ResourceCounter globalInterestCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("interestList-activated-sum", -1);

        public void Add(AsyncRef aref)
        {
            list.AddLast(aref);
        }

        public void Activate()
        {
            Dictionary<AsyncScheduler, LinkedList<AsyncRef>> dictionary =
                new Dictionary<AsyncScheduler, LinkedList<AsyncRef>>();
            foreach (AsyncRef current in list)
            {
                AsyncScheduler scheduler = current.asyncRecord.GetScheduler();
                if (!dictionary.ContainsKey(scheduler))
                {
                    dictionary[scheduler] = new LinkedList<AsyncRef>();
                }

                dictionary[scheduler].AddLast(current);
            }

            foreach (KeyValuePair<AsyncScheduler, LinkedList<AsyncRef>> current2 in dictionary)
            {
                current2.Key.Activate(current2.Value);
            }

            globalInterestCounter.crement(list.Count);
        }

        public void Dispose()
        {
            foreach (AsyncRef current in list)
            {
                current.Dispose();
            }

            globalInterestCounter.crement(-list.Count);
            list.Clear();
        }
    }
}
