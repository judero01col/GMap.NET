using System.Collections.Generic;
using System.IO;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class FetchDocumentFuture : FutureBase, IFuturePrototype
    {
        private IFuture documentFuture;
        private static Dictionary<string, string> _knownExtensions;

        private static Dictionary<string, string> knownExtensions
        {
            get
            {
                if (_knownExtensions == null)
                {
                    _knownExtensions = new Dictionary<string, string>();
                    _knownExtensions.Add("pdf", "FoxIt");
                    _knownExtensions.Add("jpg", "WPF");
                    _knownExtensions.Add("gif", "WPF");
                    _knownExtensions.Add("png", "WPF");
                    _knownExtensions.Add("wmf", "GDI");
                    _knownExtensions.Add("emf", "GDI");
                    _knownExtensions.Add("tif", "WPF");
                    _knownExtensions.Add("tiff", "WPF");
                    _knownExtensions.Add("bmp", "WPF");
                }

                return _knownExtensions;
            }
        }

        public FetchDocumentFuture(IFuture documentFuture)
        {
            this.documentFuture = documentFuture;
        }

        public override Present Realize(string refCredit)
        {
            Present present = documentFuture.Realize(refCredit);
            if (!(present is SourceDocument))
            {
                return PresentFailureCode.FailedCast(present, "FetchDocumentFuture");
            }

            SourceDocument sourceDocument = (SourceDocument)present;
            string filesystemAbsolutePath = sourceDocument.localDocument.GetFilesystemAbsolutePath();
            Present[] paramList = new Present[]
            {
                new StringParameter(filesystemAbsolutePath),
                new IntParameter(sourceDocument.localDocument.GetPageNumber())
            };
            string text = Path.GetExtension(filesystemAbsolutePath).ToLower();
            if (text[0] == '.')
            {
                text = text.Substring(1);
            }

            Verb verb = null;
            string a = null;
            if (knownExtensions.ContainsKey(text))
            {
                a = knownExtensions[text];
            }

            if (a == "FoxIt")
            {
                verb = new FoxitOpenVerb();
            }
            else
            {
                if (a == "WPF")
                {
                    verb = new WPFOpenVerb();
                }
                else
                {
                    if (a == "GDI")
                    {
                        verb = new GDIOpenVerb();
                    }
                }
            }

            if (verb == null)
            {
                return new PresentFailureCode(new UnknownImageTypeException("Unknown file type " + text));
            }

            return verb.Evaluate(paramList);
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("FetchDocumentFuture(");
            documentFuture.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }

        public static string[] GetKnownFileTypes()
        {
            return new List<string>(knownExtensions.Keys).ToArray();
        }
    }
}
