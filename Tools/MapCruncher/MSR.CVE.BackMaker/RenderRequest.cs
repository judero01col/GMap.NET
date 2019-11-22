using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace MSR.CVE.BackMaker
{
    [Serializable]
    public class RenderRequest : ISerializable
    {
        public Point topLeft;
        public Size pageSize;
        public Size outputSize;
        public bool transparentBackground;

        public RenderRequest(Point topLeft, Size pageSize, Size outputSize, bool transparentBackground)
        {
            this.topLeft = topLeft;
            this.pageSize = pageSize;
            this.outputSize = outputSize;
            this.transparentBackground = transparentBackground;
        }

        public RenderRequest(SerializationInfo info, StreamingContext context)
        {
            topLeft = (Point)info.GetValue("TopLeft", typeof(Point));
            pageSize = (Size)info.GetValue("PageSize", typeof(Size));
            outputSize = (Size)info.GetValue("OutputSize", typeof(Size));
            transparentBackground = (bool)info.GetValue("TransparentBackground", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TopLeft", topLeft);
            info.AddValue("PageSize", pageSize);
            info.AddValue("OutputSize", outputSize);
            info.AddValue("TransparentBackground", transparentBackground);
        }
    }
}
