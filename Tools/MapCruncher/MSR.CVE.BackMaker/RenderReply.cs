using System;
using System.Runtime.Serialization;

namespace MSR.CVE.BackMaker
{
    [Serializable]
    public class RenderReply : ISerializable
    {
        public byte[] data;
        public int stride;

        public RenderReply(byte[] data, int stride)
        {
            this.data = data;
            this.stride = stride;
        }

        public RenderReply(byte[] sourceData, int offset, long length, int stride)
        {
            data = new byte[length];
            Array.Copy(sourceData, offset, data, 0L, length);
            this.stride = stride;
        }

        public RenderReply(SerializationInfo info, StreamingContext context)
        {
            data = (byte[])info.GetValue("Data", typeof(byte[]));
            stride = (int)info.GetValue("Stride", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data", data);
            info.AddValue("Stride", stride);
        }
    }
}
