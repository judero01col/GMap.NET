using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    ///     GMap.NET polygon
    /// </summary>
    [Serializable]
    public class GMapPolygon : MapRoute, ISerializable, IDeserializationCallback, IDisposable
    {
        private bool _visible = true;

        /// <summary>
        ///     is polygon visible
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                if (value != _visible)
                {
                    _visible = value;

                    if (Overlay != null && Overlay.Control != null)
                    {
                        if (_visible)
                        {
                            Overlay.Control.UpdatePolygonLocalPosition(this);
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverPolygon)
                            {
                                Overlay.Control.IsMouseOverPolygon = false;
                                Overlay.Control.RestoreCursorOnLeave();
                            }
                        }

                        {
                            if (!Overlay.Control.HoldInvalidation)
                            {
                                Overlay.Control.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     can receive input
        /// </summary>
        public bool IsHitTestVisible = false;

        private bool _isMouseOver;

        /// <summary>
        ///     is mouse over
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }
            internal set
            {
                _isMouseOver = value;
            }
        }

        public GMapOverlay Overlay { get; set; }

        /// <summary>
        ///     Indicates whether the specified point is contained within this System.Drawing.Drawing2D.GraphicsPath
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool IsInsideLocal(int x, int y)
        {
            if (_graphicsPath != null)
            {
                return _graphicsPath.IsVisible(x, y);
            }

            return false;
        }

        GraphicsPath _graphicsPath;
        internal void UpdateGraphicsPath()
        {
            if (_graphicsPath == null)
            {
                _graphicsPath = new GraphicsPath();
            }
            else
            {
                _graphicsPath.Reset();
            }

            {
                var pnts = new Point[LocalPoints.Count];
                for (int i = 0; i < LocalPoints.Count; i++)
                {
                    var p2 = new Point((int)LocalPoints[i].X, (int)LocalPoints[i].Y);
                    pnts[pnts.Length - 1 - i] = p2;
                }

                if (pnts.Length > 2)
                {
                    _graphicsPath.AddPolygon(pnts);
                }
                else if (pnts.Length == 2)
                {
                    _graphicsPath.AddLines(pnts);
                }
            }
        }

        public virtual void OnRender(Graphics g)
        {
            if (IsVisible)
            {
                if (IsVisible)
                {
                    if (_graphicsPath != null)
                    {
                        g.FillPath(Fill, _graphicsPath);
                        g.DrawPath(Stroke, _graphicsPath);
                    }
                }
            }
        }

        //public double Area
        //{
        //   get
        //   {
        //      return 0;
        //   }
        //}

        public static readonly Pen DefaultStroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));

        /// <summary>
        ///     specifies how the outline is painted
        /// </summary>
        [NonSerialized] public Pen Stroke = DefaultStroke;

        public static readonly Brush DefaultFill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));

        /// <summary>
        ///     background color
        /// </summary>
        [NonSerialized] public Brush Fill = DefaultFill;

        public readonly List<GPoint> LocalPoints = new List<GPoint>();

        static GMapPolygon()
        {
            DefaultStroke.LineJoin = LineJoin.Round;
            DefaultStroke.Width = 5;
        }

        public GMapPolygon(List<PointLatLng> points, string name)
            : base(points, name)
        {
            LocalPoints.Capacity = Points.Count;
        }

        /// <summary>
        ///     checks if point is inside the polygon,
        ///     info.: http://greatmaps.codeplex.com/discussions/279437#post700449
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsInside(PointLatLng p)
        {
            int count = Points.Count;

            if (count < 3)
            {
                return false;
            }

            bool result = false;

            for (int i = 0, j = count - 1; i < count; i++)
            {
                var p1 = Points[i];
                var p2 = Points[j];

                if (p1.Lat < p.Lat && p2.Lat >= p.Lat || p2.Lat < p.Lat && p1.Lat >= p.Lat)
                {
                    if (p1.Lng + (p.Lat - p1.Lat) / (p2.Lat - p1.Lat) * (p2.Lng - p1.Lng) < p.Lng)
                    {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }

        #region ISerializable Members

        /// <summary>
        ///     Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the
        ///     target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">
        ///     The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this
        ///     serialization.
        /// </param>
        /// <exception cref="T:System.Security.SecurityException">
        ///     The caller does not have the required permission.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("LocalPoints", LocalPoints.ToArray());
            info.AddValue("Visible", IsVisible);
        }

        // Temp store for de-serialization.
        private GPoint[] deserializedLocalPoints;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MapRoute" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapPolygon(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            deserializedLocalPoints = Extensions.GetValue<GPoint[]>(info, "LocalPoints");
            IsVisible = Extensions.GetStruct(info, "Visible", true);
        }

        #endregion

        #region IDeserializationCallback Members

        /// <summary>
        ///     Runs when the entire object graph has been de-serialized.
        /// </summary>
        /// <param name="sender">
        ///     The object that initiated the callback. The functionality for this parameter is not currently
        ///     implemented.
        /// </param>
        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            // Accounts for the de-serialization being breadth first rather than depth first.
            LocalPoints.AddRange(deserializedLocalPoints);
            LocalPoints.Capacity = Points.Count;
        }

        #endregion

        #region IDisposable Members

        bool _disposed;

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                LocalPoints.Clear();

                if (_graphicsPath != null)
                {
                    _graphicsPath.Dispose();
                    _graphicsPath = null;
                }

                Clear();
            }
        }

        #endregion
    }

    public delegate void PolygonClick(GMapPolygon item, MouseEventArgs e);

    public delegate void PolygonDoubleClick(GMapPolygon item, MouseEventArgs e);

    public delegate void PolygonEnter(GMapPolygon item);

    public delegate void PolygonLeave(GMapPolygon item);
}
