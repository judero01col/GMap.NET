using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.Projections;

namespace GMap.NET.Avalonia
{
    /// <summary>
    ///     GMap.NET control for Windows Presentation
    /// </summary>
    public partial class GMapControl : ItemsControl, Interface, IDisposable
    {
        private const double MinimumHorizontalDragDistance = 3;
        private const double MinimumVerticalDragDistance = 3;

        #region DependencyProperties and related stuff

        /// <summary>
        ///     type of map
        /// </summary>
        [Browsable(false)]
        public GMapProvider MapProvider
        {
            get { return GetValue(MapProviderProperty); }
            set { SetValue(MapProviderProperty, value); }
        }

        public Point MapPoint
        {
            get { return GetValue(MapPointProperty); }
            set { SetValue(MapPointProperty, value); }
        }

        public static readonly StyledProperty<PointLatLng> CenterPositionProperty =
            AvaloniaProperty.Register<GMapControl, PointLatLng>(
                nameof(CenterPosition),
                notifying: CenterPositionPropertyChanged);

        // Using a DependencyProperty as the backing store for point.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<Point> MapPointProperty =
            AvaloniaProperty.Register<GMapControl, Point>(
                nameof(MapPoint),
                notifying: OnMapPointPropertyChanged);

        private static void OnMapPointPropertyChanged(IAvaloniaObject source, bool e)
        {
            if (e) return;
            if (source is not GMapControl gmap) return;
            gmap.Position = new PointLatLng(gmap.MapPoint.X, gmap.MapPoint.Y);
        }

        public static readonly StyledProperty<GMapProvider> MapProviderProperty =
            AvaloniaProperty.Register<GMapControl, GMapProvider>(
                nameof(MapProvider),
                notifying: MapProviderPropertyChanged);

        public static readonly StyledProperty<double> ZoomProperty =
            AvaloniaProperty.Register<GMapControl, double>(
                nameof(Zoom),
                //notifying: ZoomPropertyChanged,
                coerce: OnCoerceZoom);

        /// <summary>
        ///     The zoom x property
        /// </summary>
        public static readonly StyledProperty<double> ZoomXProperty =
            AvaloniaProperty.Register<GMapControl, double>(
                nameof(ZoomX),
                //notifying: ZoomXPropertyChanged,
                coerce: OnCoerceZoom);

        /// <summary>
        ///     The zoom y property
        /// </summary>
        public static readonly StyledProperty<double> ZoomYProperty =
            AvaloniaProperty.Register<GMapControl, double>(
                nameof(ZoomY),
                //notifying: ZoomYPropertyChanged,
                coerce: OnCoerceZoom);

        /// <summary>
        ///     The multi touch enabled property
        /// </summary>
        public static readonly StyledProperty<bool> MultiTouchEnabledProperty =
            AvaloniaProperty.Register<GMapControl, bool>(
                nameof(MultiTouchEnabled));

        /// <summary>
        ///     The touch enabled property
        /// </summary>
        public static readonly StyledProperty<bool> TouchEnabledProperty =
            AvaloniaProperty.Register<GMapControl, bool>(
                nameof(TouchEnabled));

