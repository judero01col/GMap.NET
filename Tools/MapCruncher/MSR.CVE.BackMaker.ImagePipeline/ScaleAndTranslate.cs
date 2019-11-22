using System.Windows.Media;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class ScaleAndTranslate
    {
        private double scx;
        private double scy;
        private double tx;
        private double ty;

        public ScaleAndTranslate(RectangleD source, RectangleD dest)
        {
            scx = (dest.Right - dest.Left) / (source.Right - source.Left);
            tx = dest.Left - scx * source.Left;
            scy = (dest.Bottom - dest.Top) / (source.Bottom - source.Top);
            ty = dest.Top - scy * source.Top;
        }

        public ScaleAndTranslate(double tx, double ty)
        {
            scx = 1.0;
            scy = 1.0;
            this.tx = tx;
            this.ty = ty;
        }

        public RectangleD Apply(RectangleD a)
        {
            double num = scx * a.Left + tx;
            double num2 = scy * a.Top + ty;
            double num3 = scx * a.Right + tx;
            double num4 = scy * a.Bottom + ty;
            return new RectangleD(num, num2, num3 - num, num4 - num2);
        }

        public ScaleTransform ToScaleTransform()
        {
            return new ScaleTransform(scx, scy);
        }

        public override string ToString()
        {
            return string.Format("Scale({0},{1})Transform({2},{3})",
                new object[] {scx, scy, tx, ty});
        }
    }
}
