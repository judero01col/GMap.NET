using System;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class AsyncRef : Present, IDisposable, QueueRequestIfc, RequestInterestIfc, IEvictable
    {
        public const int INTEREST_LEVEL_MORE_THAN_ANY_TILE = 524288;
        public const int INTEREST_LEVEL_OPEN_DOCUMENT_BONUS = 524291;
        public const int INTEREST_LEVEL_RENDER_ACTIVE_PAINT_EPSILON = 524296;
        public const int INTEREST_LEVEL_BOUNDS = 524290;
        public const int INTEREST_LEVEL_DOCUMENT = 524292;
        private int localInterest;
        private string debugAnnotation;
        private bool debugDisposed;

        private static ResourceCounter asyncRefsHoldingInterestResourceCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRefsHoldingInterest-count", -1);

        public Present present
        {
            get
            {
                return asyncRecord.present;
            }
        }

        public IFuture future
        {
            get
            {
                return asyncRecord.future;
            }
        }

        internal AsyncRecord asyncRecord
        {
            get;
        }

        public AsyncRef(AsyncRecord resource, string debugAnnotation)
        {
            this.asyncRecord = resource;
            this.debugAnnotation = debugAnnotation;
            DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + debugAnnotation, -1).crement(1);
            resource.AddRef();
        }

        public void Dispose()
        {
            D.Assert(!debugDisposed);
            SetInterest(0);
            asyncRecord.DropRef();
            DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + debugAnnotation, -1).crement(-1);
            debugDisposed = true;
        }

        public Present Duplicate(string refCredit)
        {
            return new AsyncRef(asyncRecord, refCredit);
        }

        public void SetInterest(int newInterest)
        {
            int num = localInterest > 524291 ? 1 : 0;
            int num2 = newInterest > 524291 ? 1 : 0;
            asyncRefsHoldingInterestResourceCounter.crement(num2 - num);
            DiagnosticUI.theDiagnostics.fetchResourceCounter("asyncRef-" + debugAnnotation + "-withInterest", -1)
                .crement(num2 - num);
            int crement = newInterest - localInterest;
            asyncRecord.ChangePriority(crement);
            localInterest = newInterest;
        }

        public void AddCallback(AsyncRecord.CompleteCallback callback)
        {
            asyncRecord.AddCallback(callback);
        }

        public override string ToString()
        {
            return asyncRecord.ToString();
        }

        public int GetInterest()
        {
            return asyncRecord.GetPriority();
        }

        public void DoWork()
        {
            asyncRecord.DoWork();
            Dispose();
        }

        public void DeQueued()
        {
            asyncRecord.DeQueued();
            Dispose();
        }

        internal void ProcessSynchronously()
        {
            asyncRecord.ProcessSynchronously();
        }

        public bool EvictMeNow()
        {
            return present is IEvictable && ((IEvictable)present).EvictMeNow();
        }
    }
}
