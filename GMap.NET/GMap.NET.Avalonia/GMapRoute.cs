using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace GMap.NET.Avalonia
{
    public interface IShapable
    {
        List<PointLatLng> Points
        {
            get;
            set;
        }

        Path CreatePath(List<Point> localPath, bool addBlurEffect);
    }

    public class GMapRoute : GMapMarker, IShapable
    {
        public List<PointLatLng> Points { get; set; }

        public GMapRoute(IEnumerable<PointLatLng> points)
        {
            Points = new List<PointLatLng>(points);
        }

        public override void Clear()
        {
            base.Clear();
            Points.Clear();
        }

        /// <summary>
        ///     creates path from list of points, for performance set addBlurEffect to false
        /// </summary>
        /// <returns></returns>
        public virtual Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            // Create a StreamGeometry to use to specify myPath.
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                //ctx.BeginFigure(localPath[0], false, false);
                //// Draw a line to the next specified point.
                //ctx.PolyLineTo(localPath, true, true);

                ctx.BeginFigure(localPath[0], false);
                // Draw a line to the next specified point.
                foreach (var path in localPath)
                {
                    ctx.LineTo(path);
                }
            }

            // Freeze the geometry (make it unmodifiable)
            // for additional performance benefits.
            //geometry.Freeze();
            geometry.EndBatchUpdate();
            // Create a path to draw a geometry with.
            var myPath = new Path();
            {
                // Specify the shape of the Path using the StreamGeometry.
                myPath.Data = geometry;

                if (addBlurEffect)
                {
                    //BlurEffect ef = new BlurEffect();
                    //{
                    //    ef.KernelType = KernelType.Gaussian;
                    //    ef.Radius = 3.0;
                    //    ef.RenderingBias = RenderingBias.Performance;
                    //}
                    //myPath.Effect = ef;
                }

                myPath.Stroke = Brushes.Navy;
                myPath.StrokeThickness = 5;
                myPath.StrokeJoin = PenLineJoin.Round;
                myPath.StrokeLineCap = PenLineCap.Square;
                //myPath.StrokeEndLineCap = PenLineCap.Square;
                myPath.Opacity = 0.6;
                myPath.IsHitTestVisible = false;
            }
            return myPath;
        }
    }
}
