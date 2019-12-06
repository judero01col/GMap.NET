using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms.Markers
{
    [Serializable]
    public class GMarkerCross : GMapMarker, ISerializable
    {
        public static readonly Pen DefaultPen = new Pen(Brushes.Red, 1);

        [NonSerialized] public Pen Pen = DefaultPen;

        public GMarkerCross(PointLatLng p)
            : base(p)
        {
            IsHitTestVisible = false;
        }

        public override void OnRender(Graphics g)
        {
            var p1 = new Point(LocalPosition.X, LocalPosition.Y);
            p1.Offset(0, -10);
            var p2 = new Point(LocalPosition.X, LocalPosition.Y);
            p2.Offset(0, 10);

            var p3 = new Point(LocalPosition.X, LocalPosition.Y);
            p3.Offset(-10, 0);
            var p4 = new Point(LocalPosition.X, LocalPosition.Y);
            p4.Offset(10, 0);

            g.DrawLine(Pen, p1.X, p1.Y, p2.X, p2.Y);
            g.DrawLine(Pen, p3.X, p3.Y, p4.X, p4.Y);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GMarkerCross(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}
