using System;
using System.IO;

namespace MSR.CVE.BackMaker
{
    public class ManifestOutputMethod : RenderOutputMethod
    {
        private class CreateCompleteClosure : StreamFilter
        {
            private ManifestOutputMethod manifestOutputMethod;
            private string path;
            private bool closed;

            public CreateCompleteClosure(Stream baseStream, ManifestOutputMethod manifestOutputMethod, string path) :
                base(baseStream)
            {
                this.manifestOutputMethod = manifestOutputMethod;
                this.path = path;
            }

            public override void Close()
            {
                if (!closed)
                {
                    long length = base.Length;
                    base.Close();
                    closed = true;
                    manifestOutputMethod.manifest.Add(path, length);
                }
            }
        }

        private string basePath;
        private RenderOutputMethod baseMethod;
        private Manifest manifest;

        public ManifestOutputMethod(RenderOutputMethod baseMethod)
        {
            basePath = "";
            this.baseMethod = baseMethod;
            manifest = new Manifest(baseMethod);
        }

        private ManifestOutputMethod(RenderOutputMethod baseMethod, string basePath, Manifest manifest)
        {
            this.baseMethod = baseMethod;
            this.basePath = basePath;
            this.manifest = manifest;
        }

        public Stream CreateFile(string relativePath, string contentType)
        {
            Stream baseStream = baseMethod.CreateFile(relativePath, contentType);
            return new CreateCompleteClosure(baseStream, this, GetPath(relativePath));
        }

        public Stream ReadFile(string relativePath)
        {
            return baseMethod.ReadFile(relativePath);
        }

        public Uri GetUri(string relativePath)
        {
            return baseMethod.GetUri(relativePath);
        }

        public bool KnowFileExists(string outputFilename)
        {
            string path = GetPath(outputFilename);
            Manifest.ManifestRecord manifestRecord = manifest.FindFirstEqual(path);
            return manifestRecord.fileExists;
        }

        public FileIdentification GetFileIdentification(string relativePath)
        {
            string path = GetPath(relativePath);
            Manifest.ManifestRecord manifestRecord = manifest.FindFirstEqual(path);
            if (!manifestRecord.fileExists)
            {
                return new FileIdentification(-1L);
            }

            return new FileIdentification(manifestRecord.fileLength);
        }

        public RenderOutputMethod MakeChildMethod(string subdir)
        {
            return new ManifestOutputMethod(baseMethod.MakeChildMethod(subdir),
                GetPath(subdir),
                manifest);
        }

        public void EmptyDirectory()
        {
            Manifest.ManifestRecord manifestRecord = manifest.FindFirstGreaterEqual(basePath);
            while (!manifestRecord.IsTailRecord && manifestRecord.path.StartsWith(basePath))
            {
                if (manifestRecord.fileExists)
                {
                    manifest.Remove(manifestRecord.path);
                }

                manifestRecord = manifest.FindFirstGreaterThan(manifestRecord.path);
            }

            CommitChanges();
            baseMethod.EmptyDirectory();
        }

        private string GetPath(string relativePath)
        {
            return basePath + "/" + relativePath;
        }

        public void CommitChanges()
        {
            manifest.CommitChanges();
        }

        public void Test_SetSplitThreshold(int splitThreshold)
        {
            manifest.Test_SetSplitThreshold(splitThreshold);
        }
    }
}
