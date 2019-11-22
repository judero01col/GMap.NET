using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Demo.WindowsPresentation.Controls;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation.CustomMarkers
{
    public class CircleVisual : FrameworkElement
    {
        public readonly Popup Popup = new Popup();
        public readonly TrolleyTooltip Tooltip = new TrolleyTooltip();
        public readonly GMapMarker Marker;

        public CircleVisual(GMapMarker m, Brush background)
        {
            Marker = m;
            Marker.ZIndex = 100;

            Popup.AllowsTransparency = true;
            Popup.PlacementTarget = this;
            Popup.Placement = PlacementMode.Mouse;
            Popup.Child = Tooltip;
            Popup.Child.Opacity = 0.777;

            SizeChanged += CircleVisual_SizeChanged;
            MouseEnter += CircleVisual_MouseEnter;
            MouseLeave += CircleVisual_MouseLeave;
            Loaded += OnLoaded;

            Text = "?";

            StrokeArrow.EndLineCap = PenLineCap.Triangle;
            StrokeArrow.LineJoin = PenLineJoin.Round;

            RenderTransform = _scale;

            Width = Height = 22;
            FontSize = Width / 1.55;

            Background = background;
            Angle = null;
        }

        void CircleVisual_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
            _scale.CenterX = -Marker.Offset.X;
            _scale.CenterY = -Marker.Offset.Y;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisual(true);
        }

        readonly ScaleTransform _scale = new ScaleTransform(1, 1);

        void CircleVisual_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Popup.IsOpen)
            {
                Popup.IsOpen = false;
            }

            Marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;

            Effect = null;

            _scale.ScaleY = 1;
            _scale.ScaleX = 1;
        }

        void CircleVisual_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Popup.IsOpen)
            {
                Popup.IsOpen = true;
            }

            Marker.ZIndex += 10000;
            Cursor = Cursors.Hand;

            Effect = ShadowEffect;

            _scale.ScaleY = 1.5;
            _scale.ScaleX = 1.5;
        }

        public DropShadowEffect ShadowEffect;

        static readonly Typeface Font = new Typeface(new FontFamily("Arial"),
            FontStyles.Normal,
            FontWeights.Bold,
            FontStretches.Normal);

        FormattedText _fText;

        private Brush _background = Brushes.Blue;

        public Brush Background
        {
            get
            {
                return _background;
            }
            set
            {
                if (_background != value)
                {
                    _background = value;
                    IsChanged = true;
                }
            }
        }

        private Brush _foreground = Brushes.White;

        public Brush Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    IsChanged = true;

                    ForceUpdateText();
                }
            }
        }

        private Pen _stroke = new Pen(Brushes.Blue, 2.0);

        public Pen Stroke
        {
            get
            {
                return _stroke;
            }
            set
            {
                if (_stroke != value)
                {
                    _stroke = value;
                    IsChanged = true;
                }
            }
        }

        private Pen _strokeArrow = new Pen(Brushes.Blue, 2.0);

        public Pen StrokeArrow
        {
            get
            {
                return _strokeArrow;
            }
            set
            {
                if (_strokeArrow != value)
                {
                    _strokeArrow = value;
                    IsChanged = true;
                }
            }
        }

        public double FontSize = 16;

        private double? _angle = 0;

        public double? Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                if (!Angle.HasValue || !value.HasValue ||
                    Angle.HasValue && value.HasValue && Math.Abs(_angle.Value - value.Value) > 11)
                {
                    _angle = value;
                    IsChanged = true;
                }
            }
        }

        public bool IsChanged = true;

        void ForceUpdateText()
        {
            _fText = new FormattedText(_text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Font,
                FontSize,
                Foreground);
            IsChanged = true;
        }

        string _text;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    ForceUpdateText();
                }
            }
        }

        Visual _child;

        public virtual Visual Child
        {
            get
            {
                return _child;
            }
            set
            {
                if (_child != value)
                {
                    if (_child != null)
                    {
                        RemoveLogicalChild(_child);
                        RemoveVisualChild(_child);
                    }

                    if (value != null)
                    {
                        AddVisualChild(value);
                        AddLogicalChild(value);
                    }

                    // cache the new child
                    _child = value;

                    InvalidateVisual();
                }
            }
        }

        public bool UpdateVisual(bool forceUpdate)
        {
            if (forceUpdate || IsChanged)
            {
                Child = Create();
                IsChanged = false;
                return true;
            }

            return false;
        }

        int _countCreate;

        private DrawingVisual Create()
        {
            _countCreate++;

            var square = new DrawingVisualFx();

            using (var dc = square.RenderOpen())
            {
                dc.DrawEllipse(null,
                    Stroke,
                    new Point(Width / 2, Height / 2),
                    Width / 2 + Stroke.Thickness / 2,
                    Height / 2 + Stroke.Thickness / 2);

                if (Angle.HasValue)
                {
                    dc.PushTransform(new RotateTransform(Angle.Value, Width / 2, Height / 2));
                    {
                        var polySeg = new PolyLineSegment(new[]
                            {
                                new Point(Width * 0.2, Height * 0.3), new Point(Width * 0.8, Height * 0.3)
                            },
                            true);
                        var pathFig = new PathFigure(new Point(Width * 0.5, -Height * 0.22),
                            new PathSegment[] {polySeg},
                            true);
                        var pathGeo = new PathGeometry(new[] {pathFig});
                        dc.DrawGeometry(Brushes.AliceBlue, StrokeArrow, pathGeo);
                    }
                    dc.Pop();
                }

                dc.DrawEllipse(Background, null, new Point(Width / 2, Height / 2), Width / 2, Height / 2);
                dc.DrawText(_fText, new Point(Width / 2 - _fText.Width / 2, Height / 2 - _fText.Height / 2));
            }

            return square;
        }

        #region Necessary Overrides -- Needed by WPF to maintain bookkeeping of our hosted visuals

        protected override int VisualChildrenCount
        {
            get
            {
                return Child == null ? 0 : 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Child;
        }

        #endregion
    }

    public class DrawingVisualFx : DrawingVisual
    {
        public static readonly DependencyProperty EffectProperty = DependencyProperty.Register("Effect",
            typeof(Effect),
            typeof(DrawingVisualFx),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnEffectChanged));

        public Effect Effect
        {
            get
            {
                return (Effect)GetValue(EffectProperty);
            }
            set
            {
                SetValue(EffectProperty, value);
            }
        }

        private static void OnEffectChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var drawingVisualFx = o as DrawingVisualFx;
            if (drawingVisualFx != null)
            {
                drawingVisualFx.SetMyProtectedVisualEffect((Effect)e.NewValue);
            }
        }

        private void SetMyProtectedVisualEffect(Effect effect)
        {
            VisualEffect = effect;
        }
    }
}
