using System;
using System.Drawing;
using System.Windows;

namespace MSR.CVE.BackMaker
{
    public class RectangleD
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;

        public double X
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

        public double Y
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

        public double Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }

        public double Left
        {
            get
            {
                return _x;
            }
        }

        public double Right
        {
            get
            {
                return _x + _width;
            }
        }

        public double Top
        {
            get
            {
                return _y;
            }
        }

        public double Bottom
        {
            get
            {
                return _y + _height;
            }
        }

        public RectangleD(double x, double y, double width, double height)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public RectangleF ToRectangleF()
        {
            return new RectangleF((float)X, (float)Y, (float)Width, (float)Height);
        }

        public RectangleD Round()
        {
            int num = (int)Math.Round(Left);
            int num2 = (int)Math.Round(Top);
            int num3 = (int)Math.Round(Right);
            int num4 = (int)Math.Round(Bottom);
            return new RectangleD((double)num, (double)num2, (double)(num3 - num), (double)(num4 - num2));
        }

        public Int32Rect ToInt32Rect()
        {
            int num = (int)Math.Round(Left);
            int num2 = (int)Math.Round(Top);
            int num3 = (int)Math.Round(Right);
            int num4 = (int)Math.Round(Bottom);
            return new Int32Rect(num, num2, num3 - num, num4 - num2);
        }

        public override string ToString()
        {
            return string.Format("RectangleD(x{0}, y{1}, w{2}, h{3})",
                new object[] {_x, _y, _width, _height});
        }

        public RectangleD Intersect(RectangleD r1)
        {
            double num = Math.Max(Left, r1.Left);
            double num2 = Math.Min(Right, r1.Right);
            double num3 = Math.Max(Top, r1.Top);
            double num4 = Math.Min(Bottom, r1.Bottom);
            return new RectangleD(num, num3, num2 - num, num4 - num3);
        }

        public RectangleD Grow(double margin)
        {
            return new RectangleD(X - margin,
                Y - margin,
                Width + 2.0 * margin,
                Height + 2.0 * margin);
        }

        internal bool IntIsEmpty()
        {
            Int32Rect int32Rect = ToInt32Rect();
            return int32Rect.Width <= 0 || int32Rect.Height <= 0;
        }
    }
}
