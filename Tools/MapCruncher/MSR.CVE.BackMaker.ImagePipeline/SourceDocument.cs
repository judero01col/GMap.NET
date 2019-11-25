using System;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class SourceDocument : Present, IDisposable
    {
        public LocalDocumentDescriptor localDocument
        {
            get;
        }

        public SourceDocument(LocalDocumentDescriptor localDocument)
        {
            this.localDocument = localDocument;
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
