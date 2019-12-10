using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Forms;
using GMap.NET.WindowsForms.ToolTips;

namespace GMap.NET.WindowsForms
{
    /// <summary>
    ///     GMap.NET marker
    /// </summary>
    [Serializable]
    public abstract class GMapMarker : ISerializable, IDisposable
    {
        GMapOverlay _overlay;

        public GMapOverlay Overlay
        {
            get
            {
                return _overlay;
            }
            internal set
            {
                _overlay = value;
            }
        }

        private PointLatLng _position;

        public PointLatLng Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (_position != value)
                {
                    _position = value;

                    if (IsVisible)
                    {
                        if (Overlay != null && Overlay.Control != null)
                        {
                            Overlay.Control.UpdateMarkerLocalPosition(this);
                        }
                    }
                }
            }
        }

        public object Tag;

        Point _offset;

        public Point Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                if (_offset != value)
                {
                    _offset = value;

                    if (IsVisible)
                    {
                        if (Overlay != null && Overlay.Control != null)
                        {
                            Overlay.Control.UpdateMarkerLocalPosition(this);
                        }
                    }
                }
            }
        }

        Rectangle _area;

        /// <summary>
        ///     marker position in local coordinates, internal only, do not set it manualy
        /// </summary>
        public Point LocalPosition
        {
            get
            {
                return _area.Location;
            }
            set
            {
                if (_area.Location != value)
                {
                    _area.Location = value;
                    {
                        if (Overlay != null && Overlay.Control != null)
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
        ///     ToolTip position in local coordinates
        /// </summary>
        public Point ToolTipPosition
        {
            get
            {
                var ret = _area.Location;
                ret.Offset(-Offset.X, -Offset.Y);
                return ret;
            }
        }

        public Size Size
        {
            get
            {
                return _area.Size;
            }
            set
            {
                _area.Size = value;
            }
        }

        public Rectangle LocalArea
        {
            get
            {
                return _area;
            }
        }

        public GMapToolTip ToolTip;

        public MarkerTooltipMode ToolTipMode = MarkerTooltipMode.OnMouseOver;

        string _toolTipText;

        public string ToolTipText
        {
            get
            {
                return _toolTipText;
            }

            set
            {
                if (ToolTip == null && !string.IsNullOrEmpty(value))
                {
                    ToolTip = new GMapRoundedToolTip(this);
                }

                _toolTipText = value;
            }
        }

        private bool _visible = true;

        /// <summary>
        ///     is marker visible
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
                            Overlay.Control.UpdateMarkerLocalPosition(this);
                        }
                        else
                        {
                            if (Overlay.Control.IsMouseOverMarker)
                            {
                                Overlay.Control.IsMouseOverMarker = false;
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
        ///     if true, marker will be rendered even if it's outside current view
        /// </summary>
        public bool DisableRegionCheck;

        /// <summary>
        ///     can maker receive input
        /// </summary>
        public bool IsHitTestVisible = true;

        private bool _isMouseOver;

        /// <summary>
        ///     is mouse over marker
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

        public GMapMarker(PointLatLng pos)
        {
            Position = pos;
        }

        public virtual void OnRender(Graphics g)
        {
            //
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Position", Position);
            info.AddValue("Tag", Tag);
            info.AddValue("Offset", Offset);
            info.AddValue("Area", _area);
            info.AddValue("ToolTip", ToolTip);
            info.AddValue("ToolTipMode", ToolTipMode);
            info.AddValue("ToolTipText", ToolTipText);
            info.AddValue("Visible", IsVisible);
            info.AddValue("DisableregionCheck", DisableRegionCheck);
            info.AddValue("IsHitTestVisible", IsHitTestVisible);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GMapMarker" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected GMapMarker(SerializationInfo info, StreamingContext context)
        {
            Position = Extensions.GetStruct(info, "Position", PointLatLng.Empty);
            Tag = Extensions.GetValue<object>(info, "Tag", null);
            Offset = Extensions.GetStruct(info, "Offset", Point.Empty);
            _area = Extensions.GetStruct(info, "Area", Rectangle.Empty);

            ToolTip = Extensions.GetValue<GMapToolTip>(info, "ToolTip", null);
            if (ToolTip != null) ToolTip.Marker = this;

            ToolTipMode =
                Extensions.GetStruct(info, "ToolTipMode", MarkerTooltipMode.OnMouseOver);
            ToolTipText = info.GetString("ToolTipText");
            IsVisible = info.GetBoolean("Visible");
            DisableRegionCheck = info.GetBoolean("DisableregionCheck");
            IsHitTestVisible = info.GetBoolean("IsHitTestVisible");
        }

        #endregion

        #region IDisposable Members

        bool _disposed;

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                Tag = null;

                if (ToolTip != null)
                {
                    _toolTipText = null;
                    ToolTip.Dispose();
                    ToolTip = null;
                }
            }
        }

        #endregion
    }

    public delegate void MarkerClick(GMapMarker item, MouseEventArgs e);

    public delegate void MarkerDoubleClick(GMapMarker item, MouseEventArgs e);

    public delegate void MarkerEnter(GMapMarker item);

    public delegate void MarkerLeave(GMapMarker item);

    /// <summary>
    ///     modeof tooltip
    /// </summary>
    public enum MarkerTooltipMode
    {
        OnMouseOver,
        Never,
        Always,
    }
}
