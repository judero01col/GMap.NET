using System;
using System.IO;
using System.Net;
using System.Web.Services.Protocols;

namespace MSR.CVE.BackMaker
{
    public class S3OutputMethod : RenderOutputMethod
    {
        private class S3PutClosure : MemoryStream
        {
            private S3OutputMethod s3OutputMethod;
            private string s3key;
            private string contentType;

            public S3PutClosure(S3OutputMethod s3OutputMethod, string s3key, string contentType)
            {
                this.s3OutputMethod = s3OutputMethod;
                this.s3key = s3key;
                this.contentType = contentType;
            }

            public override void Close()
            {
                int num = 3;
                for (int i = 0; i < num; i++)
                {
                    bool flag = i == num - 1;
                    try
                    {
                        HeaderList headerList = new HeaderList();
                        headerList.Add("content-type", contentType);
                        headerList.Add("x-amz-acl", "public-read");
                        s3OutputMethod.s3adaptor.put(s3OutputMethod.bucketName,
                            s3key,
                            new S3Content(ToArray()),
                            headerList);
                        break;
                    }
                    catch (WebException)
                    {
                        if (flag)
                        {
                            throw;
                        }
                    }
                    catch (SoapException)
                    {
                        if (flag)
                        {
                            throw;
                        }
                    }
                }

                base.Close();
            }
        }

        private S3Adaptor s3adaptor;
        private string bucketName;
        private string basePath;
        private HeapBool bucketCreated;

        public S3OutputMethod(S3Adaptor s3adaptor, string bucketName, string basePath)
        {
            this.s3adaptor = s3adaptor;
            this.bucketName = bucketName;
            this.basePath = basePath;
            bucketCreated = new HeapBool(false);
        }

        public S3OutputMethod(S3OutputMethod template)
        {
            s3adaptor = template.s3adaptor;
            bucketName = template.bucketName;
            basePath = template.basePath;
            bucketCreated = template.bucketCreated;
        }

        public Stream CreateFile(string relativePath, string contentType)
        {
            if (!bucketCreated.value)
            {
                s3adaptor.CreateBucket(bucketName);
                bucketCreated.value = true;
            }

            string path = GetPath(relativePath);
            return new S3PutClosure(this, path, contentType);
        }

        public Stream ReadFile(string relativePath)
        {
            string path = GetPath(relativePath);
            HeaderList headers = new HeaderList();
            return s3adaptor.getStream(bucketName, path, headers);
        }

        public Uri GetUri(string relativePath)
        {
            string pathValue = Path.Combine(Path.Combine(bucketName, basePath), relativePath)
                .Replace('\\', '/');
            return new UriBuilder("http", "s3.amazonaws.com", 80, pathValue).Uri;
        }

        private string GetPath(string relativePath)
        {
            string text = Path.Combine(basePath, relativePath);
            return text.Replace("\\", "/");
        }

        public bool KnowFileExists(string relativePath)
        {
            return false;
        }

        public RenderOutputMethod MakeChildMethod(string subdir)
        {
            return new S3OutputMethod(this) {basePath = GetPath(subdir)};
        }

        public FileIdentification GetFileIdentification(string relativePath)
        {
            return new FileIdentification(-1L);
        }

        public void EmptyDirectory()
        {
        }
    }
}
