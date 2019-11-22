using System;
using System.IO;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class UnseekableStream : Stream
    {
        private Stream baseStream;

        public override bool CanRead
        {
            get
            {
                return baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return baseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return baseStream.Position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public UnseekableStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return baseStream.Read(buffer, offset, count);
        }

        public override void Close()
        {
            baseStream.Close();
        }
    }
}