        /// <summary>
        ///     map zoom
        /// </summary>
        [Category("GMap.NET")]
        public double Zoom
        {
            get { return GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        /// <summary>
        ///     Map Zoom X
        /// </summary>
        [Category("GMap.NET")]
        public double ZoomX
        {
            get { return GetValue(ZoomXProperty); }
            set { SetValue(ZoomXProperty, value); }
        }

        /// <summary>
        ///     Map Zoom Y
        /// </summary>
        [Category("GMap.NET")]
        public double ZoomY
        {
            get { return GetValue(ZoomYProperty); }
            set { SetValue(ZoomYProperty, value); }
        }

        [Category("GMap.NET")]
        public PointLatLng CenterPosition
        {
            get { return GetValue(CenterPositionProperty); }
            set { SetValue(CenterPositionProperty, value); }
        }

        /// <summary>
        ///     Specifies, if a floating map scale is displayed using a
        ///     stretched, or a narrowed map.
        ///     If <code>ScaleMode</code> is <code>ScaleDown</code>,
        ///     then a scale of 12.3 is displayed using a map zoom level of 13
        ///     resized to the lower level. If the parameter is <code>ScaleUp</code> ,
        ///     then the same scale is displayed using a zoom level of 12 with an
        ///     enlarged scale. If the value is <code>Dynamic</code>, then until a
        ///     remainder of 0.25 <code>ScaleUp</code> is applied, for bigger
        ///     remainders <code>ScaleDown</code>.
        /// </summary>
        [Category("GMap.NET")]
        [Description("map scale type")]
        public ScaleModes ScaleMode
        {
            get { return _scaleMode; }
            set
            {
                _scaleMode = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [multi touch enabled].
        /// </summary>
        /// <value><c>true</c> if [multi touch enabled]; otherwise, <c>false</c>.</value>
        [Category("GMap.NET")]
        [Description("Enable pinch map zoom")]
        public bool MultiTouchEnabled
        {
            get { return GetValue(MultiTouchEnabledProperty); }
            set { SetValue(MultiTouchEnabledProperty, value); }
        }

        private static double OnCoerceZoom(IAvaloniaObject o, double value)
        {
            if (o is GMapControl map)
            {
                double result = value;

                if (result > map.MaxZoom)
                    result = map.MaxZoom;

                if (result < map.MinZoom)
                    result = map.MinZoom;

                return result;
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        ///     Centers the position property changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void CenterPositionPropertyChanged(IAvaloniaObject obj, bool e)
        {
            if (e) return;

            if (obj is not GMapControl gmapControl)
                return;

            gmapControl.Position = gmapControl.CenterPosition;
        }

        private static void MapProviderPropertyChanged(IAvaloniaObject d, bool e)
        {
            if (e) return;
            var map = (GMapControl)d;

            if (map != null && map.MapProvider != null)
            {
                //Debug.WriteLine("MapType: " + e.OldValue + " -> " + e.NewValue);

                var viewarea = map.SelectedArea;

                if (viewarea != RectLatLng.Empty)
                {
                    map.Position = new PointLatLng(viewarea.Lat - viewarea.HeightLat / 2,
                        viewarea.Lng + viewarea.WidthLng / 2);
                }
                else
                {
                    viewarea = map.ViewArea;
                }

                map._core.Provider = map.MapProvider;

                map._copyright = null;

                if (!string.IsNullOrEmpty(map._core.Provider.Copyright))
                {
                    map._copyright = new FormattedText(map._core.Provider.Copyright,
                        new Typeface("GenericSansSerif"),
                        9,
                        TextAlignment.Left,
                        TextWrapping.Wrap,
                        new Size()
                        );
                }

                if (map._core.IsStarted && map._core.ZoomToArea)
                {
                    // restore zoomrect as close as possible
                    if (viewarea != RectLatLng.Empty && viewarea != map.ViewArea)
                    {
                        int bestZoom = map._core.GetMaxZoomToFitRect(viewarea);

                        if (bestZoom > 0 && map.Zoom != bestZoom)
                            map.Zoom = bestZoom;
                    }
                }
            }
        }

        private static void ZoomPropertyChanged(GMapControl mapControl, double value, double oldValue,
            ZoomMode zoomMode)
        {
            if (mapControl != null && mapControl.MapProvider?.Projection != null)
            {
                Debug.WriteLine("Zoom: " + oldValue + " -> " + value);

                double remainder = value % 1;

                if (mapControl.ScaleMode != ScaleModes.Integer && remainder != 0 && mapControl.Bounds.Width > 0)
                {
                    bool scaleDown;

                    switch (mapControl.ScaleMode)
                    {
                        case ScaleModes.ScaleDown:
                            scaleDown = true;
                            break;

                        case ScaleModes.Dynamic:
                            scaleDown = remainder > 0.25;
                            break;

                        default:
                            scaleDown = false;
                            break;
                    }

                    if (scaleDown)
                        remainder--;

                    double scaleValue = Math.Pow(2d, remainder);
                    {
                        if (mapControl.MapScaleTransform == null)
                        {
                            mapControl.MapScaleTransform = mapControl._lastScaleTransform;
                        }

                        if (zoomMode == ZoomMode.XY || zoomMode == ZoomMode.X)
                        {
                            mapControl.MapScaleTransform.ScaleX = scaleValue;
                            mapControl._core.ScaleX = 1 / scaleValue;
                            //mapControl.MapScaleTransform.CenterX = mapControl.Width / 2;
                        }

                        if (zoomMode == ZoomMode.XY || zoomMode == ZoomMode.Y)
                        {
                            mapControl.MapScaleTransform.ScaleY = scaleValue;
                            mapControl._core.ScaleY = 1 / scaleValue;
                            //mapControl.MapScaleTransform.CenterY = mapControl.Height / 2;
                        }
                    }

                    mapControl._core.Zoom = Convert.ToInt32(scaleDown ? Math.Ceiling(value) : value - remainder);
                }
                else
                {
                    mapControl.MapScaleTransform = null;

                    if (zoomMode == ZoomMode.XY || zoomMode == ZoomMode.X)
                        mapControl._core.ScaleX = 1;

                    if (zoomMode == ZoomMode.XY || zoomMode == ZoomMode.Y)
                        mapControl._core.ScaleY = 1;

                    mapControl._core.Zoom = (int)Math.Floor(value);
                }

                if (mapControl.IsInitialized)
                {
                    mapControl.ForceUpdateOverlays();
                    mapControl.InvalidateVisual(true);
                }
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);
            switch (change.Property.Name)
            {
                case nameof(Zoom):
                    ZoomPropertyChanged(
                        this,
                        change.OldValue.GetValueOrDefault<double>(),
                        change.OldValue.GetValueOrDefault<double>(),
                        ZoomMode.XY);
                    break;

                case nameof(ZoomX):
                    ZoomPropertyChanged(
                       this,
                       change.OldValue.GetValueOrDefault<double>(),
                       change.OldValue.GetValueOrDefault<double>(),
                       ZoomMode.X);
                    break;

                case nameof(ZoomY):
                    ZoomPropertyChanged(
                       this,
                       change.OldValue.GetValueOrDefault<double>(),
                       change.OldValue.GetValueOrDefault<double>(),
                       ZoomMode.Y);
                    break;

                case nameof(Position):
                    PositionChanged();
                    break;
            }
        }

        /// <summary>
        ///     Handles the <see cref="E:MultiTouchEnabledChanged" /> event.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        //private static void OnMultiTouchEnabledChanged(IAvaloniaObject d, bool e)
        //{
        //    if (e) return;

        //    var mapControl = (GMapControl)d;
        //    mapControl.MultiTouchEnabled = (bool)e.NewValue;
        //    //mapControl.IsManipulationEnabled = (bool)e.NewValue;
        //}

        /// <summary>
        ///     is touch control enabled
        /// </summary>
        /// <value><c>true</c> if [touch enabled]; otherwise, <c>false</c>.</value>
        [Obsolete("Touch Enabled is deprecated, please use MultiTouchEnabled")]
        public bool TouchEnabled
        {
            get { return GetValue(TouchEnabledProperty); }
            set { SetValue(TouchEnabledProperty, value); }
        }

        private readonly ScaleTransform _lastScaleTransform = new ScaleTransform();

        private ScaleModes _scaleMode = ScaleModes.Integer;

        #endregion DependencyProperties and related stuff

        private readonly MouseDevice _mouse = new MouseDevice();

        private readonly Core _core = new Core();

        private PointLatLng _selectionStart;
        private PointLatLng _selectionEnd;
        private readonly Typeface _tileTypeface = new Typeface("Arial");
        private bool _showTileGridLines;

        private FormattedText? _copyright;

        /// <summary>
        ///     enables filling empty tiles using lower level images
        /// </summary>
        [Browsable(false)]
        public bool FillEmptyTiles
        {
            get { return _core.FillEmptyTiles; }
            set { _core.FillEmptyTiles = value; }
        }

        /// <summary>
        ///     max zoom
        /// </summary>
        [Category("GMap.NET")]
        [Description("maximum zoom level of map")]
        public int MaxZoom
        {
            get { return _core.MaxZoom; }
            set { _core.MaxZoom = value; }
        }

        /// <summary>
        ///     min zoom
        /// </summary>
        [Category("GMap.NET")]
        [Description("minimum zoom level of map")]
        public int MinZoom
        {
            get { return _core.MinZoom; }
            set { _core.MinZoom = value; }
        }

        /// <summary>
        ///     pen for empty tile borders
        /// </summary>
        public Pen EmptyTileBorders { get; } = new(Brushes.White, 1.0);

        /// <summary>
        ///     pen for Selection
        /// </summary>
        public Pen SelectionPen { get; } = new(Brushes.Blue, 2.0);

        /// <summary>
        ///     background of selected area
        /// </summary>
        public Brush SelectedAreaFill { get; } =
            new SolidColorBrush(Color.FromArgb(33, Colors.RoyalBlue.R, Colors.RoyalBlue.G, Colors.RoyalBlue.B));

        /// <summary>
        ///     pen for empty tile background
        /// </summary>
        public ISolidColorBrush EmptyTileBrush = Brushes.Navy;

        /// <summary>
        ///     text on empty tiles
        /// </summary>
        public FormattedText EmptyTileText { get; } =
            GetFormattedText("We are sorry, but we don't\nhave imagery at this zoom\n     level for this region.", 16);

        /// <summary>
        ///     map zooming type for mouse wheel
        /// </summary>
        [Category("GMap.NET")]
        [Description("map zooming type for mouse wheel")]
        public MouseWheelZoomType MouseWheelZoomType
        {
            get { return _core.MouseWheelZoomType; }
            set { _core.MouseWheelZoomType = value; }
        }

        /// <summary>
        ///     enable map zoom on mouse wheel
        /// </summary>
        [Category("GMap.NET")]
        [Description("enable map zoom on mouse wheel")]
        public bool MouseWheelZoomEnabled
        {
            get { return _core.MouseWheelZoomEnabled; }
            set { _core.MouseWheelZoomEnabled = value; }
        }

        /// <summary>
        ///     map dragg button
        /// </summary>
        [Category("GMap.NET")]
        public PointerUpdateKind DragButton { get; } = PointerUpdateKind.LeftButtonPressed;

        /// <summary>
        ///     use circle for selection
        /// </summary>
        public bool SelectionUseCircle { get; } = false;

        /// <summary>
        ///     shows tile gridlines
        /// </summary>
        [Category("GMap.NET")]
        public bool ShowTileGridLines
        {
            get { return _showTileGridLines; }
            set
            {
                _showTileGridLines = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        ///     retry count to get tile
        /// </summary>
        [Browsable(false)]
        public int RetryLoadTile
        {
            get { return _core.RetryLoadTile; }
            set { _core.RetryLoadTile = value; }
        }

        /// <summary>
        ///     how many levels of tiles are staying decompresed in memory
        /// </summary>
        [Browsable(false)]
        public int LevelsKeepInMemory
        {
            get { return _core.LevelsKeepInMemory; }
            set { _core.LevelsKeepInMemory = value; }
        }

        /// <summary>
        ///     current selected area in map
        /// </summary>
        private RectLatLng _selectedArea;

        [Browsable(false)]
        public RectLatLng SelectedArea
        {
            get { return _selectedArea; }
            set
            {
                _selectedArea = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        ///     map boundaries
        /// </summary>
        public RectLatLng? BoundsOfMap { get; }

        /// <summary>
        ///     occurs when mouse selection is changed
        /// </summary>
        public event SelectionChange OnSelectionChange;

        public static readonly StyledProperty<ObservableCollection<GMapMarker>> MarkersProperty =
            AvaloniaProperty.Register<GMapControl, ObservableCollection<GMapMarker>>(
                nameof(Markers));

        /// <summary>
        ///     List of markers
        /// </summary>
        public ObservableCollection<GMapMarker> Markers
        {
            get { return GetValue(MarkersProperty); }
            private set { SetValue(MarkersProperty, value); }
        }

        /// <summary>
        ///     current markers overlay offset
        /// </summary>
        internal readonly TranslateTransform MapTranslateTransform = new TranslateTransform();

        internal readonly TranslateTransform MapOverlayTranslateTransform = new TranslateTransform();

        internal ScaleTransform? MapScaleTransform = new ScaleTransform();
        internal RotateTransform MapRotateTransform = new RotateTransform();

        protected bool DesignModeInConstruct
        {
            get { return Design.IsDesignMode; }
        }

        private Canvas _mapCanvas;

        /// <summary>
        ///     markers overlay
        /// </summary>
        internal Canvas MapCanvas
        {
            get
            {
                if (_mapCanvas == null)
                {
                    if (VisualChildren.Count > 0)
                    {
                        _mapCanvas = this.GetVisualDescendants().FirstOrDefault(w => w is Canvas) as Canvas;
                        _mapCanvas.RenderTransform = MapTranslateTransform;
                    }
                }

                return _mapCanvas;
            }
        }

        public GMaps Manager
        {
            get { return GMaps.Instance; }
        }

        private static DataTemplate _dataTemplateInstance;
        private static ItemsPanelTemplate _itemsPanelTemplateInstance;
        private static Style _styleInstance;

        public GMapControl()
        {
            if (!DesignModeInConstruct)
            {
                #region -- templates --

                #region -- xaml --

                //  <ItemsControl Name="figures">
                //    <ItemsControl.ItemTemplate>
                //        <DataTemplate>
                //            <ContentPresenter Content="{Binding Path=Shape}" />
                //        </DataTemplate>
                //    </ItemsControl.ItemTemplate>
                //    <ItemsControl.ItemsPanel>
                //        <ItemsPanelTemplate>
                //            <Canvas />
                //        </ItemsPanelTemplate>
                //    </ItemsControl.ItemsPanel>
                //    <ItemsControl.ItemContainerStyle>
                //        <Style>
                //            <Setter Property="Canvas.Left" Value="{Binding Path=LocalPositionX}"/>
                //            <Setter Property="Canvas.Top" Value="{Binding Path=LocalPositionY}"/>
                //        </Style>
                //    </ItemsControl.ItemContainerStyle>
                //</ItemsControl>

                #endregion -- xaml --

                _dataTemplateInstance ??= new DataTemplate()
                {
                    Content = new ContentPresenter
                    {
                        Content = new Binding("Shape")
                    }
                };

                ItemTemplate = _dataTemplateInstance;

                _itemsPanelTemplateInstance ??= new ItemsPanelTemplate
                {
                    Content = _mapCanvas = new Canvas()
                };

                ItemsPanel = _itemsPanelTemplateInstance;

                if (_styleInstance == null)
                {
                    _styleInstance = new Style();
                    {
                        _styleInstance.Setters.Add(new Setter(Canvas.LeftProperty, new Binding("LocalPositionX")));
                        _styleInstance.Setters.Add(new Setter(Canvas.TopProperty, new Binding("LocalPositionY")));
                        _styleInstance.Setters.Add(new Setter(Panel.ZIndexProperty, new Binding("ZIndex")));
                    }
                }

                if (!Styles.Contains(_styleInstance))
                {
                    Styles.Add(_styleInstance);
                }

                #endregion -- templates --

                Markers = new ObservableCollection<GMapMarker>();

                ClipToBounds = true;

                //TODO: Check if exists in Avalonia
                //SnapsToDevicePixels = true;

                _core.SystemType = "WindowsPresentation";

                _core.RenderMode = RenderMode.WPF;

                _core.OnMapZoomChanged += ForceUpdateOverlays;
                _core.OnCurrentPositionChanged += CoreOnCurrentPositionChanged;
                //Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
                //SizeChanged += GMapControl_SizeChanged;

                // by default its internal property, feel free to use your own
                if (Items == null)
                    Items = Markers;

                //TODO: find default value here
                //_core.Zoom = (int)(double)ZoomProperty.DefaultMetadata.DefaultValue;
                _core.Zoom = 10;
            }
        }

        private void CoreOnCurrentPositionChanged(PointLatLng pointLatLng)
        {
            Position = pointLatLng;
        }

        static GMapControl()
        {
            GMapImageProxy.Enable();
            GMaps.Instance.SQLitePing();
        }

        private void InvalidatorEngage(object sender, ProgressChangedEventArgs e)
        {
            base.InvalidateVisual();
        }

        /// <summary>
        ///     enque built-in thread safe invalidation
        /// </summary>
        public new void InvalidateVisual()
        {
            if (_core.Refresh != null)
                _core.Refresh.Set();
        }

        /// <summary>
        ///     Invalidates the rendering of the element, and forces a complete new layout
        ///     pass. System.Windows.UIElement.OnRender(System.Windows.Media.DrawingContext)
        ///     is called after the layout cycle is completed. If not forced enques built-in thread safe invalidation
        /// </summary>
        /// <param name="forced"></param>
        public void InvalidateVisual(bool forced)
        {
            if (forced)
            {
                lock (_core.InvalidationLock)
                {
                    _core.LastInvalidation = DateTime.Now;
                }

                base.InvalidateVisual();
            }
            else
            {
                InvalidateVisual();
            }
        }

        protected override void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.ItemsCollectionChanged(sender, e);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ForceUpdateOverlays(e.NewItems);
            }
            else
            {
                InvalidateVisual();
            }
        }

        public void InitializeForBackgroundRendering(int width, int height)
        {
            Width = width;
            Height = height;

            if (!_core.IsStarted)
            {
                if (_lazyEvents)
                {
                    _lazyEvents = false;

                    if (_lazySetZoomToFitRect.HasValue)
                    {
                        SetZoomToFitRect(_lazySetZoomToFitRect.Value);
                        _lazySetZoomToFitRect = null;
                    }
                }

                _core.OnMapOpen();
                ForceUpdateOverlays();
            }

            _core.OnMapSizeChanged(width, height);

            if (_core.IsStarted)
            {
                if (IsRotated)
                {
                    UpdateRotationMatrix();
                }

                ForceUpdateOverlays();
            }

            base.InvalidateArrange();
        }

        /// <summary>
        ///     inits core system
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (!_core.IsStarted)
            {
                if (_lazyEvents)
                {
                    _lazyEvents = false;

                    if (_lazySetZoomToFitRect.HasValue)
                    {
                        SetZoomToFitRect(_lazySetZoomToFitRect.Value);
                        _lazySetZoomToFitRect = null;
                    }
                }

                _core.OnMapOpen().ProgressChanged += InvalidatorEngage;
                ForceUpdateOverlays();
            }
        }

        /// <summary>
        ///     recalculates size
        /// </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            _core.OnMapSizeChanged((int)availableSize.Width, (int)availableSize.Height);

            if (_core.IsStarted)
            {
                if (IsRotated)
                {
                    UpdateRotationMatrix();
                }

                ForceUpdateOverlays();
            }

            return base.MeasureOverride(availableSize);
        }

        private void ForceUpdateOverlays()
        {
            ForceUpdateOverlays(Items);
        }

        /// <summary>
        ///     regenerates shape of route
        /// </summary>
        public virtual void RegenerateShape(IShapable s)
        {
            var marker = s as GMapMarker;
            if (marker == null)
                throw new NotSupportedException("shape not supported");

            if (s.Points != null && s.Points.Count > 1)
            {
                marker.Position = s.Points[0];
                var localPath = new List<Point>(s.Points.Count);
                var offset = FromLatLngToLocal(s.Points[0]);

                foreach (var i in s.Points)
                {
                    var point = FromLatLngToLocal(i);
                    localPath.Add(new Point(point.X - offset.X, point.Y - offset.Y));
                }

                var shape = s.CreatePath(localPath, true);

                if (marker.Shape is Path p)
                {
                    p.Data = shape.Data;
                }
                else
                {
                    marker.Shape = shape;
                }
            }
            else
            {
                marker.Shape = null;
            }
        }

        private void ForceUpdateOverlays(System.Collections.IEnumerable items)
        {
            //TODO: disable
            //using (Dispatcher.DisableProcessing())
            //{
            UpdateMarkersOffset();

            foreach (GMapMarker? i in items)
            {
                if (i != null)
                {
                    i.ForceUpdateLocalPosition(this);

                    if (i is IShapable s)
                        RegenerateShape(s);
                }
            }
            //}

            InvalidateVisual();
        }

        /// <summary>
        ///     updates markers overlay offset
        /// </summary>
        private void UpdateMarkersOffset()
        {
            if (MapCanvas != null)
            {
                if (MapScaleTransform != null)
                {
                    var tp = MapScaleTransform.Transform(
                        new Point(_core.RenderOffset.X, _core.RenderOffset.Y));
                    MapOverlayTranslateTransform.X = tp.X;
                    MapOverlayTranslateTransform.Y = tp.Y;

                    // map is scaled already
                    MapTranslateTransform.X = _core.RenderOffset.X;
                    MapTranslateTransform.Y = _core.RenderOffset.Y;
                }
                else
                {
                    MapTranslateTransform.X = _core.RenderOffset.X;
                    MapTranslateTransform.Y = _core.RenderOffset.Y;

                    MapOverlayTranslateTransform.X = MapTranslateTransform.X;
                    MapOverlayTranslateTransform.Y = MapTranslateTransform.Y;
                }
            }
        }

        public ISolidColorBrush EmptyMapBackground { get; } = Brushes.WhiteSmoke;

        /// <summary>
        ///     render map in WPF
        /// </summary>
        /// <param name="g"></param>
        private void DrawMap(DrawingContext g)
        {
            if (MapProvider == EmptyProvider.Instance || MapProvider == null)
            {
                return;
            }

            _core.TileDrawingListLock.AcquireReaderLock();
            _core.Matrix.EnterReadLock();

            try
            {
                foreach (var tilePoint in _core.TileDrawingList)
                {
                    _core.TileRect.Location = tilePoint.PosPixel;
                    _core.TileRect.OffsetNegative(_core.CompensationOffset);

                    //if(region.IntersectsWith(Core.tileRect) || IsRotated)
                    {
                        bool found = false;

                        var t = _core.Matrix.GetTileWithNoLock(_core.Zoom, tilePoint.PosXY);

                        if (t.NotEmpty)
                        {
                            foreach (GMapImage img in t.Overlays)
                            {
                                if (img != null && img.Img != null)
                                {
                                    if (!found)
                                        found = true;

                                    var imgRect = new Rect(_core.TileRect.X + 0.6,
                                        _core.TileRect.Y + 0.6,
                                        _core.TileRect.Width + 0.6,
                                        _core.TileRect.Height + 0.6);

                                    if (!img.IsParent)
                                    {
                                        g.DrawImage(img.Img, imgRect);
                                    }
                                    else
                                    {
                                        // TODO: move calculations to loader thread
                                        var geometry = new RectangleGeometry(imgRect);
                                        var parentImgRect =
                                            new Rect(_core.TileRect.X - _core.TileRect.Width * img.Xoff + 0.6,
                                                _core.TileRect.Y - _core.TileRect.Height * img.Yoff + 0.6,
                                                _core.TileRect.Width * img.Ix + 0.6,
                                                _core.TileRect.Height * img.Ix + 0.6);

                                        using (g.PushClip(geometry.Rect))
                                        {
                                            g.DrawImage(img.Img, parentImgRect);
                                        }
                                        geometry = null;
                                    }
                                }
                            }
                        }
                        else if (FillEmptyTiles && MapProvider.Projection is MercatorProjection)
                        {
                            #region -- fill empty tiles --

                            int zoomOffset = 1;
                            var parentTile = Tile.Empty;
                            long ix = 0;

                            while (!parentTile.NotEmpty && zoomOffset < _core.Zoom && zoomOffset <= LevelsKeepInMemory)
                            {
                                ix = (long)Math.Pow(2, zoomOffset);
                                parentTile = _core.Matrix.GetTileWithNoLock(_core.Zoom - zoomOffset++,
                                    new GPoint((int)(tilePoint.PosXY.X / ix), (int)(tilePoint.PosXY.Y / ix)));
                            }

                            if (parentTile.NotEmpty)
                            {
                                long xOff = Math.Abs(tilePoint.PosXY.X - parentTile.Pos.X * ix);
                                long yOff = Math.Abs(tilePoint.PosXY.Y - parentTile.Pos.Y * ix);

                                var geometry =
                                    new RectangleGeometry(new Rect(_core.TileRect.X + 0.6,
                                        _core.TileRect.Y + 0.6,
                                        _core.TileRect.Width + 0.6,
                                        _core.TileRect.Height + 0.6));
                                var parentImgRect = new Rect(_core.TileRect.X - _core.TileRect.Width * xOff + 0.6,
                                    _core.TileRect.Y - _core.TileRect.Height * yOff + 0.6,
                                    _core.TileRect.Width * ix + 0.6,
                                    _core.TileRect.Height * ix + 0.6);

                                // render tile
                                foreach (GMapImage img in parentTile.Overlays)
                                {
                                    if (img != null && img.Img != null && !img.IsParent)
                                    {
                                        if (!found)
                                            found = true;

                                        using (g.PushClip(geometry.Rect))
                                        {
                                            g.DrawImage(img.Img, parentImgRect);
                                            g.DrawRectangle(SelectedAreaFill, null, geometry.Bounds);
                                        }
                                    }
                                }
                            }

                            #endregion -- fill empty tiles --
                        }

                        // add text if tile is missing
                        if (!found)
                        {
                            lock (_core.FailedLoads)
                            {
                                var lt = new LoadTask(tilePoint.PosXY, _core.Zoom);

                                if (_core.FailedLoads.ContainsKey(lt))
                                {
                                    g.DrawRectangle(EmptyTileBrush,
                                        EmptyTileBorders,
                                        new Rect(_core.TileRect.X,
                                            _core.TileRect.Y,
                                            _core.TileRect.Width,
                                            _core.TileRect.Height));

                                    var ex = _core.FailedLoads[lt];

                                    var tileText = GetFormattedText("Exception: " + ex.Message, 14);

                                    //TODO: Avalonia
                                    //tileText.MaxTextWidth = _core.TileRect.Width - 11;

                                    g.DrawText(
                                        Brushes.Black,
                                        new Point(_core.TileRect.X + 11, _core.TileRect.Y + 11),
                                        tileText);

                                    g.DrawText(
                                        Brushes.Black,
                                        new Point(
                                            _core.TileRect.X + _core.TileRect.Width / 2 - EmptyTileText.Bounds.Width / 2,
                                            _core.TileRect.Y + _core.TileRect.Height / 2 - EmptyTileText.Bounds.Height / 2),
                                        EmptyTileText);
                                }
                            }
                        }

                        if (ShowTileGridLines)
                        {
                            g.DrawRectangle(null,
                                EmptyTileBorders,
                                new Rect(_core.TileRect.X,
                                    _core.TileRect.Y,
                                    _core.TileRect.Width,
                                    _core.TileRect.Height));

                            if (tilePoint.PosXY == _core.CenterTileXYLocation)
                            {
                                var tileText = GetFormattedText("CENTER:" + tilePoint.ToString(), 16);
                                //tileText.MaxTextWidth = _core.TileRect.Width;
                                g.DrawText(
                                    Brushes.Black,
                                    new Point(
                                        _core.TileRect.X + _core.TileRect.Width / 2 - EmptyTileText.Bounds.Width / 2,
                                        _core.TileRect.Y + _core.TileRect.Height / 2 - tileText.Bounds.Height / 2),
                                    tileText);
                            }
                            else
                            {
                                var tileText = GetFormattedText("TILE: " + tilePoint.ToString(), 16);
                                //tileText.MaxTextWidth = _core.TileRect.Width;
                                g.DrawText(
                                    Brushes.Black,
                                    new Point(
                                        _core.TileRect.X + _core.TileRect.Width / 2 - EmptyTileText.Bounds.Width / 2,
                                        _core.TileRect.Y + _core.TileRect.Height / 2 - tileText.Bounds.Height / 2),
                                    tileText);
                            }
                        }
                    }
                }
            }
            finally
            {
                _core.Matrix.LeaveReadLock();
                _core.TileDrawingListLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        ///     gets image of the current view
        /// </summary>
        /// <returns></returns>
        public Bitmap ToImageSource()
        {
            var obj = this;

            // Save current canvas transform
            //TODO: Avalonia
            //var transform = obj.LayoutTransform;
            //obj.LayoutTransform = null;

            // fix margin offset as well
            var margin = obj.Margin;
            obj.Margin = new Thickness(0,
                0,
                margin.Right - margin.Left,
                margin.Bottom - margin.Top);

            // Get the size of canvas
            var size = new Size(obj.Bounds.Width, obj.Bounds.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            var bmp = new RenderTargetBitmap(
                new PixelSize((int)size.Width, (int)size.Height),
                new Vector(96, 96));

            bmp.Render(obj);

            //if (bmp.CanFreeze)
            //{
            //    bmp.Freeze();
            //}

            // return values as they were before
            //TODO: Avalonia
            //obj.LayoutTransform = transform;
            //obj.Margin = margin;

            return bmp;
        }

        /// <summary>
        ///     sets zoom to max to fit rect
        /// </summary>
        /// <param name="rect">area</param>
        /// <returns></returns>
        public bool SetZoomToFitRect(RectLatLng rect)
        {
            if (_lazyEvents)
            {
                _lazySetZoomToFitRect = rect;
            }
            else
            {
                int maxZoom = _core.GetMaxZoomToFitRect(rect);
                if (maxZoom > 0)
                {
                    var center =
                        new PointLatLng(rect.Lat - rect.HeightLat / 2, rect.Lng + rect.WidthLng / 2);
                    Position = center;

                    if (maxZoom > MaxZoom)
                    {
                        maxZoom = MaxZoom;
                    }

                    if (_core.Zoom != maxZoom)
                    {
                        Zoom = maxZoom;
                    }

                    return true;
                }
            }

            return false;
        }

        private RectLatLng? _lazySetZoomToFitRect;
        private bool _lazyEvents = true;

        /// <summary>
        ///     sets to max zoom to fit all markers and centers them in map
        /// </summary>
        /// <param name="zIndex">z index or null to check all</param>
        /// <returns></returns>
        public bool ZoomAndCenterMarkers(int? zIndex)
        {
            var rect = GetRectOfAllMarkers(zIndex);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        /// <summary>
        ///     gets rectangle with all objects inside
        /// </summary>
        /// <param name="zIndex">z index or null to check all</param>
        /// <returns></returns>
        public RectLatLng? GetRectOfAllMarkers(int? zIndex)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;
            IEnumerable<GMapMarker> overlays;

            overlays = zIndex.HasValue
                ? Items.Cast<GMapMarker>().Where(p => p != null && p.ZIndex == zIndex)
                : Items.Cast<GMapMarker>();

            if (overlays != null)
            {
                foreach (var m in overlays)
                {
                    if (m.Shape != null && m.Shape.IsVisible)
                    {
                        // left
                        if (m.Position.Lng < left)
                        {
                            left = m.Position.Lng;
                        }

                        // top
                        if (m.Position.Lat > top)
                        {
                            top = m.Position.Lat;
                        }

                        // right
                        if (m.Position.Lng > right)
                        {
                            right = m.Position.Lng;
                        }

                        // bottom
                        if (m.Position.Lat < bottom)
                        {
                            bottom = m.Position.Lat;
                        }
                    }
                }
            }

            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue &&
                bottom != double.MaxValue)
            {
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }

            return ret;
        }

        /// <summary>
        ///     offset position in pixels
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Offset(int x, int y)
        {
            if (IsInitialized)
            {
                if (IsRotated)
                {
                    var p = new Point(x, y);
                    p = _rotationMatrixInvert.Transform(p);
                    x = (int)p.X;
                    y = (int)p.Y;

                    _core.DragOffset(new GPoint(x, y));

                    ForceUpdateOverlays();
                }
                else
                {
                    _core.DragOffset(new GPoint(x, y));

                    UpdateMarkersOffset();
                    InvalidateVisual(true);
                }
            }
        }

        private readonly RotateTransform _rotationMatrix = new RotateTransform();
        private MatrixTransform _rotationMatrixInvert = new MatrixTransform();

        /// <summary>
        ///     updates rotation matrix
        /// </summary>
        private void UpdateRotationMatrix()
        {
            var center = new Point(Bounds.Width / 2.0, Bounds.Height / 2.0);

            _rotationMatrix.Angle = -Bearing;
            //TODO: Avalonia not support Center X, Y
            //_rotationMatrix.CenterY = center.Y;
            //_rotationMatrix.CenterX = center.X;

            _rotationMatrixInvert = _rotationMatrix.Inverse();
        }

        /// <summary>
        ///     returs true if map bearing is not zero
        /// </summary>
        public bool IsRotated
        {
            get { return _core.IsRotated; }
        }

        /// <summary>
        ///     bearing for rotation of the map
        /// </summary>
        [Category("GMap.NET")]
        public float Bearing
        {
            get { return _core.Bearing; }
            set
            {
                if (_core.Bearing != value)
                {
                    bool resize = _core.Bearing == 0;
                    _core.Bearing = value;

                    UpdateRotationMatrix();

                    if (value != 0 && value % 360 != 0)
                    {
                        _core.IsRotated = true;

                        if (_core.TileRectBearing.Size == _core.TileRect.Size)
                        {
                            _core.TileRectBearing = _core.TileRect;
                            _core.TileRectBearing.Inflate(1, 1);
                        }
                    }
                    else
                    {
                        _core.IsRotated = false;
                        _core.TileRectBearing = _core.TileRect;
                    }

                    if (resize)
                    {
                        _core.OnMapSizeChanged((int)Bounds.Width, (int)Bounds.Height);
                    }

                    if (_core.IsStarted)
                    {
                        ForceUpdateOverlays();
                    }
                }
            }
        }

        /// <summary>
        ///     apply transformation if in rotation mode
        /// </summary>
        private Point ApplyRotation(double x, double y)
        {
            var ret = new Point(x, y);

            if (IsRotated)
            {
                ret = _rotationMatrix.Transform(ret);
            }

            return ret;
        }

        /// <summary>
        ///     apply transformation if in rotation mode
        /// </summary>
        private Point ApplyRotationInversion(double x, double y)
        {
            var ret = new Point(x, y);

            if (IsRotated)
            {
                ret = _rotationMatrixInvert.Transform(ret);
            }

            return ret;
        }

        private static FormattedText GetFormattedText(string text, int size)
        {
            return new FormattedText(
                text,
                new Typeface("GenericSansSerif"),
                size,
                TextAlignment.Left,
                TextWrapping.Wrap,
                new Size());
        }

        #region UserControl Events

        public override void Render(DrawingContext drawingContext)
        {
            if (!_core.IsStarted)
                return;

            drawingContext.DrawRectangle(EmptyMapBackground, null, Bounds);

            if (IsRotated)
            {
                using (drawingContext.PushPreTransform(_rotationMatrix.Value))
                {
                    if (MapScaleTransform != null)
                    {
                        using (drawingContext.PushPreTransform(MapScaleTransform.Value))
                        using (drawingContext.PushPreTransform(MapTranslateTransform.Value))
                            DrawMap(drawingContext);
                    }
                    else
                    {
                        using var _ = drawingContext.PushPreTransform(MapTranslateTransform.Value);
                        DrawMap(drawingContext);
                    }
                }
            }
            else
            {
                if (MapScaleTransform != null)
                {
                    using (drawingContext.PushPreTransform(MapScaleTransform.Value))
                    using (drawingContext.PushPreTransform(MapTranslateTransform.Value))
                    {
                        DrawMap(drawingContext);

#if DEBUG
                        drawingContext.DrawLine(_virtualCenterCrossPen, new Point(-20, 0), new Point(20, 0));
                        drawingContext.DrawLine(_virtualCenterCrossPen, new Point(0, -20), new Point(0, 20));
#endif
                    }
                }
                else
                {
                    using var _ = drawingContext.PushPreTransform(MapTranslateTransform.Value);
                    DrawMap(drawingContext);
#if DEBUG
                    drawingContext.DrawLine(_virtualCenterCrossPen, new Point(-20, 0), new Point(20, 0));
                    drawingContext.DrawLine(_virtualCenterCrossPen, new Point(0, -20), new Point(0, 20));
#endif
                }
            }

            // selection
            if (!SelectedArea.IsEmpty)
            {
                var p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
                var p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

                long x1 = p1.X;
                long y1 = p1.Y;
                long x2 = p2.X;
                long y2 = p2.Y;

                if (SelectionUseCircle)
                {
                    drawingContext.DrawGeometry(SelectedAreaFill,
                        SelectionPen,
                        new EllipseGeometry(
                            new Rect(
                                x1 + (x2 - x1) / 2,
                                y1 + (y2 - y1) / 2,
                                (x2 - x1) / 2,
                                (y2 - y1) / 2)));
                }
                else
                {
                    //TODO: add rounded rectangle
                    drawingContext.DrawRectangle(SelectedAreaFill,
                        SelectionPen,
                        new Rect(x1, y1, x2 - x1, y2 - y1),
                        5,
                        5);
                }
            }

            if (ShowCenter)
            {
                drawingContext.DrawLine(CenterCrossPen,
                    new Point(Bounds.Width / 2 - 5, Bounds.Height / 2),
                    new Point(Bounds.Width / 2 + 5, Bounds.Height / 2));
                drawingContext.DrawLine(CenterCrossPen,
                    new Point(Bounds.Width / 2, Bounds.Height / 2 - 5),
                    new Point(Bounds.Width / 2, Bounds.Height / 2 + 5));
            }

            if (_renderHelperLine)
            {
                var p = _mouse.GetPosition(this);

                drawingContext.DrawLine(HelperLinePen, new Point(p.X, 0), new Point(p.X, Bounds.Height));
                drawingContext.DrawLine(HelperLinePen, new Point(0, p.Y), new Point(Bounds.Width, p.Y));
            }

            #region -- copyright --

            if (_copyright != null)
            {
                drawingContext.DrawText(Brushes.Black, new Point(5, Bounds.Height - _copyright.Bounds.Height - 5), _copyright);
            }

            #endregion -- copyright --

            base.Render(drawingContext);
        }

        public Pen CenterCrossPen { get; } = new Pen(Brushes.Red, 1);
        public bool ShowCenter { get; } = true;

#if DEBUG
        private readonly Pen _virtualCenterCrossPen = new Pen(Brushes.Blue, 2);
#endif

        private HelperLineOptions _helperLineOption = HelperLineOptions.DontShow;

        /// <summary>
        ///     draw lines at the mouse pointer position
        /// </summary>
        [Browsable(false)]
        public HelperLineOptions HelperLineOption
        {
            get { return _helperLineOption; }
            set
            {
                _helperLineOption = value;
                _renderHelperLine = _helperLineOption == HelperLineOptions.ShowAlways;
                if (_core.IsStarted)
                {
                    InvalidateVisual();
                }
            }
        }

        public Pen HelperLinePen { get; } = new Pen(Brushes.Blue, 1);
        private bool _renderHelperLine;

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
            {
                _renderHelperLine = !(e.Key == Key.LeftShift || e.KeyModifiers == KeyModifiers.Alt);
                if (!_renderHelperLine)
                {
                    InvalidateVisual();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
            {
                _renderHelperLine = e.Key == Key.LeftShift || e.KeyModifiers == KeyModifiers.Alt;
                if (_renderHelperLine)
                {
                    InvalidateVisual();
                }
            }
        }

        /// <summary>
        ///     Reverses MouseWheel zooming direction
        /// </summary>
        public static readonly StyledProperty<bool> InvertedMouseWheelZoomingProperty =
          AvaloniaProperty.Register<GMapControl, bool>(
              nameof(InvertedMouseWheelZooming));

        public bool InvertedMouseWheelZooming
        {
            get { return GetValue(InvertedMouseWheelZoomingProperty); }
            set { SetValue(InvertedMouseWheelZoomingProperty, value); }
        }

        /// <summary>
        ///     Lets you zoom by MouseWheel even when pointer is in area of marker
        /// </summary>
        public static readonly StyledProperty<bool> IgnoreMarkerOnMouseWheelProperty =
          AvaloniaProperty.Register<GMapControl, bool>(
              nameof(IgnoreMarkerOnMouseWheel));

        public bool IgnoreMarkerOnMouseWheel
        {
            get { return GetValue(IgnoreMarkerOnMouseWheelProperty); }
            set { SetValue(IgnoreMarkerOnMouseWheelProperty, value); }
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            //TODO: && (IsMouseDirectlyOver || IgnoreMarkerOnMouseWheel)
            if (MouseWheelZoomEnabled && !_core.IsDragging)
            {
                var p = e.GetPosition(this);

                if (MapScaleTransform != null)
                {
                    p = MapScaleTransform.Inverse().Transform(p);
                }

                p = ApplyRotationInversion(p.X, p.Y);

                if (_core.MouseLastZoom.X != (int)p.X && _core.MouseLastZoom.Y != (int)p.Y)
                {
                    if (MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
                    {
                        Position = FromLocalToLatLng((int)p.X, (int)p.Y);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
                    {
                        Position = FromLocalToLatLng((int)Bounds.Width / 2, (int)Bounds.Height / 2);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
                    {
                        Position = FromLocalToLatLng((int)p.X, (int)p.Y);
                    }

                    _core.MouseLastZoom.X = (int)p.X;
                    _core.MouseLastZoom.Y = (int)p.Y;
                }

                // set mouse position to map center
                if (MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
                {
                    var ps = this.PointToScreen(new Point(Bounds.Width / 2, Bounds.Height / 2));
                    Stuff.SetCursorPos(ps.X, ps.Y);
                }

                _core.MouseWheelZooming = true;

                if (e.Delta.Y > 0)
                {
                    if (!InvertedMouseWheelZooming)
                    {
                        Zoom = (int)Zoom + 1;
                    }
                    else
                    {
                        Zoom = (int)(Zoom + 0.99) - 1;
                    }
                }
                else
                {
                    if (InvertedMouseWheelZooming)
                    {
                        Zoom = (int)Zoom + 1;
                    }
                    else
                    {
                        Zoom = (int)(Zoom + 0.99) - 1;
                    }
                }

                _core.MouseWheelZooming = false;
            }
        }

        private bool _isSelected;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (CanDragMap && e.GetCurrentPoint(this).Properties.PointerUpdateKind == DragButton)
            {
                var p = e.GetPosition(this);

                if (MapScaleTransform != null)
                {
                    p = MapScaleTransform.Inverse().Transform(p);
                }

                p = ApplyRotationInversion(p.X, p.Y);

                _core.MouseDown.X = (int)p.X;
                _core.MouseDown.Y = (int)p.Y;

                InvalidateVisual();
            }
            else
            {
                if (!_isSelected)
                {
                    var p = e.GetPosition(this);
                    _isSelected = true;
                    SelectedArea = RectLatLng.Empty;
                    _selectionEnd = PointLatLng.Empty;
                    _selectionStart = FromLocalToLatLng((int)p.X, (int)p.Y);
                }
            }
        }

        private ulong _onMouseUpTimestamp;

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (_isSelected)
            {
                _isSelected = false;
            }

            if (_core.IsDragging)
            {
                if (IsDragging)
                {
                    _onMouseUpTimestamp = e.Timestamp & ulong.MaxValue;
                    IsDragging = false;
                    Debug.WriteLine("IsDragging = " + IsDragging);
                    Cursor = _cursorBefore;
                    _mouse.Capture(null);
                }

                _core.EndDrag();

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    if (_core.LastLocationInBounds.HasValue)
                    {
                        Position = _core.LastLocationInBounds.Value;
                    }
                }
            }
            else
            {
                if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == DragButton)
                {
                    _core.MouseDown = GPoint.Empty;
                }

                if (!_selectionEnd.IsEmpty && !_selectionStart.IsEmpty)
                {
                    bool zoomtofit = false;

                    if (!SelectedArea.IsEmpty && e.KeyModifiers == KeyModifiers.Shift)
                    {
                        zoomtofit = SetZoomToFitRect(SelectedArea);
                    }

                    OnSelectionChange?.Invoke(SelectedArea, zoomtofit);
                }
                else
                {
                    InvalidateVisual();
                }
            }
        }

        private Cursor _cursorBefore = Cursor.Default;

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            // wpf generates to many events if mouse is over some visual
            // and OnMouseUp is fired, wtf, anyway...
            // http://greatmaps.codeplex.com/workitem/16013
            if ((e.Timestamp & UInt32.MaxValue) - _onMouseUpTimestamp < 55)
            {
                Debug.WriteLine("OnMouseMove skipped: " + ((e.Timestamp & Int32.MaxValue) - _onMouseUpTimestamp) + "ms");
                return;
            }

            if (!_core.IsDragging && !_core.MouseDown.IsEmpty)
            {
                var p = e.GetPosition(this);

                if (MapScaleTransform != null)
                {
                    p = MapScaleTransform.Inverse().Transform(p);
                }

                p = ApplyRotationInversion(p.X, p.Y);

                // cursor has moved beyond drag tolerance
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                {
                    if (Math.Abs(p.X - _core.MouseDown.X) * 2 >= MinimumHorizontalDragDistance ||
                        Math.Abs(p.Y - _core.MouseDown.Y) * 2 >= MinimumVerticalDragDistance)
                    {
                        _core.BeginDrag(_core.MouseDown);
                    }
                }
            }

            if (_core.IsDragging)
            {
                if (!IsDragging)
                {
                    IsDragging = true;
                    Debug.WriteLine("IsDragging = " + IsDragging);
                    _cursorBefore = Cursor;
                    Cursor = new Cursor(StandardCursorType.SizeAll);
                    _mouse.Capture(this);
                }

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    // ...
                }
                else
                {
                    var p = e.GetPosition(this);

                    if (MapScaleTransform != null)
                    {
                        p = MapScaleTransform.Inverse().Transform(p);
                    }

                    p = ApplyRotationInversion(p.X, p.Y);

                    _core.MouseCurrent.X = (int)p.X;
                    _core.MouseCurrent.Y = (int)p.Y;
                    {
                        _core.Drag(_core.MouseCurrent);
                    }

                    if (IsRotated || _scaleMode != ScaleModes.Integer)
                    {
                        ForceUpdateOverlays();
                    }
                    else
                    {
                        UpdateMarkersOffset();
                    }
                }

                InvalidateVisual(true);
            }
            else
            {
                if (_isSelected && !_selectionStart.IsEmpty &&
                    (e.KeyModifiers == KeyModifiers.Shift || e.KeyModifiers == KeyModifiers.Alt ||
                     DisableAltForSelection))
                {
                    var p = e.GetPosition(this);
                    _selectionEnd = FromLocalToLatLng((int)p.X, (int)p.Y);
                    {
                        var p1 = _selectionStart;
                        var p2 = _selectionEnd;

                        double x1 = Math.Min(p1.Lng, p2.Lng);
                        double y1 = Math.Max(p1.Lat, p2.Lat);
                        double x2 = Math.Max(p1.Lng, p2.Lng);
                        double y2 = Math.Min(p1.Lat, p2.Lat);

                        SelectedArea = new RectLatLng(y1, x1, x2 - x1, y1 - y2);
                    }
                }

                if (_renderHelperLine)
                {
                    InvalidateVisual(true);
                }
            }
        }

        /// <summary>
        ///     if true, selects area just by holding mouse and moving
        /// </summary>
        public bool DisableAltForSelection { get; } = false;


        #endregion UserControl Events

        #region IGControl Members

        /// <summary>
        ///     Call it to empty tile cache & reload tiles
        /// </summary>
        public void ReloadMap()
        {
            _core.ReloadMap();
        }

#if !NETFRAMEWORK

        public Task ReloadMapAsync()
        {
            return _core.ReloadMapAsync();
        }

#endif

        /// <summary>
        ///     sets position using geocoder
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public GeoCoderStatusCode SetPositionByKeywords(string keys)
        {
            var status = GeoCoderStatusCode.UNKNOWN_ERROR;

            var gp = MapProvider as GeocodingProvider;
            if (gp == null)
            {
                gp = GMapProviders.OpenStreetMap as GeocodingProvider;
            }

            if (gp != null)
            {
                var pt = gp.GetPoint(keys, out status);
                if (status == GeoCoderStatusCode.OK && pt.HasValue)
                {
                    Position = pt.Value;
                }
            }

            return status;
        }

        /// <summary>
        ///     gets position using geocoder
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public PointLatLng GetPositionByKeywords(string keys)
        {
            var status = GeoCoderStatusCode.UNKNOWN_ERROR;

            var gp = MapProvider as GeocodingProvider;
            if (gp == null)
            {
                gp = GMapProviders.OpenStreetMap as GeocodingProvider;
            }

            if (gp != null)
            {
                var pt = gp.GetPoint(keys, out status);
                if (status == GeoCoderStatusCode.OK && pt.HasValue)
                {
                    return pt.Value;
                }
            }

            return new PointLatLng();
        }

        public PointLatLng FromLocalToLatLng(int x, int y)
        {
            if (MapScaleTransform != null)
            {
                var tp = MapScaleTransform.Inverse().Transform(new Point(x, y));
                x = (int)tp.X;
                y = (int)tp.Y;
            }

            if (IsRotated)
            {
                var f = _rotationMatrixInvert.Transform(new Point(x, y));

                x = (int)f.X;
                y = (int)f.Y;
            }

            return _core.FromLocalToLatLng(x, y);
        }

        public GPoint FromLatLngToLocal(PointLatLng point)
        {
            var ret = _core.FromLatLngToLocal(point);

            if (MapScaleTransform != null)
            {
                var tp = MapScaleTransform.Transform(new Point(ret.X, ret.Y));
                ret.X = (int)tp.X;
                ret.Y = (int)tp.Y;
            }

            if (IsRotated)
            {
                var f = _rotationMatrix.Transform(new Point(ret.X, ret.Y));

                ret.X = (int)f.X;
                ret.Y = (int)f.Y;
            }

            return ret;
        }

        #region Save and Load offline database

        //TODO: Native dialog for avalonia

        public bool ShowExportDialog()
        {
            return false;
            //    var dlg = new Microsoft.Win32.SaveFileDialog();
            //    {
            //        dlg.CheckPathExists = true;
            //        dlg.CheckFileExists = false;
            //        dlg.AddExtension = true;
            //        dlg.DefaultExt = "gmdb";
            //        dlg.ValidateNames = true;
            //        dlg.Title = "GMap.NET: Export map to db, if file exsist only new data will be added";
            //        dlg.FileName = "DataExp";
            //        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //        dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            //        dlg.FilterIndex = 1;
            //        dlg.RestoreDirectory = true;

            //        if (dlg.ShowDialog() == true)
            //        {
            //            bool ok = GMaps.Instance.ExportToGMDB(dlg.FileName);
            //            if (ok)
            //            {
            //                MessageBox.Show("Complete!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Information);
            //            }
            //            else
            //            {
            //                MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
            //            }

            //            return ok;
            //        }
            //    }

            //    return false;
        }

        public bool ShowImportDialog()
        {
            return false;
            //    var dlg = new Microsoft.Win32.OpenFileDialog();
            //    {
            //        dlg.CheckPathExists = true;
            //        dlg.CheckFileExists = false;
            //        dlg.AddExtension = true;
            //        dlg.DefaultExt = "gmdb";
            //        dlg.ValidateNames = true;
            //        dlg.Title = "GMap.NET: Import to db, only new data will be added";
            //        dlg.FileName = "DataImport";
            //        dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //        dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
            //        dlg.FilterIndex = 1;
            //        dlg.RestoreDirectory = true;

            //        if (dlg.ShowDialog() == true)
            //        {
            //            Cursor = Cursors.Wait;

            //            bool ok = GMaps.Instance.ImportFromGMDB(dlg.FileName);
            //            if (ok)
            //            {
            //                MessageBox.Show("Complete!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Information);
            //                ReloadMap();
            //            }
            //            else
            //            {
            //                MessageBox.Show("  Failed!", "GMap.NET", MessageBoxButton.OK, MessageBoxImage.Warning);
            //            }

            //            Cursor = Cursors.Arrow;

            //            return ok;
            //        }
            //    }

            //    return false;
        }

        #endregion Save and Load offline database

        public static readonly StyledProperty<PointLatLng> PositionProperty =
            AvaloniaProperty.Register<GMapControl, PointLatLng>(
                nameof(Position));

        /// <summary>
        ///     Current coordinates of the map center
        /// </summary>
        [Browsable(false)]
        public PointLatLng Position
        {
            get { return GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        private void PositionChanged()
        {
            _core.Position = Position;
            if (_core.IsStarted)
            {
                ForceUpdateOverlays();
            }
        }

        [Browsable(false)]
        public GPoint PositionPixel
        {
            get { return _core.PositionPixel; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CacheLocation
        {
            get { return CacheLocator.Location; }
            set { CacheLocator.Location = value; }
        }

        [Browsable(false)] public bool IsDragging { get; private set; }

        [Browsable(false)]
        public RectLatLng ViewArea
        {
            get
            {
                if (!IsRotated)
                {
                    return _core.ViewArea;
                }
                else if (_core.Provider.Projection != null)
                {
                    var p = FromLocalToLatLng(0, 0);
                    var p2 = FromLocalToLatLng((int)Bounds.Width, (int)Bounds.Height);

                    return RectLatLng.FromLTRB(p.Lng, p.Lat, p2.Lng, p2.Lat);
                }

                return RectLatLng.Empty;
            }
        }

        [Category("GMap.NET")]
        public bool CanDragMap
        {
            get { return _core.CanDragMap; }
            set { _core.CanDragMap = value; }
        }

        public RenderMode RenderMode
        {
            get { return RenderMode.WPF; }
        }

        #endregion IGControl Members

        #region IGControl event Members

        public event PositionChanged OnPositionChanged
        {
            add { _core.OnCurrentPositionChanged += value; }
            remove { _core.OnCurrentPositionChanged -= value; }
        }

        public event TileLoadComplete OnTileLoadComplete
        {
            add { _core.OnTileLoadComplete += value; }
            remove { _core.OnTileLoadComplete -= value; }
        }

        public event TileLoadStart OnTileLoadStart
        {
            add { _core.OnTileLoadStart += value; }
            remove { _core.OnTileLoadStart -= value; }
        }

        public event MapDrag OnMapDrag
        {
            add { _core.OnMapDrag += value; }
            remove { _core.OnMapDrag -= value; }
        }

        public event MapZoomChanged OnMapZoomChanged
        {
            add { _core.OnMapZoomChanged += value; }
            remove { _core.OnMapZoomChanged -= value; }
        }

        /// <summary>
        ///     occures on map type changed
        /// </summary>
        public event MapTypeChanged OnMapTypeChanged
        {
            add { _core.OnMapTypeChanged += value; }
            remove { _core.OnMapTypeChanged -= value; }
        }

        /// <summary>
        ///     occurs on empty tile displayed
        /// </summary>
        public event EmptyTileError OnEmptyTileError
        {
            add { _core.OnEmptyTileError += value; }
            remove { _core.OnEmptyTileError -= value; }
        }

        #endregion IGControl event Members

        #region IDisposable Members

        public virtual void Dispose()
        {
            GMaps.Instance.CancelTileCaching();

            if (_core.IsStarted)
            {
                _core.OnMapZoomChanged -= ForceUpdateOverlays;
                _core.OnMapClose();
            }
        }

        #endregion IDisposable Members
    }

    public enum HelperLineOptions
    {
        DontShow = 0,
        ShowAlways = 1,
        ShowOnModifierKey = 2
    }

    public enum ScaleModes
    {
        /// <summary>
        ///     no scaling
        /// </summary>
        Integer,

        /// <summary>
        ///     scales to fractional level using a stretched tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        ScaleUp,

        /// <summary>
        ///     scales to fractional level using a narrowed tiles, any issues -> http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        ScaleDown,

        /// <summary>
        ///     scales to fractional level using a combination both stretched and narrowed tiles, any issues ->
        ///     http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        Dynamic
    }

    /// <summary>
    ///     Enum ZoomMode
    /// </summary>
    public enum ZoomMode
    {
        /// <summary>
        ///     Only update X coordinates
        /// </summary>
        X,

        /// <summary>
        ///     Only update Y coordinates
        /// </summary>
        Y,

        /// <summary>
        ///     Updates both the X and Y coordinates
        /// </summary>
        XY
    }

    public delegate void SelectionChange(RectLatLng selection, bool zoomToFit);
}
