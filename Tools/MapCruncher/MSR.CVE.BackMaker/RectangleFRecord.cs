using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace MSR.CVE.BackMaker
{
    [Serializable]
    public class RectangleFRecord : ISerializable
    {
        public RectangleF rect;

        public RectangleFRecord(RectangleF rect)
        {
            this.rect = rect;
        }

        public RectangleFRecord(SerializationInfo info, StreamingContext context)
        {
            rect.X = (float)info.GetValue("X", typeof(float));
            rect.Y = (float)info.GetValue("Y", typeof(float));
            rect.Width = (float)info.GetValue("Width", typeof(float));
            rect.Height = (float)info.GetValue("Height", typeof(float));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", rect.X);
            info.AddValue("Y", rect.Y);
            info.AddValue("Width", rect.Width);
            info.AddValue("Height", rect.Height);
        }
    }
}
