using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.VisualTree;

namespace GMap.NET.Avalonia
{
    /// <summary>
    ///     GMap.NET marker
    /// </summary>
    public class GMapMarker : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, name);
            }
        }

        Visual _shape;
        static readonly PropertyChangedEventArgs ShapePropertyChangedEventArgs = new PropertyChangedEventArgs(nameof(Shape));

        /// <summary>
        ///     marker visual
        /// </summary>
        public Visual Shape
        {
            get
            {
                return _shape;
            }
            set
            {
                if (_shape != value)
                {
                    _shape = value;
                    OnPropertyChanged(ShapePropertyChangedEventArgs);

                    UpdateLocalPosition();
                }
            }
        }

        private PointLatLng _position;

        /// <summary>
        ///     coordinate of marker
        /// </summary>
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
                    UpdateLocalPosition();
                }
            }
        }

        GMapControl _map;

        /// <summary>
        ///     the map of this marker
        /// </summary>
        public GMapControl Map
        {
            get
            {
                if (Shape != null && _map == null)
                {
                    _map = Shape.GetVisualParent<GMapControl>();
                }

                return _map;
            }
            internal set
            {
                _map = value;
            }
        }

        /// <summary>
        ///     custom object
        /// </summary>
        public object Tag;

        Point _offset;

        /// <summary>
        ///     offset of marker
        /// </summary>
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
                    UpdateLocalPosition();
                }
            }
        }

        int _localPositionX;

        static readonly PropertyChangedEventArgs LocalPositionXPropertyChangedEventArgs =
            new PropertyChangedEventArgs(nameof(LocalPositionX));

        /// <summary>
        ///     local X position of marker
        /// </summary>
        public int LocalPositionX
        {
            get
            {
                return _localPositionX;
            }
            internal set
            {
                if (_localPositionX != value)
                {
                    _localPositionX = value;
                    OnPropertyChanged(LocalPositionXPropertyChangedEventArgs);
                }
            }
        }

        int _localPositionY;

        static readonly PropertyChangedEventArgs LocalPositionYPropertyChangedEventArgs =
            new PropertyChangedEventArgs(nameof(LocalPositionY));

        /// <summary>
        ///     local Y position of marker
        /// </summary>
        public int LocalPositionY
        {
            get
            {
                return _localPositionY;
            }
            internal set
            {
                if (_localPositionY != value)
                {
                    _localPositionY = value;
                    OnPropertyChanged(LocalPositionYPropertyChangedEventArgs);
                }
            }
        }

        int _zIndex;

        static readonly PropertyChangedEventArgs ZIndexPropertyChangedEventArgs =
            new PropertyChangedEventArgs(nameof(ZIndex));

        /// <summary>
        ///     the index of Z, render order
        /// </summary>
        public int ZIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                if (_zIndex != value)
                {
                    _zIndex = value;
                    OnPropertyChanged(ZIndexPropertyChangedEventArgs);
                }
            }
        }

        public GMapMarker(PointLatLng pos)
        {
            Position = pos;
        }

        internal GMapMarker()
        {
        }

        /// <summary>
        ///     calls Dispose on shape if it implements IDisposable, sets shape to null and clears route
        /// </summary>
        public virtual void Clear()
        {
            var s = Shape as IDisposable;
            if (s != null)
            {
                s.Dispose();
                s = null;
            }

            Shape = null;
        }

        /// <summary>
        ///     updates marker position, internal access usualy
        /// </summary>
        void UpdateLocalPosition()
        {
            if (Map != null)
            {
                var p = Map.FromLatLngToLocal(Position);
                p.Offset(-(long)Map.MapTranslateTransform.X, -(long)Map.MapTranslateTransform.Y);

                LocalPositionX = (int)(p.X + (long)Offset.X);
                LocalPositionY = (int)(p.Y + (long)Offset.Y);
            }
        }

        /// <summary>
        ///     forces to update local marker  position
        ///     dot not call it if you don't really need to ;}
        /// </summary>
        /// <param name="m"></param>
        internal void ForceUpdateLocalPosition(GMapControl m)
        {
            if (m != null)
            {
                _map = m;
            }

            UpdateLocalPosition();
        }
    }
}
