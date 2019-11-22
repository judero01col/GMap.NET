using System.Drawing;

namespace MSR.CVE.BackMaker
{
    public class Point64
    {
        private long _x;
        private long _y;

        public long X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public long Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public Point64(long x, long y)
        {
            _x = x;
            _y = y;
        }

        public Point ToPoint()
        {
            D.Assert(_x <= 2147483647L && _x >= -2147483648L && _y <= 2147483647L &&
                     _y >= -2147483648L);
            return new Point((int)_x, (int)_y);
        }
    }
}
