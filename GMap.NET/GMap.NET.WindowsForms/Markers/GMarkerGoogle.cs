using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace GMap.NET.WindowsForms.Markers
{
    // ReSharper disable InconsistentNaming
    public enum GMarkerGoogleType
    {
        none = 0,
        arrow,
        blue,
        blue_small,
        blue_dot,
        blue_pushpin,
        brown_small,
        gray_small,
        green,
        green_small,
        green_dot,
        green_pushpin,
        green_big_go,
        yellow,
        yellow_small,
        yellow_dot,
        yellow_big_pause,
        yellow_pushpin,
        lightblue,
        lightblue_dot,
        lightblue_pushpin,
        orange,
        orange_small,
        orange_dot,
        pink,
        pink_dot,
        pink_pushpin,
        purple,
        purple_small,
        purple_dot,
        purple_pushpin,
        red,
        red_small,
        red_dot,
        red_pushpin,
        red_big_stop,
        black_small,
        white_small,
    }
    // ReSharper restore InconsistentNaming

    [Serializable]
    public class GMarkerGoogle : GMapMarker, ISerializable, IDeserializationCallback
    {
        private Bitmap _bitmap;

        public Bitmap Bitmap
        {
            get { return _bitmap; }
            set
            {
                _bitmap = value;
            }
        }

        Bitmap _bitmapShadow;

        static Bitmap _arrowShadow;
        static Bitmap _msmarkerShadow;
        static Bitmap _shadowSmall;
        static Bitmap _pushpinShadow;

        public readonly GMarkerGoogleType Type;

        public GMarkerGoogle(PointLatLng p, GMarkerGoogleType type)
            : base(p)
        {
            Type = type;

            if (type != GMarkerGoogleType.none)
            {
                LoadBitmap();
            }
        }

        void LoadBitmap()
        {
            Bitmap = GetIcon(Type.ToString());
            Size = new Size(Bitmap.Width, Bitmap.Height);

            switch (Type)
            {
                case GMarkerGoogleType.arrow:
                {
                    Offset = new Point(-11, -Size.Height);

                    if (_arrowShadow == null)
                    {
                        _arrowShadow = Properties.Resources.arrowshadow;
                    }

                    _bitmapShadow = _arrowShadow;
                }
                    break;

                case GMarkerGoogleType.blue:
                case GMarkerGoogleType.blue_dot:
                case GMarkerGoogleType.green:
                case GMarkerGoogleType.green_dot:
                case GMarkerGoogleType.yellow:
                case GMarkerGoogleType.yellow_dot:
                case GMarkerGoogleType.lightblue:
                case GMarkerGoogleType.lightblue_dot:
                case GMarkerGoogleType.orange:
                case GMarkerGoogleType.orange_dot:
                case GMarkerGoogleType.pink:
                case GMarkerGoogleType.pink_dot:
                case GMarkerGoogleType.purple:
                case GMarkerGoogleType.purple_dot:
                case GMarkerGoogleType.red:
                case GMarkerGoogleType.red_dot:
                {
                    Offset = new Point(-Size.Width / 2 + 1, -Size.Height + 1);

                    if (_msmarkerShadow == null)
                    {
                        _msmarkerShadow = Properties.Resources.msmarker_shadow;
                    }

                    _bitmapShadow = _msmarkerShadow;
                }
                    break;

                case GMarkerGoogleType.black_small:
                case GMarkerGoogleType.blue_small:
                case GMarkerGoogleType.brown_small:
                case GMarkerGoogleType.gray_small:
                case GMarkerGoogleType.green_small:
                case GMarkerGoogleType.yellow_small:
                case GMarkerGoogleType.orange_small:
                case GMarkerGoogleType.purple_small:
                case GMarkerGoogleType.red_small:
                case GMarkerGoogleType.white_small:
                {
                    Offset = new Point(-Size.Width / 2, -Size.Height + 1);

                    if (_shadowSmall == null)
                    {
                        _shadowSmall = Properties.Resources.shadow_small;
                    }

                    _bitmapShadow = _shadowSmall;
                }
                    break;

                case GMarkerGoogleType.green_big_go:
                case GMarkerGoogleType.yellow_big_pause:
                case GMarkerGoogleType.red_big_stop:
                {
                    Offset = new Point(-Size.Width / 2, -Size.Height + 1);
                    if (_msmarkerShadow == null)
                    {
                        _msmarkerShadow = Properties.Resources.msmarker_shadow;
                    }

                    _bitmapShadow = _msmarkerShadow;
                }
                    break;

                case GMarkerGoogleType.blue_pushpin:
                case GMarkerGoogleType.green_pushpin:
                case GMarkerGoogleType.yellow_pushpin:
                case GMarkerGoogleType.lightblue_pushpin:
                case GMarkerGoogleType.pink_pushpin:
                case GMarkerGoogleType.purple_pushpin:
                case GMarkerGoogleType.red_pushpin:
                {
                    Offset = new Point(-9, -Size.Height + 1);

                    if (_pushpinShadow == null)
                    {
                        _pushpinShadow = Properties.Resources.pushpin_shadow;
                    }

                    _bitmapShadow = _pushpinShadow;
                }
                    break;
            }
        }

        /// <summary>
        ///     marker using manual bitmap, NonSerialized
        /// </summary>
        /// <param name="p"></param>
        /// <param name="bitmap"></param>
        public GMarkerGoogle(PointLatLng p, Bitmap bitmap)
            : base(p)
        {
            Bitmap = bitmap;
            Size = new Size(bitmap.Width, bitmap.Height);
            Offset = new Point(-Size.Width / 2, -Size.Height);
        }

        static readonly Dictionary<string, Bitmap> IconCache = new Dictionary<string, Bitmap>();

        internal static Bitmap GetIcon(string name)
        {
            Bitmap ret;
            if (!IconCache.TryGetValue(name, out ret))
            {
                ret = Properties.Resources.ResourceManager.GetObject(name, Properties.Resources.Culture) as Bitmap;
                IconCache.Add(name, ret);
            }

            return ret;
        }

        public override void OnRender(Graphics g)
        {
            lock (Bitmap)
            {
                if (_bitmapShadow != null)
                {
                    g.DrawImage(_bitmapShadow,
                        LocalPosition.X,
                        LocalPosition.Y,
                        _bitmapShadow.Width,
                        _bitmapShadow.Height);
                }

                g.DrawImage(Bitmap, LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height);
            }

            //g.DrawString(LocalPosition.ToString(), SystemFonts.DefaultFont, Brushes.Red, LocalPosition);
        }

        public override void Dispose()
        {
            if (Bitmap != null)
            {
                if (!IconCache.ContainsValue(Bitmap))
                {
                    Bitmap.Dispose();
                    Bitmap = null;
                }
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("type", Type);
            //info.AddValue("Bearing", this.Bearing);

            base.GetObjectData(info, context);
        }

        protected GMarkerGoogle(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Type = Extensions.GetStruct(info, "type", GMarkerGoogleType.none);
            //this.Bearing = Extensions.GetStruct<float>(info, "Bearing", null);
        }

        #endregion

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            if (Type != GMarkerGoogleType.none)
            {
                LoadBitmap();
            }
        }

        #endregion
    }
}
