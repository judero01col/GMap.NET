using System.Drawing;

namespace MSR.CVE.BackMaker
{
    public class Point64
    {
        public long X
        {
            get;
            set;
        }

        public long Y
        {
            get;
            set;
        }

        public Point64(long x, long y)
        {
            X = x;
            Y = y;
        }

        public Point ToPoint()
        {
            D.Assert(X <= 2147483647L && X >= -2147483648L && Y <= 2147483647L &&
                     Y >= -2147483648L);
            return new Point((int)X, (int)Y);
        }
    }
}
