using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;

namespace GMap.NET.Avalonia
{
    public static class TransformHelper
    {
        public static Point Transform(this ScaleTransform transform, Point point)
        {
            var matrix = transform.Value;
            return new Point(matrix.M11 * point.X, matrix.M22 * point.Y);
        }
        public static Point Transform(this RotateTransform transform, Point point)
        {
            var matrix = transform.Value;
            double xadd = point.Y * matrix.M21 + matrix.M31;
            double yadd = point.X * matrix.M12 + matrix.M32;
            double x = point.X * matrix.M11;
            x += xadd;
            double y = point.Y * matrix.M22;
            y += yadd;
            return new Point(matrix.M11 * x, matrix.M22 * y);
        }
        public static Point Transform(this MatrixTransform transform, Point point)
        {
            var matrix = transform.Value;
            double xadd = point.Y * matrix.M21 + matrix.M31;
            double yadd = point.X * matrix.M12 + matrix.M32;
            double x = point.X * matrix.M11;
            x += xadd;
            double y = point.Y * matrix.M22;
            y += yadd;
            return new Point(matrix.M11 * x, matrix.M22 * y);
        }
        public static MatrixTransform Inverse(this Transform transform)
        {
            var matrix = transform.Value.Invert();
            return new MatrixTransform(matrix);
        }
    }
}
