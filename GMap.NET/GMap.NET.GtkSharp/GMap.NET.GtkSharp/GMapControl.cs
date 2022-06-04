using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using GMap.NET;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.Projections;

namespace GMap.NET.GtkSharp
{


    /// <summary>
    /// GMap.NET control for Windows Forms
    /// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public partial class GMapControl :  Gtk.DrawingArea, Interface
    {
        /// <summary>
        /// occurs when clicked on marker
        /// </summary>
        //public event MarkerClick OnMarkerClick;

        /// <summary>
        /// occurs when clicked on polygon
        /// </summary>
        //public event PolygonClick OnPolygonClick;

        /// <summary>
        /// occurs when clicked on route
        /// </summary>
        //public event RouteClick OnRouteClick;

        /// <summary>
        /// occurs on mouse enters route area
        /// </summary>
        public event RouteEnter OnRouteEnter;

        /// <summary>
        /// occurs on mouse leaves route area
        /// </summary>
        public event RouteLeave OnRouteLeave;

        /// <summary>
        /// occurs when mouse selection is changed
        /// </summary>        
        public event SelectionChange OnSelectionChange;

        /// <summary>
        /// occurs on mouse enters marker area
        /// </summary>
        public event MarkerEnter OnMarkerEnter;

        /// <summary>
        /// occurs on mouse leaves marker area
        /// </summary>
        public event MarkerLeave OnMarkerLeave;

        /// <summary>
        /// occurs on mouse enters Polygon area
        /// </summary>
        public event PolygonEnter OnPolygonEnter;

        /// <summary>
        /// occurs on mouse leaves Polygon area
        /// </summary>
        public event PolygonLeave OnPolygonLeave;

        /// <summary>
        /// list of overlays, should be thread safe
        /// </summary>
        public readonly ObservableCollectionThreadSafe<GMapOverlay> Overlays = new ObservableCollectionThreadSafe<GMapOverlay>();

        /// <summary>
        /// max zoom
        /// </summary>         
        [Category("GMap.NET")]
        [Description("maximum zoom level of map")]
        public int MaxZoom
        {
            get
            {
                return Core.MaxZoom;
            }
            set
            {
                Core.MaxZoom = value;
            }
        }

        /// <summary>
        /// min zoom
        /// </summary>      
        [Category("GMap.NET")]
        [Description("minimum zoom level of map")]
        public int MinZoom
        {
            get
            {
                return Core.MinZoom;
            }
            set
            {
                Core.MinZoom = value;
            }
        }

        /// <summary>
        /// map zooming type for mouse wheel
        /// </summary>
        [Category("GMap.NET")]
        [Description("map zooming type for mouse wheel")]
        public MouseWheelZoomType MouseWheelZoomType
        {
            get
            {
                return Core.MouseWheelZoomType;
            }
            set
            {
                Core.MouseWheelZoomType = value;
            }
        }

        /// <summary>
        /// enable map zoom on mouse wheel
        /// </summary>
        [Category("GMap.NET")]
        [Description("enable map zoom on mouse wheel")]
        public bool MouseWheelZoomEnabled
        {
            get
            {
                return Core.MouseWheelZoomEnabled;
            }
            set
            {
                Core.MouseWheelZoomEnabled = value;
            }
        }

        /// <summary>
        /// text on empty tiles
        /// </summary>
        public string EmptyTileText = "We are sorry, but we don't\nhave imagery at this zoom\nlevel for this region.";

        /// <summary>
        /// pen for empty tile borders
        /// </summary>
        public Pen EmptyTileBorders = new Pen(Brushes.White, 1);

        public bool ShowCenter = true;

        /// <summary>
        /// pen for scale info
        /// </summary>
        public Pen ScalePen = new Pen(Brushes.Blue, 1);
        public Pen CenterPen = new Pen(Brushes.Red, 1);

        /// <summary>
        /// area selection pen
        /// </summary>
        public Pen SelectionPen = new Pen(Brushes.Blue, 2);

        Brush SelectedAreaFill = new SolidBrush(Color.FromArgb(33, Color.RoyalBlue));
        Color selectedAreaFillColor = Color.FromArgb(33, Color.RoyalBlue);

        /// <summary>
        /// background of selected area
        /// </summary>
        [Category("GMap.NET")]
        [Description("background color od the selected area")]
        public Color SelectedAreaFillColor
        {
            get
            {
                return selectedAreaFillColor;
            }
            set
            {
                if (selectedAreaFillColor != value)
                {
                    selectedAreaFillColor = value;

                    if (SelectedAreaFill != null)
                    {
                        SelectedAreaFill.Dispose();
                        SelectedAreaFill = null;
                    }
                    SelectedAreaFill = new SolidBrush(selectedAreaFillColor);
                }
            }
        }

        HelperLineOptions helperLineOption = HelperLineOptions.DontShow;

        /// <summary>
        /// draw lines at the mouse pointer position
        /// </summary>
        [Browsable(false)]
        public HelperLineOptions HelperLineOption
        {
            get
            {
                return helperLineOption;
            }
            set
            {
                helperLineOption = value;
                renderHelperLine = (helperLineOption == HelperLineOptions.ShowAlways);
                if (Core.IsStarted)
                {
                    Invalidate();
                }
            }
        }

        public Pen HelperLinePen = new Pen(Color.Blue, 1);
        bool renderHelperLine = false;

		protected override bool OnKeyPressEvent(Gdk.EventKey e)
        {
            if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
            {
				renderHelperLine = (e.State.HasFlag(Gdk.ModifierType.ShiftMask) || e.State.HasFlag(Gdk.ModifierType.Mod1Mask)); // Mod1Mask == Altkey
                if (renderHelperLine)
                {
                    Invalidate();
                }
            }

			return base.OnKeyPressEvent(e);
        }

		protected override bool OnKeyReleaseEvent(Gdk.EventKey e)
        {
			if (HelperLineOption == HelperLineOptions.ShowOnModifierKey)
            {
				renderHelperLine = (e.State.HasFlag(Gdk.ModifierType.ShiftMask) || e.State.HasFlag(Gdk.ModifierType.Mod1Mask)); // Mod1Mask == Altkey
                if (!renderHelperLine)
                {
                    Invalidate();
                }
            }
			return base.OnKeyReleaseEvent(e);
        }

        Brush EmptytileBrush = new SolidBrush(Color.Navy);
        Color emptyTileColor = Color.Navy;

        /// <summary>
        /// color of empty tile background
        /// </summary>
        [Category("GMap.NET")]
        [Description("background color of the empty tile")]
        public Color EmptyTileColor
        {
            get
            {
                return emptyTileColor;
            }
            set
            {
                if (emptyTileColor != value)
                {
                    emptyTileColor = value;

                    if (EmptytileBrush != null)
                    {
                        EmptytileBrush.Dispose();
                        EmptytileBrush = null;
                    }
                    EmptytileBrush = new SolidBrush(emptyTileColor);
                }
            }
        }

        /// <summary>
        /// show map scale info
        /// </summary>
        public bool MapScaleInfoEnabled = false;

        /// <summary>
        /// enables filling empty tiles using lower level images
        /// </summary>
        public bool FillEmptyTiles = true;

        /// <summary>
        /// if true, selects area just by holding mouse and moving
        /// </summary>
        public bool DisableAltForSelection = false;

        /// <summary>
        /// retry count to get tile 
        /// </summary>
        [Browsable(false)]
        public int RetryLoadTile
        {
            get
            {
                return Core.RetryLoadTile;
            }
            set
            {
                Core.RetryLoadTile = value;
            }
        }

        /// <summary>
        /// how many levels of tiles are staying decompresed in memory
        /// </summary>
        [Browsable(false)]
        public int LevelsKeepInMemmory
        {
            get
            {
                return Core.LevelsKeepInMemory;
            }

            set
            {
                Core.LevelsKeepInMemory = value;
            }
        }

        /// <summary>
        /// map dragg button
        /// </summary>
        [Category("GMap.NET")]
		public uint DragButton = 3; //Right button

        private bool showTileGridLines = false;

        /// <summary>
        /// shows tile gridlines
        /// </summary>
        [Category("GMap.NET")]
        [Description("shows tile gridlines")]
        public bool ShowTileGridLines
        {
            get
            {
                return showTileGridLines;
            }
            set
            {
                showTileGridLines = value;
                Invalidate();
            }
        }

        /// <summary>
        /// current selected area in map
        /// </summary>
        private RectLatLng selectedArea;

        [Browsable(false)]
        public RectLatLng SelectedArea
        {
            get
            {
                return selectedArea;
            }
            set
            {
                selectedArea = value;

                if (Core.IsStarted)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// map boundaries
        /// </summary>
        public RectLatLng? BoundsOfMap = null;

        /// <summary>
        /// enables integrated DoubleBuffer for running on windows mobile
        /// </summary>
		//FIXME Убрать полностью мобильную версию.
        public bool ForceDoubleBuffer = false;
        readonly bool MobileMode = false;

        /// <summary>
        /// stops immediate marker/route/polygon invalidations;
        /// call Refresh to perform single refresh and reset invalidation state
        /// </summary>
        public bool HoldInvalidation = false;

        /// <summary>
        /// call this to stop HoldInvalidation and perform single forced instant refresh 
        /// </summary>
        public void Refresh()
        {
            HoldInvalidation = false;

            lock (Core.InvalidationLock)
            {
                Core.LastInvalidation = DateTime.Now;
            }

			#if DEBUG
			Console.WriteLine(String.Format("Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
			#endif
			base.QueueDraw();
        }

        /// <summary>
        /// enque built-in thread safe invalidation
        /// </summary>
        public void Invalidate()
        {
			if (Core.Refresh != null)
            {
                Core.Refresh.Set();
            }
        }
			
        private bool _GrayScale = false;

        [Category("GMap.NET")]
        public bool GrayScaleMode
        {
            get
            {
                return _GrayScale;
            }
            set
            {
                _GrayScale = value;
                ColorMatrix = (value == true ? ColorMatrixs.GrayScale : null);
            }
        }

        private bool _Negative = false;

        [Category("GMap.NET")]
        public bool NegativeMode
        {
            get
            {
                return _Negative;
            }
            set
            {
                _Negative = value;
                ColorMatrix = (value == true ? ColorMatrixs.Negative : null);
            }
        }

        ColorMatrix colorMatrix;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ColorMatrix ColorMatrix
        {
            get
            {
                return colorMatrix;
            }
            set
            {
                colorMatrix = value;
                if (GMapProvider.TileImageProxy != null && GMapProvider.TileImageProxy is GMapImageProxy)
                {
                    (GMapProvider.TileImageProxy as GMapImageProxy).ColorMatrix = value;
                    if (Core.IsStarted)
                    {
                        ReloadMap();
                    }
                }
            }
        }

		public GPoint RenderOffset{
			get{
				return Core.RenderOffset;
			}
		}

        // internal stuff
        internal readonly Core Core = new Core();

        internal readonly Font CopyrightFont = new Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular);
        internal readonly Font MissingDataFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
        Font ScaleFont = new Font(FontFamily.GenericSansSerif, 5, FontStyle.Italic);
        internal readonly StringFormat CenterFormat = new StringFormat();
        internal readonly StringFormat BottomFormat = new StringFormat();
        readonly ImageAttributes TileFlipXYAttributes = new ImageAttributes();
        double zoomReal;
        Bitmap backBuffer;
        Graphics gxOff;

		#if DEBUG
		public Thread GuiThread; //Only for Debug
		#endif

#if !DESIGN
        /// <summary>
        /// construct
        /// </summary>
        public GMapControl()
        {
		#if DEBUG
			GuiThread = Thread.CurrentThread;
			Console.WriteLine(String.Format("Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
		#endif
			if (!IsDesignerHosted)
            {
				AddEvents ((int) Gdk.EventMask.ButtonPressMask);
				AddEvents ((int) Gdk.EventMask.ButtonReleaseMask);
				AddEvents ((int) Gdk.EventMask.PointerMotionMask);
				AddEvents ((int) Gdk.EventMask.ScrollMask);

				//this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                //this.SetStyle(ControlStyles.UserPaint, true);
                //this.SetStyle(ControlStyles.Opaque, true);
                //ResizeRedraw = true;

                TileFlipXYAttributes.SetWrapMode(WrapMode.TileFlipXY);

                // only one mode will be active, to get mixed mode create new ColorMatrix
                GrayScaleMode = GrayScaleMode;
                NegativeMode = NegativeMode;

                Core.SystemType = "WindowsForms";

                RenderMode = RenderMode.GDI_PLUS;

                CenterFormat.Alignment = StringAlignment.Center;
                CenterFormat.LineAlignment = StringAlignment.Center;

                BottomFormat.Alignment = StringAlignment.Center;

                BottomFormat.LineAlignment = StringAlignment.Far;

                if (GMaps.Instance.IsRunningOnMono)
                {
                    // no imports to move pointer
                    MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
                }

                Overlays.CollectionChanged += new NotifyCollectionChangedEventHandler(Overlays_CollectionChanged);
            }
        }

#endif

        static GMapControl()
        {
       		GMapImageProxy.Enable();
        	GMaps.Instance.SQLitePing();
        }

        void Overlays_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
			if (e.NewItems != null)
            {
                foreach (GMapOverlay obj in e.NewItems)
                {
                    if (obj != null)
                    {
                        obj.Control = this;
                    }
                }

                if (Core.IsStarted && !HoldInvalidation)
                {
                    Invalidate();
                }
            }
        }

        void invalidatorEngage(object sender, ProgressChangedEventArgs e)
        {
			Console.WriteLine(String.Format("Get invaladation from Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
			Gtk.Application.Invoke(delegate {
				base.QueueDraw();
			});
        }

        /// <summary>
        /// update objects when map is draged/zoomed
        /// </summary>
        internal void ForceUpdateOverlays()
        {
			Console.WriteLine(String.Format("Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
			try
            {
                HoldInvalidation = true;

                foreach (GMapOverlay o in Overlays)
                {
                    if (o.IsVisibile)
                    {
                        o.ForceUpdate();
                    }
                }
            }
            finally
            {
                Refresh();
            }
        }

        /// <summary>
        /// updates markers local position
        /// </summary>
        /// <param name="marker"></param>
        public void UpdateMarkerLocalPosition(GMapMarker marker)
        {
            GPoint p = FromLatLngToLocal(marker.Position);
            {
                if (!MobileMode)
                {
                    p.OffsetNegative(Core.RenderOffset);
                }
                marker.LocalPosition = new System.Drawing.Point((int)(p.X + marker.Offset.X), (int)(p.Y + marker.Offset.Y));
            }
        }

        /// <summary>
        /// updates routes local position
        /// </summary>
        /// <param name="route"></param>
        public void UpdateRouteLocalPosition(GMapRoute route)
        {
          route.LocalPoints.Clear();
          
          for (int i = 0; i < route.Points.Count; i++)
          {
                GPoint p = FromLatLngToLocal(route.Points[i]);

                if (!MobileMode)
                {
                    p.OffsetNegative(Core.RenderOffset);
                }
                route.LocalPoints.Add(p);
            }
            route.UpdateGraphicsPath();
        }

        /// <summary>
        /// updates polygons local position
        /// </summary>
        /// <param name="polygon"></param>
        public void UpdatePolygonLocalPosition(GMapPolygon polygon)
        {
          polygon.LocalPoints.Clear();

          for (int i = 0; i < polygon.Points.Count; i++)
          {
                GPoint p = FromLatLngToLocal(polygon.Points[i]);
                if (!MobileMode)
                {
                    p.OffsetNegative(Core.RenderOffset);
                }
                polygon.LocalPoints.Add(p);
            }
            polygon.UpdateGraphicsPath();
        }

        /// <summary>
        /// sets zoom to max to fit rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool SetZoomToFitRect(RectLatLng rect)
        {
            if (lazyEvents)
            {
                lazySetZoomToFitRect = rect;
            }
            else
            {
                int maxZoom = Core.GetMaxZoomToFitRect(rect);
                if (maxZoom > 0)
                {
                    PointLatLng center = new PointLatLng(rect.Lat - (rect.HeightLat / 2), rect.Lng + (rect.WidthLng / 2));
                    Position = center;

                    if (maxZoom > MaxZoom)
                    {
                        maxZoom = MaxZoom;
                    }

                    if ((int)Zoom != maxZoom)
                    {
                        Zoom = maxZoom;
                    }

                    return true;
                }
            }

            return false;
        }

        RectLatLng? lazySetZoomToFitRect = null;
        bool lazyEvents = true;

        /// <summary>
        /// sets to max zoom to fit all markers and centers them in map
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public bool ZoomAndCenterMarkers(string overlayId)
        {
            RectLatLng? rect = GetRectOfAllMarkers(overlayId);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        /// <summary>
        /// zooms and centers all route
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public bool ZoomAndCenterRoutes(string overlayId)
        {
            RectLatLng? rect = GetRectOfAllRoutes(overlayId);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        /// <summary>
        /// zooms and centers route 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public bool ZoomAndCenterRoute(MapRoute route)
        {
            RectLatLng? rect = GetRectOfRoute(route);
            if (rect.HasValue)
            {
                return SetZoomToFitRect(rect.Value);
            }

            return false;
        }

        /// <summary>
        /// gets rectangle with all objects inside
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public RectLatLng? GetRectOfAllMarkers(string overlayId)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;

            foreach (GMapOverlay o in Overlays)
            {
                if (overlayId == null || o.Id == overlayId)
                {
                    if (o.IsVisibile && o.Markers.Count > 0)
                    {
                        foreach (GMapMarker m in o.Markers)
                        {
                            if (m.IsVisible)
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
                }
            }

            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
            {
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }

            return ret;
        }

        /// <summary>
        /// gets rectangle with all objects inside
        /// </summary>
        /// <param name="overlayId">overlay id or null to check all</param>
        /// <returns></returns>
        public RectLatLng? GetRectOfAllRoutes(string overlayId)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;

            foreach (GMapOverlay o in Overlays)
            {
                if (overlayId == null || o.Id == overlayId)
                {
                    if (o.IsVisibile && o.Routes.Count > 0)
                    {
                        foreach (GMapRoute route in o.Routes)
                        {
                            if (route.IsVisible && route.From.HasValue && route.To.HasValue)
                            {
                                foreach (PointLatLng p in route.Points)
                                {
                                    // left
                                    if (p.Lng < left)
                                    {
                                        left = p.Lng;
                                    }

                                    // top
                                    if (p.Lat > top)
                                    {
                                        top = p.Lat;
                                    }

                                    // right
                                    if (p.Lng > right)
                                    {
                                        right = p.Lng;
                                    }

                                    // bottom
                                    if (p.Lat < bottom)
                                    {
                                        bottom = p.Lat;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (left != double.MaxValue && right != double.MinValue && top != double.MinValue && bottom != double.MaxValue)
            {
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }

            return ret;
        }

        /// <summary>
        /// gets rect of route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public RectLatLng? GetRectOfRoute(MapRoute route)
        {
            RectLatLng? ret = null;

            double left = double.MaxValue;
            double top = double.MinValue;
            double right = double.MinValue;
            double bottom = double.MaxValue;

            if (route.From.HasValue && route.To.HasValue)
            {
                foreach (PointLatLng p in route.Points)
                {
                    // left
                    if (p.Lng < left)
                    {
                        left = p.Lng;
                    }

                    // top
                    if (p.Lat > top)
                    {
                        top = p.Lat;
                    }

                    // right
                    if (p.Lng > right)
                    {
                        right = p.Lng;
                    }

                    // bottom
                    if (p.Lat < bottom)
                    {
                        bottom = p.Lat;
                    }
                }
                ret = RectLatLng.FromLTRB(left, top, right, bottom);
            }
            return ret;
        }

        /// <summary>
        /// gets image of the current view
        /// </summary>
        /// <returns></returns>
        public Image ToImage()
        {
            Image ret = null;

            bool r = ForceDoubleBuffer;
            try
            {
                UpdateBackBuffer();

                if (!r)
                {
                    ForceDoubleBuffer = true;
                }

                Refresh();                
               // Application.DoEvents();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (var frame = (backBuffer.Clone() as Bitmap))
                    {
                        frame.Save(ms, ImageFormat.Png);
                    }
                    ret = Image.FromStream(ms);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!r)
                {
                    ForceDoubleBuffer = false;
                    ClearBackBuffer();
                }
            }
            return ret;
        }

		public Bitmap ToBitmap(Action<int> waitTilesCount = null, bool runningFromMainThreed = true)
		{
			bool r = ForceDoubleBuffer;
			Bitmap result;
			try {
				UpdateBackBuffer();

				if(!r) 
                {
					ForceDoubleBuffer = true;
				}

				int lastCount = 0;
				Console.WriteLine($"Start waiting {Core._loadWaitCount}");

				while(Core.IsWaitTileLoad)
				{
					Console.WriteLine($"waiting {Core._loadWaitCount}");

                #if NET46
                    if (lastCount != Core.TileLoadQueue4.Count)
                        waitTilesCount?.Invoke(Core.TileLoadQueue4.Count);
                #endif				

					if(runningFromMainThreed)
						Gtk.Main.Iteration();
					else
						Thread.Sleep(200);
				}

				DrawGraphics(gxOff);

				result = backBuffer.Clone() as Bitmap;
			} catch(Exception) {
				throw;
			} finally {
				if(!r) {
					ForceDoubleBuffer = false;
					ClearBackBuffer();
				}
			}
			return result;
		}

        /// <summary>
        /// offset position in pixels
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Offset(int x, int y)
        {
                if (IsRotated)
                {
                    System.Drawing.Point[] p = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
                    rotationMatrixInvert.TransformVectors(p);
                    x = (int)p[0].X;
                    y = (int)p[0].Y;
                }
                Core.DragOffset(new GPoint(x, y));

                ForceUpdateOverlays();
        }

#region UserControl Events

        public readonly static bool IsDesignerHosted = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

		protected override void OnShown() // OnLoad
        {
			base.OnShown();

            if (!IsDesignerHosted)
            {
                //MethodInvoker m = delegate
                //{
                //   Thread.Sleep(444);

                //OnSizeChanged(null);

                if (lazyEvents)
                {
                    lazyEvents = false;

                    if (lazySetZoomToFitRect.HasValue)
                    {
                        SetZoomToFitRect(lazySetZoomToFitRect.Value);
                        lazySetZoomToFitRect = null;
                    }
                }
                Core.OnMapOpen().ProgressChanged += new ProgressChangedEventHandler(invalidatorEngage);
                ForceUpdateOverlays();
                //};
                //this.BeginInvoke(m);
            }
        }
			
		/*    protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!IsDesignerHosted)
            {
                var f = ParentForm;
                if (f != null)
                {
                    while (f.ParentForm != null)
                    {
                        f = f.ParentForm;
                    }

                    if (f != null)
                    {
                        f.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
                    }
                }
            }
        }

        void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing)
            {
                Manager.CancelTileCaching();
            }
        }*/

		public override void Destroy()
		{
            Core.OnMapClose();

            Overlays.CollectionChanged -= new NotifyCollectionChangedEventHandler(Overlays_CollectionChanged);

            foreach (var o in Overlays)
            {
                o.Dispose();
            }
            Overlays.Clear();

            ScaleFont.Dispose();
            ScalePen.Dispose();
            CenterFormat.Dispose();
            CenterPen.Dispose();
            BottomFormat.Dispose();
            CopyrightFont.Dispose();
            EmptyTileBorders.Dispose();
            EmptytileBrush.Dispose();

            SelectedAreaFill.Dispose();
            SelectionPen.Dispose();
            ClearBackBuffer();

			base.Destroy();
        }

        PointLatLng selectionStart;
        PointLatLng selectionEnd;

        float? MapRenderTransform = null;

        public Color EmptyMapBackground = Color.WhiteSmoke;

		[Category("GMap.NET")]
		[Description("Widget has frame")]
		public bool HasFrame { get; set;}

		protected override bool OnExposeEvent(Gdk.EventExpose e)
		{
			Console.WriteLine(String.Format("Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
			if (ForceDoubleBuffer)
            {
                if (gxOff != null)
                {
                    DrawGraphics(gxOff);
					//throw new NotSupportedException();
                    //e.Graphics.DrawImage(backBuffer, 0, 0);
                }
            }
            else
            {
				Graphics g = null;
				try {
					g = Gtk.DotNet.Graphics.FromDrawable(e.Window);
				} catch(NullReferenceException) {
					return base.OnExposeEvent(e);
				}
				g.SetClip(new Rectangle(e.Area.X, e.Area.Y, e.Area.Width, e.Area.Height));
				DrawGraphics(g);
				g.Dispose();
            }

			if(HasFrame)
			{
				var gc = new Gdk.GC(e.Window);
				e.Window.DrawRectangle(gc, false, e.Area.X, e.Area.Y, e.Area.Width - 1, e.Area.Height - 1);
			}

			return base.OnExposeEvent(e);
        }

        void DrawGraphics(Graphics g)
        {
            // render white background
            g.Clear(EmptyMapBackground);

            if (MapRenderTransform.HasValue)
            {
#region -- scale --
                if (!MobileMode)
                {
					var center = new GPoint(Allocation.Width / 2, Allocation.Height / 2);
                    var delta = center;
                    delta.OffsetNegative(Core.RenderOffset);
                    var pos = center;
                    pos.OffsetNegative(delta);

                    g.ScaleTransform(MapRenderTransform.Value, MapRenderTransform.Value, MatrixOrder.Append);
                    g.TranslateTransform(pos.X, pos.Y, MatrixOrder.Append);

                    DrawMap(g);
                    g.ResetTransform();

                    g.TranslateTransform(pos.X, pos.Y, MatrixOrder.Append);
                }
                else
                {
                    DrawMap(g);
                    g.ResetTransform();
                }
                OnPaintOverlays(g);
#endregion
            }
            else
            {
                if (IsRotated)
                {
#region -- rotation --

                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    g.TranslateTransform((float)(Core.Width / 2.0), (float)(Core.Height / 2.0));
                    g.RotateTransform(-Bearing);
                    g.TranslateTransform((float)(-Core.Width / 2.0), (float)(-Core.Height / 2.0));

                    g.TranslateTransform(Core.RenderOffset.X, Core.RenderOffset.Y);

                    DrawMap(g);

                    g.ResetTransform();
                    g.TranslateTransform(Core.RenderOffset.X, Core.RenderOffset.Y);

                    OnPaintOverlays(g);

#endregion
                }
                else
                {
                    if (!MobileMode)
                    {
                        g.TranslateTransform(Core.RenderOffset.X, Core.RenderOffset.Y);
                    }
                    DrawMap(g);
                    OnPaintOverlays(g);
                }
            }
        }

        void DrawMap(Graphics g)
        {
            if (Core.UpdatingBounds || MapProvider == EmptyProvider.Instance || MapProvider == null)
            {
                Debug.WriteLine("Core.UpdatingBounds");
                return;
            }

            Core.TileDrawingListLock.AcquireReaderLock();
            Core.Matrix.EnterReadLock();

            //g.TextRenderingHint = TextRenderingHint.AntiAlias;
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.CompositingQuality = CompositingQuality.HighQuality;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;  

            try
            {
                foreach (var tilePoint in Core.TileDrawingList)
                {
                    {
                        Core.TileRect.Location = tilePoint.PosPixel;
                        if (ForceDoubleBuffer)
                        {
                            if (MobileMode)
                            {
                                Core.TileRect.Offset(Core.RenderOffset);
                            }
                        }
                        Core.TileRect.OffsetNegative(Core.CompensationOffset);

                        //if(Core.currentRegion.IntersectsWith(Core.tileRect) || IsRotated)
                        {
                            bool found = false;

                            Tile t = Core.Matrix.GetTileWithNoLock(Core.Zoom, tilePoint.PosXY);
                            if (t.NotEmpty)
                            {
                                // render tile
                                {
                                    foreach (GMapImage img in t.Overlays)
                                    {
                                        if (img != null && img.Img != null)
                                        {
                                            if (!found)
                                                found = true;

                                            if (!img.IsParent)
                                            {
                                                if (!MapRenderTransform.HasValue && !IsRotated)
                                                {
                                                    g.DrawImage(img.Img, Core.TileRect.X, Core.TileRect.Y, Core.TileRect.Width, Core.TileRect.Height);
                                                }
                                                else
                                                {
                                                    g.DrawImage(img.Img, new Rectangle((int)Core.TileRect.X, (int)Core.TileRect.Y, (int)Core.TileRect.Width, (int)Core.TileRect.Height), 0, 0, Core.TileRect.Width, Core.TileRect.Height, GraphicsUnit.Pixel, TileFlipXYAttributes);
                                                }
                                            }
                                            else
                                            {
                                                // TODO: move calculations to loader thread
                                                System.Drawing.RectangleF srcRect = new System.Drawing.RectangleF((float)(img.Xoff * (img.Img.Width / img.Ix)), (float)(img.Yoff * (img.Img.Height / img.Ix)), (img.Img.Width / img.Ix), (img.Img.Height / img.Ix));
                                                System.Drawing.Rectangle dst = new System.Drawing.Rectangle((int)Core.TileRect.X, (int)Core.TileRect.Y, (int)Core.TileRect.Width, (int)Core.TileRect.Height);

                                                g.DrawImage(img.Img, dst, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, TileFlipXYAttributes);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (FillEmptyTiles && MapProvider.Projection is MercatorProjection)
                            {
#region -- fill empty lines --
                                int zoomOffset = 1;
                                Tile parentTile = Tile.Empty;
                                long Ix = 0;

                                while (!parentTile.NotEmpty && zoomOffset < Core.Zoom && zoomOffset <= LevelsKeepInMemmory)
                                {
                                    Ix = (long)Math.Pow(2, zoomOffset);
                                    parentTile = Core.Matrix.GetTileWithNoLock(Core.Zoom - zoomOffset++, new GPoint((int)(tilePoint.PosXY.X / Ix), (int)(tilePoint.PosXY.Y / Ix)));
                                }

                                if (parentTile.NotEmpty)
                                {
                                    long Xoff = Math.Abs(tilePoint.PosXY.X - (parentTile.Pos.X * Ix));
                                    long Yoff = Math.Abs(tilePoint.PosXY.Y - (parentTile.Pos.Y * Ix));

                                    // render tile 
                                    {
                                        foreach (GMapImage img in parentTile.Overlays)
                                        {
                                            if (img != null && img.Img != null && !img.IsParent)
                                            {
                                                if (!found)
                                                    found = true;

                                                System.Drawing.RectangleF srcRect = new System.Drawing.RectangleF((float)(Xoff * (img.Img.Width / Ix)), (float)(Yoff * (img.Img.Height / Ix)), (img.Img.Width / Ix), (img.Img.Height / Ix));
                                                System.Drawing.Rectangle dst = new System.Drawing.Rectangle((int)Core.TileRect.X, (int)Core.TileRect.Y, (int)Core.TileRect.Width, (int)Core.TileRect.Height);

                                                g.DrawImage(img.Img, dst, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, TileFlipXYAttributes);
                                                g.FillRectangle(SelectedAreaFill, dst);
                                            }
                                        }
                                    }
                                }
#endregion
                            }
                            // add text if tile is missing
                            if (!found)
                            {
                                lock (Core.FailedLoads)
                                {
                                    var lt = new LoadTask(tilePoint.PosXY, Core.Zoom);
                                    if (Core.FailedLoads.ContainsKey(lt))
                                    {
                                        var ex = Core.FailedLoads[lt];

                                        g.FillRectangle(EmptytileBrush, new RectangleF(Core.TileRect.X, Core.TileRect.Y, Core.TileRect.Width, Core.TileRect.Height));

                                        g.DrawString("Exception: " + ex.Message, MissingDataFont, Brushes.Red, new RectangleF(Core.TileRect.X + 11, Core.TileRect.Y + 11, Core.TileRect.Width - 11, Core.TileRect.Height - 11));

                                        g.DrawString(EmptyTileText, MissingDataFont, Brushes.Blue, new RectangleF(Core.TileRect.X, Core.TileRect.Y, Core.TileRect.Width, Core.TileRect.Height), CenterFormat);

                                        g.DrawRectangle(EmptyTileBorders, (int)Core.TileRect.X, (int)Core.TileRect.Y, (int)Core.TileRect.Width, (int)Core.TileRect.Height);
                                    }
                                }
                            }

                            if (ShowTileGridLines)
                            {
                                g.DrawRectangle(EmptyTileBorders, (int)Core.TileRect.X, (int)Core.TileRect.Y, (int)Core.TileRect.Width, (int)Core.TileRect.Height);
                                {
                                    g.DrawString((tilePoint.PosXY == Core.CenterTileXYLocation ? "CENTER: " : "TILE: ") + tilePoint, MissingDataFont, Brushes.Red, new RectangleF(Core.TileRect.X, Core.TileRect.Y, Core.TileRect.Width, Core.TileRect.Height), CenterFormat);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Core.Matrix.LeaveReadLock();
                Core.TileDrawingListLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// override, to render something more
        /// </summary>
        /// <param name="g"></param>
        protected virtual void OnPaintOverlays(Graphics g)
        {
#if DEBUG
			if(GuiThread != Thread.CurrentThread)
			{
				Debug.WriteLine("Paint from not Gui thread.");
				return;
			}
#endif

            g.SmoothingMode = SmoothingMode.HighQuality;
            foreach (GMapOverlay o in Overlays)
            {
                if (o.IsVisibile)
                {
                    o.OnRender(g);
                }
            }

            // center in virtual space...
#if DEBUG
            if (!IsRotated)
            {
                g.DrawLine(ScalePen, -20, 0, 20, 0);
                g.DrawLine(ScalePen, 0, -20, 0, 20);
                g.DrawString("debug build", CopyrightFont, Brushes.Blue, 2, CopyrightFont.Height);
            }
#endif

            if (!MobileMode)
            {
                g.ResetTransform();
            }

            if (!SelectedArea.IsEmpty)
            {
                GPoint p1 = FromLatLngToLocal(SelectedArea.LocationTopLeft);
                GPoint p2 = FromLatLngToLocal(SelectedArea.LocationRightBottom);

                long x1 = p1.X;
                long y1 = p1.Y;
                long x2 = p2.X;
                long y2 = p2.Y;

                g.DrawRectangle(SelectionPen, x1, y1, x2 - x1, y2 - y1);
                g.FillRectangle(SelectedAreaFill, x1, y1, x2 - x1, y2 - y1);
            }

            if (renderHelperLine)
            {
				int mouseX, mouseY;
				base.GetPointer(out mouseX, out mouseY);

				g.DrawLine(HelperLinePen, mouseX, 0, mouseX, Allocation.Height);
				g.DrawLine(HelperLinePen, 0, mouseY, Allocation.Width, mouseY);
            }
            if (ShowCenter)
            {
				g.DrawLine(CenterPen, Allocation.Width / 2 - 5, Allocation.Height / 2, Allocation.Width / 2 + 5, Allocation.Height / 2);
				g.DrawLine(CenterPen, Allocation.Width / 2, Allocation.Height / 2 - 5, Allocation.Width / 2, Allocation.Height / 2 + 5);
            }

#region -- copyright --

            if (!string.IsNullOrEmpty(Core.Provider.Copyright))
            {
				g.DrawString(Core.Provider.Copyright, CopyrightFont, Brushes.Navy, 3, Allocation.Height - CopyrightFont.Height - 5);
            }

#endregion

#region -- draw scale --
            if (MapScaleInfoEnabled)
            {
				if (Allocation.Width > Core.PxRes5000Km)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes5000Km, 10);
                    g.DrawString("5000Km", ScaleFont, Brushes.Blue, Core.PxRes5000Km + 10, 11);
                }
				if (Allocation.Width > Core.PxRes1000Km)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes1000Km, 10);
                    g.DrawString("1000Km", ScaleFont, Brushes.Blue, Core.PxRes1000Km + 10, 11);
                }
				if (Allocation.Width > Core.PxRes100Km && Zoom > 2)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes100Km, 10);
                    g.DrawString("100Km", ScaleFont, Brushes.Blue, Core.PxRes100Km + 10, 11);
                }
				if (Allocation.Width > Core.PxRes10Km && Zoom > 5)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes10Km, 10);
                    g.DrawString("10Km", ScaleFont, Brushes.Blue, Core.PxRes10Km + 10, 11);
                }
				if (Allocation.Width > Core.PxRes1000M && Zoom >= 10)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes1000M, 10);
                    g.DrawString("1000m", ScaleFont, Brushes.Blue, Core.PxRes1000M + 10, 11);
                }
				if (Allocation.Width > Core.PxRes100M && Zoom > 11)
                {
                    g.DrawRectangle(ScalePen, 10, 10, Core.PxRes100M, 10);
                    g.DrawString("100m", ScaleFont, Brushes.Blue, Core.PxRes100M + 9, 11);
                }
            }
#endregion
        }

        readonly Matrix rotationMatrix = new Matrix();
        readonly Matrix rotationMatrixInvert = new Matrix();

        /// <summary>
        /// updates rotation matrix
        /// </summary>
        void UpdateRotationMatrix()
        {
            PointF center = new PointF(Core.Width / 2, Core.Height / 2);

            rotationMatrix.Reset();
            rotationMatrix.RotateAt(-Bearing, center);

            rotationMatrixInvert.Reset();
            rotationMatrixInvert.RotateAt(-Bearing, center);
            rotationMatrixInvert.Invert();
        }

        /// <summary>
        /// returs true if map bearing is not zero
        /// </summary>    
        [Browsable(false)]
        public bool IsRotated
        {
            get
            {
                return Core.IsRotated;
            }
        }

        /// <summary>
        /// bearing for rotation of the map
        /// </summary>
        [Category("GMap.NET")]
        public float Bearing
        {
            get
            {
                return Core.Bearing;
            }
            set
            {
                if (Core.Bearing != value)
                {
                    bool resize = Core.Bearing == 0;
                    Core.Bearing = value;

                    //if(VirtualSizeEnabled)
                    //{
                    // c.X += (Width - Core.vWidth) / 2;
                    // c.Y += (Height - Core.vHeight) / 2;
                    //}

                    UpdateRotationMatrix();

                    if (value != 0 && value % 360 != 0)
                    {
                        Core.IsRotated = true;

                        if (Core.TileRectBearing.Size == Core.TileRect.Size)
                        {
                            Core.TileRectBearing = Core.TileRect;
                            Core.TileRectBearing.Inflate(1, 1);
                        }
                    }
                    else
                    {
                        Core.IsRotated = false;
                        Core.TileRectBearing = Core.TileRect;
                    }

                    if (resize)
                    {
						Core.OnMapSizeChanged(Allocation.Width, Allocation.Height);
                    }

                    if (!HoldInvalidation && Core.IsStarted)
                    {
                        ForceUpdateOverlays();
                    }
                }
            }
        }

        /// <summary>
        /// shrinks map area, useful just for testing
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool VirtualSizeEnabled
        {
            get
            {
                return Core.VirtualSizeEnabled;
            }
            set
            {
                Core.VirtualSizeEnabled = value;
            }
        }

		protected override void OnSizeAllocated(Gdk.Rectangle box)
        {
			base.OnSizeAllocated(box);
			Console.WriteLine(String.Format("Allocation Tread: {0}", System.Threading.Thread.CurrentThread.ManagedThreadId));
            if (box.Width == 0 || box.Height == 0)
            {
                Debug.WriteLine("minimized");
                return;
            }

			if (box.Width == Core.Width && box.Height == Core.Height)
            {
                Debug.WriteLine("maximized");
                return;
            }
				
            if (!IsDesignerHosted)
            {
                if (ForceDoubleBuffer)
                {
                    UpdateBackBuffer();
                }
					
                if (VirtualSizeEnabled)
                {
                    Core.OnMapSizeChanged(Core.VWidth, Core.VHeight);
                }
                else
                {
					Core.OnMapSizeChanged(box.Width, box.Height);
                }
                //Core.currentRegion = new GRect(-50, -50, Core.Width + 50, Core.Height + 50);

                if (Visible && Core.IsStarted)
                {
                    if (IsRotated)
                    {
                        UpdateRotationMatrix();
                    }
                    ForceUpdateOverlays();
                }
            }
        }

		public void SetFakeAllocationSize(Gdk.Rectangle box)
		{

			if(!Core.IsStarted) {
				lazyEvents = false;
				Core.OnMapOpen();
				ForceUpdateOverlays();
				//Core.ReloadMap();
			}

			Allocation = box;
			OnSizeAllocated(box);
		}

        void UpdateBackBuffer()
        {
            ClearBackBuffer();

			backBuffer = new Bitmap(Allocation.Width, Allocation.Height);
            gxOff = Graphics.FromImage(backBuffer);
        }

        private void ClearBackBuffer()
        {
            if (backBuffer != null)
            {
                backBuffer.Dispose();
                backBuffer = null;
            }
            if (gxOff != null)
            {
                gxOff.Dispose();
                gxOff = null;
            }
        }

        bool isSelected = false;
		protected override bool OnButtonPressEvent(Gdk.EventButton e)
        {
            if (!IsMouseOverMarker)
            {
                if (e.Button == DragButton && CanDragMap)
                {
					Core.MouseDown = ApplyRotationInversion((int)e.X, (int)e.Y);
                    this.Invalidate();
                }
                else if (!isSelected)
                {
                    isSelected = true;
                    SelectedArea = RectLatLng.Empty;
                    selectionEnd = PointLatLng.Empty;
					selectionStart = FromLocalToLatLng((int)e.X, (int)e.Y);
                }
            }
			return base.OnButtonPressEvent(e);
        }

		protected override bool OnButtonReleaseEvent(Gdk.EventButton e)
        {
            if (isSelected)
            {
                isSelected = false;
            }

            if (Core.IsDragging)
            {
                if (isDragging)
                {
                    isDragging = false;
                    Debug.WriteLine("IsDragging = " + isDragging);
					currentCursorType = Gdk.CursorType.LeftPtr;
					this.GdkWindow.Cursor = new Gdk.Cursor(currentCursorType);
                }
                Core.EndDrag();

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    if (Core.LastLocationInBounds.HasValue)
                    {
                        Position = Core.LastLocationInBounds.Value;
                    }
                }
            }
            else
            {
                if (e.Button == DragButton)
                {
                    Core.MouseDown = GPoint.Empty;
                }

                if (!selectionEnd.IsEmpty && !selectionStart.IsEmpty)
                {
                    bool zoomtofit = false;

					if (!SelectedArea.IsEmpty && e.State.HasFlag(Gdk.ModifierType.ShiftMask))
                    {
                        zoomtofit = SetZoomToFitRect(SelectedArea);
                    }

                    if (OnSelectionChange != null)
                    {
                        OnSelectionChange(SelectedArea, zoomtofit);
                    }
                }
                else
                {
                    Invalidate();
                }
            }
			return base.OnButtonReleaseEvent(e);
        }

		/// <summary>
        /// apply transformation if in rotation mode
        /// </summary>
        GPoint ApplyRotationInversion(int x, int y)
        {
            GPoint ret = new GPoint(x, y);

            if (IsRotated)
            {
                System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
                rotationMatrixInvert.TransformPoints(tt);
                var f = tt[0];

                ret.X = f.X;
                ret.Y = f.Y;
            }

            return ret;
        }

        /// <summary>
        /// apply transformation if in rotation mode
        /// </summary>
        GPoint ApplyRotation(int x, int y)
        {
            GPoint ret = new GPoint(x, y);

            if (IsRotated)
            {
                System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
                rotationMatrix.TransformPoints(tt);
                var f = tt[0];

                ret.X = f.X;
                ret.Y = f.Y;
            }

            return ret;
        }

		Gdk.CursorType currentCursorType;

        /// <summary>
        /// Gets the width and height of a rectangle centered on the point the mouse
        /// button was pressed, within which a drag operation will not begin.
        /// </summary>
      public Size DragSize = new Size(4, 4);

		protected override bool OnMotionNotifyEvent(Gdk.EventMotion e)
        {   
            if (!Core.IsDragging && !Core.MouseDown.IsEmpty)
            {
				GPoint p = ApplyRotationInversion((int)e.X, (int)e.Y);
                if (Math.Abs(p.X - Core.MouseDown.X) * 2 >= DragSize.Width || Math.Abs(p.Y - Core.MouseDown.Y) * 2 >= DragSize.Height)
                {
                    Core.BeginDrag(Core.MouseDown);
                }
            }

            if (Core.IsDragging)
            {
                if (!isDragging)
                {
                    isDragging = true;
                    Debug.WriteLine("IsDragging = " + isDragging);

					currentCursorType = Gdk.CursorType.Fleur;
					this.GdkWindow.Cursor = new Gdk.Cursor(currentCursorType);
                }

                if (BoundsOfMap.HasValue && !BoundsOfMap.Value.Contains(Position))
                {
                    // ...
                }
                else
                {
					Core.MouseCurrent = ApplyRotationInversion((int)e.X, (int)e.Y);
                    Core.Drag(Core.MouseCurrent);
                    if (MobileMode || IsRotated)
                    {
                        ForceUpdateOverlays();
                    }
					base.QueueDraw();
                }
            }
            else
            {
				if (isSelected && !selectionStart.IsEmpty && (e.State.HasFlag(Gdk.ModifierType.ShiftMask) || e.State.HasFlag(Gdk.ModifierType.Mod1Mask) || DisableAltForSelection))
                {
					selectionEnd = FromLocalToLatLng((int)e.X, (int)e.Y);
                    {
                        GMap.NET.PointLatLng p1 = selectionStart;
                        GMap.NET.PointLatLng p2 = selectionEnd;

                        double x1 = Math.Min(p1.Lng, p2.Lng);
                        double y1 = Math.Max(p1.Lat, p2.Lat);
                        double x2 = Math.Max(p1.Lng, p2.Lng);
                        double y2 = Math.Min(p1.Lat, p2.Lat);

                        SelectedArea = new RectLatLng(y1, x1, x2 - x1, y1 - y2);
                    }
                }
                else
                    if (Core.MouseDown.IsEmpty)
                    {
                        for (int i = Overlays.Count - 1; i >= 0; i--)
                        {
                            GMapOverlay o = Overlays[i];
                            if (o != null && o.IsVisibile)
                            {
                                foreach (GMapMarker m in o.Markers)
                                {
                                    if (m.IsVisible && m.IsHitTestVisible)
                                    {
#region -- check --

										GPoint rp = new GPoint((long)e.X, (long)e.Y);
                                        if (!MobileMode)
                                        {
                                            rp.OffsetNegative(Core.RenderOffset);
                                        }
                                        if (m.LocalArea.Contains((int)rp.X, (int)rp.Y))
                                        {
                                            if (!m.IsMouseOver)
                                            {
                                                SetCursorHandOnEnter();

                                                m.IsMouseOver = true;
                                                IsMouseOverMarker = true;

                                                if (OnMarkerEnter != null)
                                                {
                                                    OnMarkerEnter(m);
                                                }

                                                Invalidate();
                                            }
                                        }
                                        else if (m.IsMouseOver)
                                        {
                                            m.IsMouseOver = false;
                                            IsMouseOverMarker = false;
                                            RestoreCursorOnLeave();
                                            if (OnMarkerLeave != null)
                                            {
                                                OnMarkerLeave(m);
                                            }

                                            Invalidate();
                                        }
#endregion
                                    }
                                }
									
                                foreach (GMapRoute m in o.Routes)
                                {
                                    if (m.IsVisible && m.IsHitTestVisible)
                                    {
#region -- check --

										GPoint rp = new GPoint((long)e.X, (long)e.Y);
                                        if (!MobileMode)
                                        {
                                            rp.OffsetNegative(Core.RenderOffset);
                                        }
                                        if (m.IsInside((int)rp.X, (int)rp.Y))
                                        {
                                            if (!m.IsMouseOver)
                                            {
                                                SetCursorHandOnEnter();
                                                m.IsMouseOver = true;
                                                IsMouseOverRoute = true;

                                                if (OnRouteEnter != null)
                                                {
                                                    OnRouteEnter(m);
                                                }

                                                Invalidate();
                                            }
                                        }
                                        else
                                        {
                                            if (m.IsMouseOver)
                                            {
                                                m.IsMouseOver = false;
                                                IsMouseOverRoute = false;
                                                RestoreCursorOnLeave();
                                                if (OnRouteLeave != null)
                                                {
                                                    OnRouteLeave(m);
                                                }

                                                Invalidate();
                                            }
                                        }
#endregion
                                    }
                                }

                                foreach (GMapPolygon m in o.Polygons)
                                {
                                    if (m.IsVisible && m.IsHitTestVisible)
                                    {
#region -- check --
										GPoint rp = new GPoint((long)e.X, (long)e.Y);

                                        if (!MobileMode)
                                        {
                                            rp.OffsetNegative(Core.RenderOffset);
                                        }

                                        if (m.IsInsideLocal((int)rp.X, (int)rp.Y))
                                        {
                                            if (!m.IsMouseOver)
                                            {
                                                SetCursorHandOnEnter();
                                                m.IsMouseOver = true;
                                                IsMouseOverPolygon = true;

                                                if (OnPolygonEnter != null)
                                                {
                                                    OnPolygonEnter(m);
                                                }

                                                Invalidate();
                                            }
                                        }
                                        else
                                        {
                                            if (m.IsMouseOver)
                                            {
                                                m.IsMouseOver = false;
                                                IsMouseOverPolygon = false;
                                                RestoreCursorOnLeave();
                                                if (OnPolygonLeave != null)
                                                {
                                                    OnPolygonLeave(m);
                                                }

                                                Invalidate();
                                            }
                                        }
#endregion
                                    }
                                }
                            }
                        }
                    }
						
                if (renderHelperLine)
                {
					base.QueueDraw();
                }
            }
			return base.OnMotionNotifyEvent(e);
        }

        internal void RestoreCursorOnLeave()
        {
			if (overObjectCount <= 0 && currentCursorType != Gdk.CursorType.LeftPtr)
            {
                overObjectCount = 0;
				currentCursorType = Gdk.CursorType.LeftPtr;
				this.GdkWindow.Cursor = new Gdk.Cursor(currentCursorType);
            }
        }

        internal void SetCursorHandOnEnter()
        {
			if (overObjectCount <= 0 && currentCursorType != Gdk.CursorType.Hand1)
            {
                overObjectCount = 0;
				currentCursorType = Gdk.CursorType.Hand1;
				this.GdkWindow.Cursor = new Gdk.Cursor(currentCursorType);
            }
        }

        /// <summary>
        /// prevents focusing map if mouse enters it's area
        /// </summary>
        public bool DisableFocusOnMouseEnter = false;

		protected override bool OnEnterNotifyEvent(Gdk.EventCrossing e)
        {
            if (!DisableFocusOnMouseEnter)
            {
				GrabFocus();
            }
			return base.OnEnterNotifyEvent(e);
        }

        /// <summary>
        /// reverses MouseWheel zooming direction
        /// </summary>
        public bool InvertedMouseWheelZooming = false;

        /// <summary>
        /// lets you zoom by MouseWheel even when pointer is in area of marker
        /// </summary>
        public bool IgnoreMarkerOnMouseWheel = false;

		protected override bool OnScrollEvent(Gdk.EventScroll e)
        {
            if (MouseWheelZoomEnabled && (!IsMouseOverMarker || IgnoreMarkerOnMouseWheel) && !Core.IsDragging)
            {
				if (Core.MouseLastZoom.X != (int)e.X && Core.MouseLastZoom.Y != (int)e.Y)
                {
                    if (MouseWheelZoomType == MouseWheelZoomType.MousePositionAndCenter)
                    {
						Core.Position = FromLocalToLatLng((int)e.X, (int)e.Y);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.ViewCenter)
                    {
						Core.Position = FromLocalToLatLng((int)Allocation.Width / 2, (int)Allocation.Height / 2);
                    }
                    else if (MouseWheelZoomType == MouseWheelZoomType.MousePositionWithoutCenter)
                    {
						Core.Position = FromLocalToLatLng((int)e.X, (int)e.Y);
                    }

					Core.MouseLastZoom.X = (int)e.X;
					Core.MouseLastZoom.Y = (int)e.Y;
                }

                // set mouse position to map center
                if (MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
                {
                    if (!GMaps.Instance.IsRunningOnMono)
                    {
                        //FIXME
						//System.Drawing.Point p = PointToScreen(new System.Drawing.Point(Width / 2, Height / 2));
                        //Stuff.SetCursorPos((int)p.X, (int)p.Y);
                    }
                }

                Core.MouseWheelZooming = true;

				if (e.Direction == Gdk.ScrollDirection.Up)
                {
                    if (!InvertedMouseWheelZooming)
                    {
                        Zoom = ((int)Zoom) + 1;
                    }
                    else
                    {
                        Zoom = ((int)(Zoom + 0.99)) - 1;
                    }
                }
				else if (e.Direction == Gdk.ScrollDirection.Down)
                {
                    if (!InvertedMouseWheelZooming)
                    {
                        Zoom = ((int)(Zoom + 0.99)) - 1;
                    }
                    else
                    {
                        Zoom = ((int)Zoom) + 1;
                    }
                }

                Core.MouseWheelZooming = false;
             }
			return base.OnScrollEvent(e);
        }
#endregion

#region IGControl Members

        /// <summary>
        /// Call it to empty tile cache & reload tiles
        /// </summary>
        public void ReloadMap()
        {
            Core.ReloadMap();
        }

        /// <summary>
        /// set current position using keywords
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>true if successfull</returns>
        public GeoCoderStatusCode SetPositionByKeywords(string keys)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;

            GeocodingProvider gp = MapProvider as GeocodingProvider;
            if (gp == null)
            {
                gp = GMapProviders.OpenStreetMap as GeocodingProvider;
            }

            if (gp != null)
            {
                var pt = gp.GetPoint(keys, out status);
                if (status == GeoCoderStatusCode.G_GEO_SUCCESS && pt.HasValue)
                {
                    Position = pt.Value;
                }
            }

            return status;
        }

		public PointLatLng FromLocalToLatLng (GPoint point)
		{
			return FromLocalToLatLng ((int)point.X, (int)point.Y);
		}

        /// <summary>
        /// gets world coordinate from local control coordinate 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public PointLatLng FromLocalToLatLng(int x, int y)
        {
            if (MapRenderTransform.HasValue)
            {
                //var xx = (int)(Core.RenderOffset.X + ((x - Core.RenderOffset.X) / MapRenderTransform.Value));
                //var yy = (int)(Core.RenderOffset.Y + ((y - Core.RenderOffset.Y) / MapRenderTransform.Value));

                //PointF center = new PointF(Core.Width / 2, Core.Height / 2);

                //Matrix m = new Matrix();
                //m.Translate(-Core.RenderOffset.X, -Core.RenderOffset.Y);
                //m.Scale(MapRenderTransform.Value, MapRenderTransform.Value);

                //System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
                //m.TransformPoints(tt);
                //var z = tt[0];

                //

                x = (int)(Core.RenderOffset.X + ((x - Core.RenderOffset.X) / MapRenderTransform.Value));
                y = (int)(Core.RenderOffset.Y + ((y - Core.RenderOffset.Y) / MapRenderTransform.Value));
            }

            if (IsRotated)
            {
                System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point(x, y) };
                rotationMatrixInvert.TransformPoints(tt);
                var f = tt[0];

                if (VirtualSizeEnabled)
                {
					f.X += (Allocation.Width - Core.VWidth) / 2;
					f.Y += (Allocation.Height - Core.VHeight) / 2;
                }

                x = f.X;
                y = f.Y;
            }
            return Core.FromLocalToLatLng(x, y);
        }

        /// <summary>
        /// gets local coordinate from world coordinate
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public GPoint FromLatLngToLocal(PointLatLng point)
        {
            GPoint ret = Core.FromLatLngToLocal(point);

            if (MapRenderTransform.HasValue)
            {
                ret.X = (int)(Core.RenderOffset.X + ((Core.RenderOffset.X - ret.X) * -MapRenderTransform.Value));
                ret.Y = (int)(Core.RenderOffset.Y + ((Core.RenderOffset.Y - ret.Y) * -MapRenderTransform.Value));
            }

            if (IsRotated)
            {
                System.Drawing.Point[] tt = new System.Drawing.Point[] { new System.Drawing.Point((int)ret.X, (int)ret.Y) };
                rotationMatrix.TransformPoints(tt);
                var f = tt[0];

                if (VirtualSizeEnabled)
                {
					f.X += (Allocation.Width - Core.VWidth) / 2;
					f.Y += (Allocation.Height - Core.VHeight) / 2;
                }

                ret.X = f.X;
                ret.Y = f.Y;
            }
				
            return ret;
        }

        /// <summary>
        /// shows map db export dialog
        /// </summary>
        /// <returns></returns>
        public bool ShowExportDialog()
        {
/*            using (FileDialog dlg = new SaveFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.DefaultExt = "gmdb";
                dlg.ValidateNames = true;
                dlg.Title = "GMap.NET: Export map to db, if file exsist only new data will be added";
                dlg.FileName = "DataExp";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool ok = GMaps.Instance.ExportToGMDB(dlg.FileName);
                    if (ok)
                    {
                        MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    return ok;
                }
            }
*/
            return false;
        }
        /// <summary>
        /// shows map dbimport dialog
        /// </summary>
        /// <returns></returns>
        public bool ShowImportDialog()
        {
/*            using (FileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.DefaultExt = "gmdb";
                dlg.ValidateNames = true;
                dlg.Title = "GMap.NET: Import to db, only new data will be added";
                dlg.FileName = "DataImport";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dlg.Filter = "GMap.NET DB files (*.gmdb)|*.gmdb";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    bool ok = GMaps.Instance.ImportFromGMDB(dlg.FileName);
                    if (ok)
                    {
                        MessageBox.Show("Complete!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ReloadMap();
                    }
                    else
                    {
                        MessageBox.Show("Failed!", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    return ok;
                }
            }
*/
            return false;
        }

        private ScaleModes scaleMode = ScaleModes.Integer;

        [Category("GMap.NET")]
        [Description("map scale type")]
        public ScaleModes ScaleMode
        {
            get
            {
                return scaleMode;
            }
            set
            {
                scaleMode = value;
            }
        }

        [Category("GMap.NET"), DefaultValue(0)]
        public double Zoom
        {
            get
            {
                return zoomReal;
            }
            set
            {
                if (zoomReal != value)
                {
					//FIXME May be this line crash in Tread on mono.
					//Debug.WriteLine("ZoomPropertyChanged: " + zoomReal + " -> " + value);

                    if (value > MaxZoom)
                    {
                        zoomReal = MaxZoom;
                    }
                    else if (value < MinZoom)
                    {
                        zoomReal = MinZoom;
                    }
                    else
                    {
                        zoomReal = value;
                    }
						
                    double remainder = value % 1;
                    if (ScaleMode == ScaleModes.Fractional && remainder != 0)
                    {
                        float scaleValue = (float)Math.Pow(2d, remainder);
                        {
                            MapRenderTransform = scaleValue;
                        }

                        ZoomStep = Convert.ToInt32(value - remainder);
                    }
                    else
                    {
                        MapRenderTransform = null;
                        ZoomStep = (int)Math.Floor(value);
                        //zoomReal = ZoomStep;
                    }

                    if (Core.IsStarted && !IsDragging)
                    {
                        ForceUpdateOverlays();
                    }
                }
            }
        }

        /// <summary>
        /// map zoom level
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        internal int ZoomStep
        {
            get
            {
                return Core.Zoom;
            }
            set
            {
                if (value > MaxZoom)
                {
                    Core.Zoom = MaxZoom;
                }
                else if (value < MinZoom)
                {
                    Core.Zoom = MinZoom;
                }
                else
                {
                    Core.Zoom = value;
                }
            }
        }

        /// <summary>
        /// current map center position
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public PointLatLng Position
        {
            get
            {
                return Core.Position;
            }
            set
            {
                Core.Position = value;

                if (Core.IsStarted)
                {
                    ForceUpdateOverlays();
                }
            }
        }

        /// <summary>
        /// current position in pixel coordinates
        /// </summary>
        [Browsable(false)]
        public GPoint PositionPixel
        {
            get
            {
                return Core.PositionPixel;
            }
        }

        /// <summary>
        /// location of cache
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string CacheLocation
        {
            get
            {
#if !DESIGN
                return CacheLocator.Location;
#else
            return string.Empty;
#endif
            }
            set
            {
#if !DESIGN
                CacheLocator.Location = value;
#endif
            }
        }

        bool isDragging = false;

        /// <summary>
        /// is user dragging map
        /// </summary>
        [Browsable(false)]
        public bool IsDragging
        {
            get
            {
                return isDragging;
            }
        }

        bool isMouseOverMarker;
        internal int overObjectCount = 0;

        /// <summary>
        /// is mouse over marker
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsMouseOverMarker
        {
            get
            {
                return isMouseOverMarker;
            }
            internal set
            {
                isMouseOverMarker = value;
                overObjectCount += value ? 1 : -1;
            }
        }

        bool isMouseOverRoute;

        /// <summary>
        /// is mouse over route
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsMouseOverRoute
        {
            get
            {
                return isMouseOverRoute;
            }
            internal set
            {
                isMouseOverRoute = value;
                overObjectCount += value ? 1 : -1;
            }
        }

        bool isMouseOverPolygon;

        /// <summary>
        /// is mouse over polygon
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsMouseOverPolygon
        {
            get
            {
                return isMouseOverPolygon;
            }
            internal set
            {
                isMouseOverPolygon = value;
                overObjectCount += value ? 1 : -1;
            }
        }

        /// <summary>
        /// gets current map view top/left coordinate, width in Lng, height in Lat
        /// </summary>
        [Browsable(false)]
        public RectLatLng ViewArea
        {
            get
            {
                if (!IsRotated) 
                {
                    return Core.ViewArea;
                }
                else if (Core.Provider.Projection != null)
                {
                    var p = FromLocalToLatLng(0, 0);
					var p2 = FromLocalToLatLng(Allocation.Width, Allocation.Height);

                    return RectLatLng.FromLTRB(p.Lng, p.Lat, p2.Lng, p2.Lat);
                }
                return RectLatLng.Empty;        
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GMapProvider MapProvider
        {
            get
            {
                return Core.Provider;
            }
            set
            {
                if (Core.Provider == null || !Core.Provider.Equals(value))
                {
                    Debug.WriteLine("MapType: " + Core.Provider.Name + " -> " + value.Name);

                    RectLatLng viewarea = SelectedArea;
                    if (viewarea != RectLatLng.Empty)
                    {
                        Position = new PointLatLng(viewarea.Lat - viewarea.HeightLat / 2, viewarea.Lng + viewarea.WidthLng / 2);
                    }
                    else
                    {
                        viewarea = ViewArea;
                    }

                    Core.Provider = value;

                    if (Core.IsStarted)
                    {
                        if (Core.ZoomToArea)
                        {
                            // restore zoomrect as close as possible
                            if (viewarea != RectLatLng.Empty && viewarea != ViewArea)
                            {
                                int bestZoom = Core.GetMaxZoomToFitRect(viewarea);
                                if (bestZoom > 0 && Zoom != bestZoom)
                                {
                                    Zoom = bestZoom;
                                }
                            }
                        }
                        else
                        {
                            ForceUpdateOverlays();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// is routes enabled
        /// </summary>
        [Category("GMap.NET")]
        public bool RoutesEnabled
        {
            get
            {
                return Core.RoutesEnabled;
            }
            set
            {
                Core.RoutesEnabled = value;
            }
        }

        /// <summary>
        /// is polygons enabled
        /// </summary>
        [Category("GMap.NET")]
        public bool PolygonsEnabled
        {
            get
            {
                return Core.PolygonsEnabled;
            }
            set
            {
                Core.PolygonsEnabled = value;
            }
        }

        /// <summary>
        /// is markers enabled
        /// </summary>
        [Category("GMap.NET")]
        public bool MarkersEnabled
        {
            get
            {
                return Core.MarkersEnabled;
            }
            set
            {
                Core.MarkersEnabled = value;
            }
        }

        /// <summary>
        /// can user drag map
        /// </summary>
        [Category("GMap.NET")]
        public bool CanDragMap
        {
            get
            {
                return Core.CanDragMap;
            }
            set
            {
                Core.CanDragMap = value;
            }
        }

        /// <summary>
        /// map render mode
        /// </summary>
        [Browsable(false)]
        public RenderMode RenderMode
        {
            get
            {
                return Core.RenderMode;
            }
            internal set
            {
                Core.RenderMode = value;
            }
        }

        /// <summary>
        /// gets map manager
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GMaps Manager
        {
            get
            {
                return GMaps.Instance;
            }
        }

#endregion

#region IGControl event Members

        /// <summary>
        /// occurs when current position is changed
        /// </summary>
        public event PositionChanged OnPositionChanged
        {
            add
            {
                Core.OnCurrentPositionChanged += value;
            }
            remove
            {
                Core.OnCurrentPositionChanged -= value;
            }
        }

        /// <summary>
        /// occurs when tile set load is complete
        /// </summary>
        public event TileLoadComplete OnTileLoadComplete
        {
            add
            {
                Core.OnTileLoadComplete += value;
            }
            remove
            {
                Core.OnTileLoadComplete -= value;
            }
        }

        /// <summary>
        /// occurs when tile set is starting to load
        /// </summary>
        public event TileLoadStart OnTileLoadStart
        {
            add
            {
                Core.OnTileLoadStart += value;
            }
            remove
            {
                Core.OnTileLoadStart -= value;
            }
        }

        /// <summary>
        /// occurs on map drag
        /// </summary>
        public event MapDrag OnMapDrag
        {
            add
            {
                Core.OnMapDrag += value;
            }
            remove
            {
                Core.OnMapDrag -= value;
            }
        }

        /// <summary>
        /// occurs on map zoom changed
        /// </summary>
        public event MapZoomChanged OnMapZoomChanged
        {
            add
            {
                Core.OnMapZoomChanged += value;
            }
            remove
            {
                Core.OnMapZoomChanged -= value;
            }
        }

        /// <summary>
        /// occures on map type changed
        /// </summary>
        public event MapTypeChanged OnMapTypeChanged
        {
            add
            {
                Core.OnMapTypeChanged += value;
            }
            remove
            {
                Core.OnMapTypeChanged -= value;
            }
        }

        /// <summary>
        /// occurs on empty tile displayed
        /// </summary>
        public event EmptyTileError OnEmptyTileError
        {
            add
            {
                Core.OnEmptyTileError += value;
            }
            remove
            {
                Core.OnEmptyTileError -= value;
            }
        }

#endregion

#region Serialization

        static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        /// <summary>
        /// Serializes the overlays.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void SerializeOverlays(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Create an array from the overlays
            GMapOverlay[] overlayArray = new GMapOverlay[this.Overlays.Count];
            this.Overlays.CopyTo(overlayArray, 0);

            // Serialize the overlays
            BinaryFormatter.Serialize(stream, overlayArray);
        }

        /// <summary>
        /// De-serializes the overlays.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void DeserializeOverlays(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // De-serialize the overlays
            GMapOverlay[] overlayArray = BinaryFormatter.Deserialize(stream) as GMapOverlay[];

            // Populate the collection of overlays.
            foreach (GMapOverlay overlay in overlayArray)
            {
                overlay.Control = this;
                this.Overlays.Add(overlay);
            }

            this.ForceUpdateOverlays();
        }

#endregion
    }

    public enum ScaleModes
    {
        /// <summary>
        /// no scaling
        /// </summary>
        Integer,

        /// <summary>
        /// scales to fractional level, CURRENT VERSION DOESN'T HANDLE OBJECT POSITIONS CORRECLTY, 
        /// http://greatmaps.codeplex.com/workitem/16046
        /// </summary>
        Fractional,
    }
		
    public enum HelperLineOptions
    {
        DontShow = 0,
        ShowAlways = 1,
        ShowOnModifierKey = 2
    }

    public delegate void SelectionChange(RectLatLng Selection, bool ZoomToFit);
}
