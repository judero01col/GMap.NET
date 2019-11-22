using System.Collections.Generic;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class OpenDocumentSensitivePrioritizer : OpenDocumentStateObserverIfc
    {
        private class ODSPFutureSet : Dictionary<int, OpenDocumentSensitivePrioritizedFuture>
        {
        }

        private class DocToFuturesDict : Dictionary<IFuture, ODSPFutureSet>
        {
        }

        private SizeSensitiveCache openDocumentCache;
        private DocToFuturesDict docToFuturesDict = new DocToFuturesDict();

        public OpenDocumentSensitivePrioritizer(SizeSensitiveCache openDocumentCache)
        {
            this.openDocumentCache = openDocumentCache;
            openDocumentCache.AddCallback(this);
        }

        internal void Realizing(OpenDocumentSensitivePrioritizedFuture openDocumentSensitivePrioritizedFuture)
        {
            Monitor.Enter(this);
            try
            {
                IFuture openDocumentFuture = openDocumentSensitivePrioritizedFuture.GetOpenDocumentFuture();
                if (openDocumentFuture != null)
                {
                    if (!docToFuturesDict.ContainsKey(openDocumentFuture))
                    {
                        docToFuturesDict[openDocumentFuture] = new ODSPFutureSet();
                    }

                    D.Assert(!docToFuturesDict[openDocumentFuture]
                        .ContainsKey(openDocumentSensitivePrioritizedFuture.identity));
                    docToFuturesDict[openDocumentFuture][openDocumentSensitivePrioritizedFuture.identity] =
                        openDocumentSensitivePrioritizedFuture;
                    openDocumentSensitivePrioritizedFuture.DocumentStateChanged(
                        openDocumentCache.Contains(openDocumentFuture));
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal void Complete(OpenDocumentSensitivePrioritizedFuture openDocumentSensitivePrioritizedFuture)
        {
            Monitor.Enter(this);
            try
            {
                IFuture openDocumentFuture = openDocumentSensitivePrioritizedFuture.GetOpenDocumentFuture();
                if (openDocumentFuture != null)
                {
                    bool condition = docToFuturesDict[openDocumentFuture]
                        .Remove(openDocumentSensitivePrioritizedFuture.identity);
                    D.Assert(condition);
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void DocumentStateChanged(IFuture documentFuture, bool isOpen)
        {
            Monitor.Enter(this);
            try
            {
                if (docToFuturesDict.ContainsKey(documentFuture))
                {
                    foreach (OpenDocumentSensitivePrioritizedFuture current in docToFuturesDict[documentFuture]
                        .Values)
                    {
                        current.DocumentStateChanged(isOpen);
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }
    }
}
