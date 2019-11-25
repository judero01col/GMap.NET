namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class GeneralDocumentFuture
    {
        public IDocumentFuture documentFuture
        {
            get;
        }

        public void WriteXML(MashupWriteContext context, string pathBase)
        {
            context.writer.WriteStartElement(GetXMLTag());
            documentFuture.WriteXML(context, pathBase);
            context.writer.WriteEndElement();
        }

        public GeneralDocumentFuture(IDocumentFuture documentFuture)
        {
            this.documentFuture = documentFuture;
        }

        public GeneralDocumentFuture(MashupParseContext context, string pathBase)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(FutureDocumentFromFilesystem.GetXMLTag()))
                {
                    if (documentFuture != null)
                    {
                        throw new InvalidMashupFile(context, "Too many specs in " + GetXMLTag());
                    }

                    documentFuture = new FutureDocumentFromFilesystem(context, pathBase);
                }
                else
                {
                    if (xMLTagReader.TagIs(FutureDocumentFromUri.GetXMLTag()))
                    {
                        if (documentFuture != null)
                        {
                            throw new InvalidMashupFile(context, "Too many specs in " + GetXMLTag());
                        }

                        documentFuture = new FutureDocumentFromUri(context);
                    }
                }
            }

            if (documentFuture == null)
            {
                throw new InvalidMashupFile(context, "No spec in " + GetXMLTag());
            }
        }

        internal static string GetXMLTag()
        {
            return "Document";
        }

        public IFuture GetSynchronousFuture(CachePackage cachePackage)
        {
            return new MemCacheFuture(cachePackage.documentFetchCache, documentFuture);
        }

        public IFuture GetAsynchronousFuture(CachePackage cachePackage)
        {
            return new MemCacheFuture(cachePackage.asyncCache,
                Asynchronizer.MakeFuture(cachePackage.computeAsyncScheduler, GetSynchronousFuture(cachePackage)));
        }

        public SourceDocument RealizeSynchronously(CachePackage cachePackage)
        {
            Present present = GetSynchronousFuture(cachePackage).Realize("SourceDocument.RealizeSynchronously");
            if (present is SourceDocument)
            {
                SourceDocument sourceDocument = (SourceDocument)present;
                D.Assert(sourceDocument.localDocument != null, "We waited for document to arrive synchronously.");
                return sourceDocument;
            }

            throw ((PresentFailureCode)present).exception;
        }

        internal void AccumulateRobustHash(IRobustHash hash)
        {
            documentFuture.AccumulateRobustHash(hash);
        }
    }
}
