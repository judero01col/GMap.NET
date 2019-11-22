using Jama;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class FastPoly2PointTransformer : IPolyPointTransformer
    {
        private double[] c = new double[12];

        public FastPoly2PointTransformer(JamaMatrix matrix) : base(matrix)
        {
            polynomialDegree = 2;
            for (int i = 0; i < 12; i++)
            {
                c[i] = matrix.GetElement(i, 0);
            }
        }

        public override void doTransform(PointD p0, PointD p1)
        {
            double num = p0.x * p0.x;
            double num2 = p0.x * p0.y;
            double num3 = p0.y * p0.y;
            p1.x = c[0] + c[1] * p0.y + c[2] * num3 + c[3] * p0.x + c[4] * num2 +
                   c[5] * num;
            p1.y = c[6] + c[7] * p0.y + c[8] * num3 + c[9] * p0.x + c[10] * num2 +
                   c[11] * num;
        }
    }
}
