using System.Collections.Generic;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ImageRefCounted
    {
        internal int refCreditCounter;

        private static ResourceCounter imageResourceCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("Image", 10);

        private int refs;
        private bool disposed;
        private List<string> _referenceHistory;

        public GDIBigLockedImage image
        {
            get;
            private set;
        }

        private List<string> referenceHistory
        {
            get
            {
                if (_referenceHistory == null)
                {
                    _referenceHistory = new List<string>();
                }

                return _referenceHistory;
            }
        }

        public ImageRefCounted(GDIBigLockedImage image)
        {
            this.image = image;
            imageResourceCounter.crement(1);
        }

        private void Dispose()
        {
            image.Dispose();
            image = null;
            imageResourceCounter.crement(-1);
        }

        public void AddRef(string refCredit)
        {
            Monitor.Enter(this);
            try
            {
                D.Assert(!disposed);
                refs++;
                if (BuildConfig.theConfig.debugRefs)
                {
                    referenceHistory.Add(string.Format("{0} Add  {1} refs={2}",
                        MakeObjectID.Maker.make(this),
                        refCredit,
                        refs));
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void DropRef(string refCredit)
        {
            Monitor.Enter(this);
            try
            {
                if (disposed)
                {
                    if (BuildConfig.theConfig.debugRefs)
                    {
                        PrintReferenceHistory();
                    }

                    D.Assert(!disposed);
                }

                refs--;
                if (BuildConfig.theConfig.debugRefs)
                {
                    referenceHistory.Add(string.Format("{0} Drop {1} refs={2}",
                        MakeObjectID.Maker.make(this),
                        refCredit,
                        refs));
                }

                if (refs == 0)
                {
                    Dispose();
                    disposed = true;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void PrintReferenceHistory()
        {
            foreach (string current in referenceHistory)
            {
                D.Sayf(0, "History: {0}", new object[] {current});
            }
        }
    }
}
