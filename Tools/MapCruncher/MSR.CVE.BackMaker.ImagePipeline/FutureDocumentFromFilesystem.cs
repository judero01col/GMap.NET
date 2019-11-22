using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class FutureDocumentFromFilesystem : FutureBase, IDocumentFuture, IFuture, IRobustlyHashable,
        IFuturePrototype
    {
        private string path;
        private int pageNumber;
        private DateTime lastWriteTime;
        private static string FilenameAttr = "Filename";
        private static string PageNumberAttr = "PageNumber";

        public FutureDocumentFromFilesystem(string path, int pageNumber)
        {
            this.path = path;
            this.pageNumber = pageNumber;
            File.GetLastWriteTime(path).ToUniversalTime();
            ValidateFilename();
        }

        public FutureDocumentFromFilesystem(MashupParseContext context, string pathBase)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            string requiredAttribute = context.GetRequiredAttribute(FilenameAttr);
            path = Path.Combine(pathBase, requiredAttribute);
            pageNumber = context.GetRequiredAttributeInt(PageNumberAttr);
            xMLTagReader.SkipAllSubTags();
            ValidateFilename();
        }

        public void WriteXML(MashupWriteContext context, string pathBase)
        {
            string value = MakeRelativePath(pathBase, path);
            context.writer.WriteStartElement(GetXMLTag());
            context.writer.WriteAttributeString(FilenameAttr, value);
            context.writer.WriteAttributeString(PageNumberAttr, pageNumber.ToString(CultureInfo.InvariantCulture));
            context.writer.WriteEndElement();
        }

        public static string MakeRelativePath(string pathBase, string path)
        {
            D.Assert(Path.IsPathRooted(path));
            string text;
            if (pathBase == null || pathBase == "")
            {
                text = "";
            }
            else
            {
                text = Path.GetFullPath(pathBase);
            }

            string[] array = text.Split(new[] {Path.DirectorySeparatorChar});
            string fullPath = Path.GetFullPath(path);
            string[] array2 = fullPath.Split(new[] {Path.DirectorySeparatorChar});
            if (array[0] != array2[0])
            {
                return fullPath;
            }

            int num = 0;
            while (num < Math.Min(array.Length, text.Length) && !(array[num] != array2[num]))
            {
                num++;
            }

            int num2 = array.Length - num;
            List<string> list = new List<string>();
            for (int i = 0; i < num2; i++)
            {
                list.Add("..");
            }

            for (int j = num; j < array2.Length; j++)
            {
                list.Add(array2[j]);
            }

            return string.Join("" + Path.DirectorySeparatorChar, list.ToArray());
        }

        private void ValidateFilename()
        {
            if (!File.Exists(path))
            {
                throw new InvalidFileContentsException(string.Format("Document reference to {0} invalid", path));
            }
        }

        public override Present Realize(string refCredit)
        {
            return new SourceDocument(new LocalDocumentDescriptor(path, pageNumber));
        }

        public override void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("FutureDocumentFromFilesystem(");
            hash.Accumulate(path);
            hash.Accumulate(pageNumber);
            hash.Accumulate(lastWriteTime.ToBinary());
            hash.Accumulate(")");
        }

        public string GetDefaultDisplayName()
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        internal static string GetXMLTag()
        {
            return "FileDocument";
        }
    }
}
