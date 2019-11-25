using System;
using System.Drawing;
using System.Windows;

namespace MSR.CVE.BackMaker
{
    public class RectangleD
    {
        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public double Width
        {
            get;
            set;
        }

        public double Height
        {
            get;
            set;
        }

        public double Left
        {
            get
            {
                return X;
            }
        }

        public double Right
        {
            get
            {
                return X + Width;
            }
        }

        public double Top
        {
            get
            {
                return Y;
            }
        }

        public double Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        public RectangleD(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
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
            return new RectangleD(num, num2, num3 - num, num4 - num2);
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
                new object[] {X, Y, Width, Height});
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
