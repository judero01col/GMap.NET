using Jama;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class FastPoly1PointTransformer : IPolyPointTransformer
    {
        private double[] c = new double[6];

        public FastPoly1PointTransformer(JamaMatrix matrix) : base(matrix)
        {
            polynomialDegree = 1;
            for (int i = 0; i < 6; i++)
            {
                c[i] = matrix.GetElement(i, 0);
            }
        }

        public override void doTransform(PointD p0, PointD p1)
        {
            double arg_06_0 = p0.x;
            double arg_0D_0 = p0.x;
            double arg_14_0 = p0.x;
            double arg_1B_0 = p0.y;
            double arg_22_0 = p0.y;
            double arg_29_0 = p0.y;
            p1.x = c[0] + c[1] * p0.y + c[2] * p0.x;
            p1.y = c[3] + c[4] * p0.y + c[5] * p0.x;
        }
    }
}
