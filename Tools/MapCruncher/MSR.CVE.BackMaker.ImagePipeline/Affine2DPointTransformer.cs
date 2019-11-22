using System;
using Jama;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class Affine2DPointTransformer : IPointTransformer
    {
        private double c0;
        private double c1;
        private double c2;
        private double c3;
        private double c4;
        private double c5;

        public Affine2DPointTransformer(JamaMatrix matrix)
        {
            c0 = matrix.GetElement(0, 0);
            c1 = matrix.GetElement(0, 1);
            c2 = matrix.GetElement(0, 2);
            c3 = matrix.GetElement(1, 0);
            c4 = matrix.GetElement(1, 1);
            c5 = matrix.GetElement(1, 2);
        }

        public override void doTransform(PointD p0, PointD p1)
        {
            p1.x = c0 * p0.x + c1 * p0.y + c2;
            p1.y = c3 * p0.x + c4 * p0.y + c5;
        }

        public override IPointTransformer getInversePointTransfomer()
        {
            throw new NotImplementedException();
        }
    }
}
