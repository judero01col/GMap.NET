using System.IO;

namespace MSR.CVE.BackMaker
{
    internal class StreamFilter : Stream
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
                return baseStream.CanSeek;
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
                baseStream.Position = value;
            }
        }

        public StreamFilter(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            baseStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            baseStream.Close();
        }
    }
}
