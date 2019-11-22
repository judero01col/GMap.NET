using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class SourceDocument : Present, IDisposable
    {
        private LocalDocumentDescriptor _localDocument;

        public LocalDocumentDescriptor localDocument
        {
            get
            {
                return _localDocument;
            }
        }

        public SourceDocument(LocalDocumentDescriptor localDocument)
        {
            _localDocument = localDocument;
        }

        public Present Duplicate(string refCredit)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
