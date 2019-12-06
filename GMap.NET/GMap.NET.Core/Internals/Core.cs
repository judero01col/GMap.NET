using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
#if NET40
using System.Collections.Concurrent;
#endif

namespace GMap.NET.Internals
{
    /// <summary>
    ///     internal map control core
    /// </summary>
    internal sealed class Core : IDisposable
    {
        internal PointLatLng _position;
        private GPoint _positionPixel;

        internal GPoint RenderOffset;
        internal GPoint CenterTileXYLocation;
        private GPoint _centerTileXYLocationLast;
        private GPoint _dragPoint;
        internal GPoint CompensationOffset;

        internal GPoint MouseDown;
        internal GPoint MouseCurrent;
        internal GPoint MouseLastZoom;

        public MouseWheelZoomType MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
        public bool MouseWheelZoomEnabled = true;

        public PointLatLng? LastLocationInBounds;
        public bool VirtualSizeEnabled = false;

        private GSize _sizeOfMapArea;
        private GSize _minOfTiles;
        private GSize _maxOfTiles;

        internal GRect TileRect;

        internal GRect TileRectBearing;

        //private GRect _currentRegion;
        internal float Bearing = 0;
        public bool IsRotated = false;

        internal bool FillEmptyTiles = true;

        public TileMatrix Matrix = new TileMatrix();

        internal List<DrawTile> TileDrawingList = new List<DrawTile>();
        internal FastReaderWriterLock TileDrawingListLock = new FastReaderWriterLock();

#if !NET40
        public readonly Stack<LoadTask> TileLoadQueue = new Stack<LoadTask>();
#endif

        static readonly int GThreadPoolSize = 4;

        DateTime _lastTileLoadStart = DateTime.Now;
        DateTime _lastTileLoadEnd = DateTime.Now;
        internal volatile bool IsStarted;
        int _zoom;

        internal double ScaleX = 1;
        internal double ScaleY = 1;

        internal int MaxZoom = 2;
        internal int MinZoom = 2;
        internal int Width;
        internal int Height;

        internal int PxRes100M; // 100 meters
        internal int PxRes1000M; // 1km  
        internal int PxRes10Km; // 10km
        internal int PxRes100Km; // 100km
        internal int PxRes1000Km; // 1000km
        internal int PxRes5000Km; // 5000km

        /// <summary>
        ///     is user dragging map
        /// </summary>
        public bool IsDragging;

        public Core()
        {
            Provider = EmptyProvider.Instance;
        }

        /// <summary>
        ///     map zoom
        /// </summary>
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (_zoom != value && !IsDragging)
                {
                    _zoom = value;

                    _minOfTiles = Provider.Projection.GetTileMatrixMinXY(value);
                    _maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(value);

                    _positionPixel = Provider.Projection.FromLatLngToPixel(Position, value);

                    if (IsStarted)
                    {
                        CancelAsyncTasks();

                        Matrix.ClearLevelsBelove(_zoom - LevelsKeepInMemory);
                        Matrix.ClearLevelsAbove(_zoom + LevelsKeepInMemory);

                        lock (FailedLoads)
                        {
                            FailedLoads.Clear();
                            _raiseEmptyTileError = true;
                        }

                        GoToCurrentPositionOnZoom();
                        UpdateBounds();

                        if (OnMapZoomChanged != null)
                        {
                            OnMapZoomChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     current marker position in pixel coordinates
        /// </summary>
        public GPoint PositionPixel
        {
            get
            {
                return _positionPixel;
            }
        }

        /// <summary>
        ///     current marker position
        /// </summary>
        public PointLatLng Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                _positionPixel = Provider.Projection.FromLatLngToPixel(value, Zoom);

                if (IsStarted)
                {
                    if (!IsDragging)
                    {
                        GoToCurrentPosition();
                    }

                    if (OnCurrentPositionChanged != null)
                        OnCurrentPositionChanged(_position);
                }
            }
        }

        private GMapProvider _provider;

        public GMapProvider Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                if (_provider == null || !_provider.Equals(value))
                {
                    bool diffProjection = _provider == null || _provider.Projection != value.Projection;

                    _provider = value;

                    if (!_provider.IsInitialized)
                    {
                        _provider.IsInitialized = true;
                        _provider.OnInitialized();
                    }

                    if (_provider.Projection != null && diffProjection)
                    {
                        TileRect = new GRect(GPoint.Empty, Provider.Projection.TileSize);
                        TileRectBearing = TileRect;
                        if (IsRotated)
                        {
                            TileRectBearing.Inflate(1, 1);
                        }

                        _minOfTiles = Provider.Projection.GetTileMatrixMinXY(Zoom);
                        _maxOfTiles = Provider.Projection.GetTileMatrixMaxXY(Zoom);
                        _positionPixel = Provider.Projection.FromLatLngToPixel(Position, Zoom);
                    }

                    if (IsStarted)
                    {
                        CancelAsyncTasks();
                        if (diffProjection)
                        {
                            OnMapSizeChanged(Width, Height);
                        }

                        ReloadMap();

                        if (MinZoom < _provider.MinZoom)
                        {
                            MinZoom = _provider.MinZoom;
                        }

                        //if(provider.MaxZoom.HasValue && maxZoom > provider.MaxZoom)
                        //{
                        //   maxZoom = provider.MaxZoom.Value;
                        //}

                        ZoomToArea = true;

                        if (_provider.Area.HasValue && !_provider.Area.Value.Contains(Position))
                        {
                            SetZoomToFitRect(_provider.Area.Value);
                            ZoomToArea = false;
                        }

                        if (OnMapTypeChanged != null)
                        {
                            OnMapTypeChanged(value);
                        }
                    }
                }
            }
        }

        internal bool ZoomToArea = true;

        /// <summary>
        ///     sets zoom to max to fit rect
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool SetZoomToFitRect(RectLatLng rect)
        {
            int mmaxZoom = GetMaxZoomToFitRect(rect);
            if (mmaxZoom > 0)
            {
                var center = new PointLatLng(rect.Lat - rect.HeightLat / 2, rect.Lng + rect.WidthLng / 2);
                Position = center;

                if (mmaxZoom > MaxZoom)
                {
                    mmaxZoom = MaxZoom;
                }

                if (Zoom != mmaxZoom)
                {
                    Zoom = mmaxZoom;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     is polygons enabled
        /// </summary>
        public bool PolygonsEnabled = true;

        /// <summary>
        ///     is routes enabled
        /// </summary>
        public bool RoutesEnabled = true;

        /// <summary>
        ///     is markers enabled
        /// </summary>
        public bool MarkersEnabled = true;

        /// <summary>
        ///     can user drag map
        /// </summary>
        public bool CanDragMap = true;

        /// <summary>
        ///     retry count to get tile
        /// </summary>
        public int RetryLoadTile = 0;

        /// <summary>
        ///     how many levels of tiles are staying decompressed in memory
        /// </summary>
        public int LevelsKeepInMemory = 5;

        /// <summary>
        ///     map render mode
        /// </summary>
        public RenderMode RenderMode = RenderMode.GDI_PLUS;

        /// <summary>
        ///     occurs when current position is changed
        /// </summary>
        public event PositionChanged OnCurrentPositionChanged;

        /// <summary>
        ///     occurs when tile set load is complete
        /// </summary>
        public event TileLoadComplete OnTileLoadComplete;

        /// <summary>
        ///     occurs when tile set is starting to load
        /// </summary>
        public event TileLoadStart OnTileLoadStart;

        /// <summary>
        ///     occurs on empty tile displayed
        /// </summary>
        public event EmptyTileError OnEmptyTileError;

        /// <summary>
        ///     occurs on map drag
        /// </summary>
        public event MapDrag OnMapDrag;

        /// <summary>
        ///     occurs on map zoom changed
        /// </summary>
        public event MapZoomChanged OnMapZoomChanged;

        /// <summary>
        ///     occurs on map type changed
        /// </summary>
        public event MapTypeChanged OnMapTypeChanged;

        readonly List<Thread> _gThreadPool = new List<Thread>();
        // ^
        // should be only one pool for multiply controls, any ideas how to fix?
        //static readonly List<Thread> GThreadPool = new List<Thread>();

        // windows forms or wpf
        internal string SystemType;

        internal static int Instances;

        BackgroundWorker _invalidator;

        public BackgroundWorker OnMapOpen()
        {
            if (!IsStarted)
            {
                int x = Interlocked.Increment(ref Instances);
                Debug.WriteLine("OnMapOpen: " + x);

                IsStarted = true;

                if (x == 1)
                {
                    GMaps.Instance.NoMapInstances = false;
                }

                GoToCurrentPosition();

                _invalidator = new BackgroundWorker();
                _invalidator.WorkerSupportsCancellation = true;
                _invalidator.WorkerReportsProgress = true;
                _invalidator.DoWork += InvalidatorWatch;
                _invalidator.RunWorkerAsync();

                //if(x == 1)
                //{
                // first control shown
                //}
            }

            return _invalidator;
        }

        public void OnMapClose()
        {
            Dispose();
        }

        internal readonly object InvalidationLock = new object();
        internal DateTime LastInvalidation = DateTime.Now;

        void InvalidatorWatch(object sender, DoWorkEventArgs e)
        {
            var w = sender as BackgroundWorker;

            var span = TimeSpan.FromMilliseconds(111);
            int spanMs = (int)span.TotalMilliseconds;
            bool skiped = false;
            TimeSpan delta;
            DateTime now;

            while (Refresh != null && (!skiped && Refresh.WaitOne() || Refresh.WaitOne(spanMs, false) || true))
            {
                if (w.CancellationPending)
                    break;

                now = DateTime.Now;
                lock (InvalidationLock)
                {
                    delta = now - LastInvalidation;
                }

                if (delta > span)
                {
                    lock (InvalidationLock)
                    {
                        LastInvalidation = now;
                    }

                    skiped = false;

                    w.ReportProgress(1);
                    Debug.WriteLine("Invalidate delta: " + (int)delta.TotalMilliseconds + "ms");
                }
                else
                {
                    skiped = true;
                }
            }
        }

        public void UpdateCenterTileXYLocation()
        {
            var center = FromLocalToLatLng(Width / 2, Height / 2);
            var centerPixel = Provider.Projection.FromLatLngToPixel(center, Zoom);
            CenterTileXYLocation = Provider.Projection.FromPixelToTileXY(centerPixel);
        }

        internal int VWidth = 800;
        internal int VHeight = 400;

        public void OnMapSizeChanged(int width, int height)
        {
            Width = width;
            Height = height;

            if (IsRotated)
            {
                int diag = (int)Math.Round(
                    Math.Sqrt(Width * Width + Height * Height) / Provider.Projection.TileSize.Width,
                    MidpointRounding.AwayFromZero);
                _sizeOfMapArea.Width = 1 + diag / 2;
                _sizeOfMapArea.Height = 1 + diag / 2;
            }
            else
            {
                _sizeOfMapArea.Width = 1 + Width / Provider.Projection.TileSize.Width / 2;
                _sizeOfMapArea.Height = 1 + Height / Provider.Projection.TileSize.Height / 2;
            }

            Debug.WriteLine("OnMapSizeChanged, w: " + width + ", h: " + height + ", size: " + _sizeOfMapArea);

            if (IsStarted)
            {
                UpdateBounds();
                GoToCurrentPosition();
            }
        }

        /// <summary>
        ///     gets current map view top/left coordinate, width in Lng, height in Lat
        /// </summary>
        /// <returns></returns>
        public RectLatLng ViewArea
        {
            get
            {
                if (Provider.Projection != null)
                {
                    var p = FromLocalToLatLng(0, 0);
                    var p2 = FromLocalToLatLng(Width, Height);

                    return RectLatLng.FromLTRB(p.Lng, p.Lat, p2.Lng, p2.Lat);
                }

                return RectLatLng.Empty;
            }
        }

        /// <summary>
        ///     gets lat/lng from local control coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public PointLatLng FromLocalToLatLng(long x, long y)
        {
            var p = new GPoint(x, y);
            p.OffsetNegative(RenderOffset);
            p.Offset(CompensationOffset);

            return Provider.Projection.FromPixelToLatLng(p, Zoom);
        }

        /// <summary>
        ///     return local coordinates from lat/lng
        /// </summary>
        /// <param name="latlng"></param>
        /// <returns></returns>
        public GPoint FromLatLngToLocal(PointLatLng latlng)
        {
            var pLocal = Provider.Projection.FromLatLngToPixel(latlng, Zoom);
            pLocal.Offset(RenderOffset);
            pLocal.OffsetNegative(CompensationOffset);
            return pLocal;
        }

        /// <summary>
        ///     gets max zoom level to fit rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public int GetMaxZoomToFitRect(RectLatLng rect)
        {
            int zoom = MinZoom;

            if (rect.HeightLat == 0 || rect.WidthLng == 0)
            {
                zoom = MaxZoom / 2;
            }
            else
            {
                for (int i = zoom; i <= MaxZoom; i++)
                {
                    var p1 = Provider.Projection.FromLatLngToPixel(rect.LocationTopLeft, i);
                    var p2 = Provider.Projection.FromLatLngToPixel(rect.LocationRightBottom, i);

                    if (p2.X - p1.X <= Width + 10 && p2.Y - p1.Y <= Height + 10)
                    {
                        zoom = i;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return zoom;
        }

        /// <summary>
        ///     initiates map dragging
        /// </summary>
        /// <param name="pt"></param>
        public void BeginDrag(GPoint pt)
        {
            _dragPoint.X = pt.X - RenderOffset.X;
            _dragPoint.Y = pt.Y - RenderOffset.Y;
            IsDragging = true;
        }

        /// <summary>
        ///     ends map dragging
        /// </summary>
        public void EndDrag()
        {
            IsDragging = false;
            MouseDown = GPoint.Empty;

            Refresh.Set();
        }

        /// <summary>
        ///     reloads map
        /// </summary>
        public void ReloadMap()
        {
            if (IsStarted)
            {
                Debug.WriteLine("------------------");

                _okZoom = 0;
                _skipOverZoom = 0;

                CancelAsyncTasks();

                Matrix.ClearAllLevels();

                lock (FailedLoads)
                {
                    FailedLoads.Clear();
                    _raiseEmptyTileError = true;
                }

                Refresh.Set();

                UpdateBounds();
            }
            else
            {
                throw new Exception("Please, do not call ReloadMap before form is loaded, it's useless");
            }
        }

#if !NET40
        public Task ReloadMapAsync()
        {
            ReloadMap();
            return Task.Factory.StartNew(() =>
            {
                bool wait;
                do
                {
                    Thread.Sleep(100);
                    Monitor.Enter(TileLoadQueue);
                    try
                    {
                        wait = TileLoadQueue.Any();
                    }
                    finally
                    {
                        Monitor.Exit(TileLoadQueue);
                    }
                } while (wait);
            });
        }
#endif

        /// <summary>
        ///     moves current position into map center
        /// </summary>
        public void GoToCurrentPosition()
        {
            CompensationOffset = _positionPixel; // TODO: fix

            // reset stuff
            RenderOffset = GPoint.Empty;
            _dragPoint = GPoint.Empty;

            //var dd = new GPoint(-(CurrentPositionGPixel.X - Width / 2), -(CurrentPositionGPixel.Y - Height / 2));
            //dd.Offset(compensationOffset);

            var d = new GPoint(Width / 2, Height / 2);

            Drag(d);
        }

        public bool MouseWheelZooming = false;

        /// <summary>
        ///     moves current position into map center
        /// </summary>
        internal void GoToCurrentPositionOnZoom()
        {
            CompensationOffset = _positionPixel; // TODO: fix

            // reset stuff
            RenderOffset = GPoint.Empty;
            _dragPoint = GPoint.Empty;

            // goto location and centering
            if (MouseWheelZooming)
            {
                if (MouseWheelZoomType != MouseWheelZoomType.MousePositionWithoutCenter)
                {
                    var pt = new GPoint(-(_positionPixel.X - Width / 2), -(_positionPixel.Y - Height / 2));
                    pt.Offset(CompensationOffset);
                    RenderOffset.X = pt.X - _dragPoint.X;
                    RenderOffset.Y = pt.Y - _dragPoint.Y;
                }
                else // without centering
                {
                    RenderOffset.X = -_positionPixel.X - _dragPoint.X;
                    RenderOffset.Y = -_positionPixel.Y - _dragPoint.Y;
                    RenderOffset.Offset(MouseLastZoom);
                    RenderOffset.Offset(CompensationOffset);
                }
            }
            else // use current map center
            {
                MouseLastZoom = GPoint.Empty;

                var pt = new GPoint(-(_positionPixel.X - Width / 2), -(_positionPixel.Y - Height / 2));
                pt.Offset(CompensationOffset);
                RenderOffset.X = pt.X - _dragPoint.X;
                RenderOffset.Y = pt.Y - _dragPoint.Y;
            }

            UpdateCenterTileXYLocation();
        }

        /// <summary>
        ///     darg map by offset in pixels
        /// </summary>
        /// <param name="offset"></param>
        public void DragOffset(GPoint offset)
        {
            RenderOffset.Offset(offset);

            UpdateCenterTileXYLocation();

            if (CenterTileXYLocation != _centerTileXYLocationLast)
            {
                _centerTileXYLocationLast = CenterTileXYLocation;
                UpdateBounds();
            }

            {
                LastLocationInBounds = Position;

                IsDragging = true;
                Position = FromLocalToLatLng(Width / 2, Height / 2);
                IsDragging = false;
            }

            if (OnMapDrag != null)
            {
                OnMapDrag();
            }
        }

        /// <summary>
        ///     drag map
        /// </summary>
        /// <param name="pt"></param>
        public void Drag(GPoint pt)
        {
            RenderOffset.X = pt.X - _dragPoint.X;
            RenderOffset.Y = pt.Y - _dragPoint.Y;

            UpdateCenterTileXYLocation();

            if (CenterTileXYLocation != _centerTileXYLocationLast)
            {
                _centerTileXYLocationLast = CenterTileXYLocation;
                UpdateBounds();
            }

            if (IsDragging)
            {
                LastLocationInBounds = Position;
                Position = FromLocalToLatLng(Width / 2, Height / 2);

                if (OnMapDrag != null)
                {
                    OnMapDrag();
                }
            }
        }

        /// <summary>
        ///     cancels tile loaders and bounds checker
        /// </summary>
        public void CancelAsyncTasks()
        {
            if (IsStarted)
            {
#if NET40
                //TODO: clear loading
#else
                Monitor.Enter(TileLoadQueue);
                try
                {
                    TileLoadQueue.Clear();
                }
                finally
                {
                    Monitor.Exit(TileLoadQueue);
                }
#endif
            }
        }

        bool _raiseEmptyTileError;

        internal Dictionary<LoadTask, Exception> FailedLoads =
            new Dictionary<LoadTask, Exception>(new LoadTaskComparer());

        internal static readonly int WaitForTileLoadThreadTimeout = 5 * 1000 * 60; // 5 min.

        volatile int _okZoom;
        volatile int _skipOverZoom;

#if NET40
        static readonly BlockingCollection<LoadTask> TileLoadQueue4 =
            new BlockingCollection<LoadTask>(new ConcurrentStack<LoadTask>());

        static List<Task> _tileLoadQueue4Tasks;
        static int _loadWaitCount;
        void AddLoadTask(LoadTask t)
        {
            if (_tileLoadQueue4Tasks == null)
            {
                lock (TileLoadQueue4)
                {
                    if (_tileLoadQueue4Tasks == null)
                    {
                        _tileLoadQueue4Tasks = new List<Task>();

                        while (_tileLoadQueue4Tasks.Count < GThreadPoolSize)
                        {
                            Debug.WriteLine("creating ProcessLoadTask: " + _tileLoadQueue4Tasks.Count);

                            _tileLoadQueue4Tasks.Add(Task.Factory.StartNew(delegate()
                                {
                                    string ctid = "ProcessLoadTask[" + Thread.CurrentThread.ManagedThreadId + "]";
                                    Thread.CurrentThread.Name = ctid;

                                    Debug.WriteLine(ctid + ": started");
                                    do
                                    {
                                        if (TileLoadQueue4.Count == 0)
                                        {
                                            Debug.WriteLine(ctid + ": ready");

                                            if (Interlocked.Increment(ref _loadWaitCount) >= GThreadPoolSize)
                                            {
                                                Interlocked.Exchange(ref _loadWaitCount, 0);
                                                OnLoadComplete(ctid);
                                            }
                                        }

                                        ProcessLoadTask(TileLoadQueue4.Take(), ctid);
                                    } while (!TileLoadQueue4.IsAddingCompleted);

                                    Debug.WriteLine(ctid + ": exit");
                                },
                                TaskCreationOptions.LongRunning));
                        }
                    }
                }
            }

            TileLoadQueue4.Add(t);
        }
#else
        byte _loadWaitCount = 0;

        void TileLoadThread()
        {
            LoadTask? task = null;
            bool stop = false;

            var ct = Thread.CurrentThread;
            string ctid = "Thread[" + ct.ManagedThreadId + "]";
            while (!stop && IsStarted)
            {
                task = null;

                Monitor.Enter(TileLoadQueue);
                try
                {
                    while (TileLoadQueue.Count == 0)
                    {
                        Debug.WriteLine(ctid + " - Wait " + _loadWaitCount + " - " + DateTime.Now.TimeOfDay);

                        if (++_loadWaitCount >= GThreadPoolSize)
                        {
                            _loadWaitCount = 0;
                            OnLoadComplete(ctid);
                        }

                        if (!IsStarted || false == Monitor.Wait(TileLoadQueue, WaitForTileLoadThreadTimeout, false) || !IsStarted)
                        {
                            stop = true;
                            break;
                        }
                    }

                    if (IsStarted && !stop || TileLoadQueue.Count > 0)
                    {
                        task = TileLoadQueue.Pop();
                    }
                }
                finally
                {
                    Monitor.Exit(TileLoadQueue);
                }

                if (task.HasValue && IsStarted)
                {
                    ProcessLoadTask(task.Value, ctid);
                }
            }

            Monitor.Enter(TileLoadQueue);
            try
            {
                Debug.WriteLine("Quit - " + ct.Name);
                lock (_gThreadPool)
                {
                    _gThreadPool.Remove(ct);
                }
            }
            finally
            {
                Monitor.Exit(TileLoadQueue);
            }
        }
#endif

        static void ProcessLoadTask(LoadTask task, string ctid)
        {
            try
            {
                #region -- execute --

                var matrix = task.Core.Matrix;
                if (matrix == null)
                {
                    return;
                }

                var m = task.Core.Matrix.GetTileWithReadLock(task.Zoom, task.Pos);
                if (!m.NotEmpty)
                {
                    Debug.WriteLine(ctid + " - try load: " + task);

                    var t = new Tile(task.Zoom, task.Pos);

                    foreach (var tl in task.Core._provider.Overlays)
                    {
                        int retry = 0;
                        do
                        {
                            PureImage img = null;
                            Exception ex = null;

                            if (task.Zoom >= task.Core._provider.MinZoom &&
                                (!task.Core._provider.MaxZoom.HasValue || task.Zoom <= task.Core._provider.MaxZoom))
                            {
                                if (task.Core._skipOverZoom == 0 || task.Zoom <= task.Core._skipOverZoom)
                                {
                                    // tile number inversion(BottomLeft -> TopLeft)
                                    if (tl.InvertedAxisY)
                                    {
                                        img = GMaps.Instance.GetImageFrom(tl,
                                            new GPoint(task.Pos.X, task.Core._maxOfTiles.Height - task.Pos.Y),
                                            task.Zoom,
                                            out ex);
                                    }
                                    else // ok
                                    {
                                        img = GMaps.Instance.GetImageFrom(tl, task.Pos, task.Zoom, out ex);
                                    }
                                }
                            }

                            if (img != null && ex == null)
                            {
                                if (task.Core._okZoom < task.Zoom)
                                {
                                    task.Core._okZoom = task.Zoom;
                                    task.Core._skipOverZoom = 0;
                                    Debug.WriteLine("skipOverZoom disabled, okZoom: " + task.Core._okZoom);
                                }
                            }
                            else if (ex != null)
                            {
                                if (task.Core._skipOverZoom != task.Core._okZoom && task.Zoom > task.Core._okZoom)
                                {
                                    if (ex.Message.Contains("(404) Not Found"))
                                    {
                                        task.Core._skipOverZoom = task.Core._okZoom;
                                        Debug.WriteLine("skipOverZoom enabled: " + task.Core._skipOverZoom);
                                    }
                                }
                            }

                            // check for parent tiles if not found
                            if (img == null && task.Core._okZoom > 0 && task.Core.FillEmptyTiles &&
                                task.Core.Provider.Projection is MercatorProjection)
                            {
                                int zoomOffset = task.Zoom > task.Core._okZoom ? task.Zoom - task.Core._okZoom : 1;
                                long ix = 0;
                                var parentTile = GPoint.Empty;

                                while (img == null && zoomOffset < task.Zoom)
                                {
                                    ix = (long)Math.Pow(2, zoomOffset);
                                    parentTile = new GPoint(task.Pos.X / ix, task.Pos.Y / ix);
                                    img = GMaps.Instance.GetImageFrom(tl, parentTile, task.Zoom - zoomOffset++, out ex);
                                }

                                if (img != null)
                                {
                                    // offsets in quadrant
                                    long xOff = Math.Abs(task.Pos.X - parentTile.X * ix);
                                    long yOff = Math.Abs(task.Pos.Y - parentTile.Y * ix);

                                    img.IsParent = true;
                                    img.Ix = ix;
                                    img.Xoff = xOff;
                                    img.Yoff = yOff;

                                    // wpf
                                    //var geometry = new RectangleGeometry(new Rect(Core.tileRect.X + 0.6, Core.tileRect.Y + 0.6, Core.tileRect.Width + 0.6, Core.tileRect.Height + 0.6));
                                    //var parentImgRect = new Rect(Core.tileRect.X - Core.tileRect.Width * Xoff + 0.6, Core.tileRect.Y - Core.tileRect.Height * Yoff + 0.6, Core.tileRect.Width * Ix + 0.6, Core.tileRect.Height * Ix + 0.6);

                                    // gdi+
                                    //System.Drawing.Rectangle dst = new System.Drawing.Rectangle((int)Core.tileRect.X, (int)Core.tileRect.Y, (int)Core.tileRect.Width, (int)Core.tileRect.Height);
                                    //System.Drawing.RectangleF srcRect = new System.Drawing.RectangleF((float)(Xoff * (img.Img.Width / Ix)), (float)(Yoff * (img.Img.Height / Ix)), (img.Img.Width / Ix), (img.Img.Height / Ix));
                                }
                            }

                            if (img != null)
                            {
                                Debug.WriteLine(ctid + " - tile loaded: " + img.Data.Length / 1024 + "KB, " + task);
                                {
                                    t.AddOverlay(img);
                                }
                                break;
                            }
                            else
                            {
                                if (ex != null && task.Core.FailedLoads != null)
                                {
                                    lock (task.Core.FailedLoads)
                                    {
                                        if (!task.Core.FailedLoads.ContainsKey(task))
                                        {
                                            task.Core.FailedLoads.Add(task, ex);

                                            if (task.Core.OnEmptyTileError != null)
                                            {
                                                if (!task.Core._raiseEmptyTileError)
                                                {
                                                    task.Core._raiseEmptyTileError = true;
                                                    task.Core.OnEmptyTileError(task.Zoom, task.Pos);
                                                }
                                            }
                                        }
                                    }
                                }

                                if (task.Core.RetryLoadTile > 0)
                                {
                                    Debug.WriteLine(ctid + " - ProcessLoadTask: " + task + " -> empty tile, retry " +
                                                    retry);
                                    {
                                        Thread.Sleep(1111);
                                    }
                                }
                            }
                        } while (++retry < task.Core.RetryLoadTile);
                    }

                    if (t.HasAnyOverlays && task.Core.IsStarted)
                    {
                        task.Core.Matrix.SetTile(t);
                    }
                    else
                    {
                        t.Dispose();
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ctid + " - ProcessLoadTask: " + ex.ToString());
            }
            finally
            {
                if (task.Core.Refresh != null)
                {
                    task.Core.Refresh.Set();
                }
            }
        }

        void OnLoadComplete(string ctid)
        {
            _lastTileLoadEnd = DateTime.Now;
            long lastTileLoadTimeMs = (long)(_lastTileLoadEnd - _lastTileLoadStart).TotalMilliseconds;

            #region -- clear stuff--

            if (IsStarted)
            {
                GMaps.Instance.MemoryCache.RemoveOverload();

                TileDrawingListLock.AcquireReaderLock();
                try
                {
                    Matrix.ClearLevelAndPointsNotIn(Zoom, TileDrawingList);
                }
                finally
                {
                    TileDrawingListLock.ReleaseReaderLock();
                }
            }

            #endregion

            UpdateGroundResolution();
#if UseGC
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
#endif
            Debug.WriteLine(ctid + " - OnTileLoadComplete: " + lastTileLoadTimeMs + "ms, MemoryCacheSize: " +
                            GMaps.Instance.MemoryCache.Size + "MB");

            if (OnTileLoadComplete != null)
            {
                OnTileLoadComplete(lastTileLoadTimeMs);
            }
        }

        public AutoResetEvent Refresh = new AutoResetEvent(false);

        public bool UpdatingBounds;

        /// <summary>
        ///     updates map bounds
        /// </summary>
        void UpdateBounds()
        {
            if (!IsStarted || Provider.Equals(EmptyProvider.Instance))
            {
                return;
            }

            UpdatingBounds = true;

            TileDrawingListLock.AcquireWriterLock();
            try
            {
                #region -- find tiles around --

                TileDrawingList.Clear();

                for (long i = (int)Math.Floor(-_sizeOfMapArea.Width * ScaleX),
                    countI = (int)Math.Ceiling(_sizeOfMapArea.Width * ScaleX);
                    i <= countI;
                    i++)
                {
                    for (long j = (int)Math.Floor(-_sizeOfMapArea.Height * ScaleY),
                        countJ = (int)Math.Ceiling(_sizeOfMapArea.Height * ScaleY);
                        j <= countJ;
                        j++)
                    {
                        var p = CenterTileXYLocation;
                        p.X += i;
                        p.Y += j;

#if ContinuesMap
               // ----------------------------
               if(p.X < minOfTiles.Width)
               {
                  p.X += (maxOfTiles.Width + 1);
               }

               if(p.X > maxOfTiles.Width)
               {
                  p.X -= (maxOfTiles.Width + 1);
               }
               // ----------------------------
#endif

                        if (p.X >= _minOfTiles.Width && p.Y >= _minOfTiles.Height && p.X <= _maxOfTiles.Width &&
                            p.Y <= _maxOfTiles.Height)
                        {
                            var dt = new DrawTile()
                            {
                                PosXY = p,
                                PosPixel = new GPoint(p.X * TileRect.Width, p.Y * TileRect.Height),
                                DistanceSqr = (CenterTileXYLocation.X - p.X) * (CenterTileXYLocation.X - p.X) +
                                              (CenterTileXYLocation.Y - p.Y) * (CenterTileXYLocation.Y - p.Y)
                            };

                            if (!TileDrawingList.Contains(dt))
                            {
                                TileDrawingList.Add(dt);
                            }
                        }
                    }
                }

                if (GMaps.Instance.ShuffleTilesOnLoad)
                {
                    Stuff.Shuffle(TileDrawingList);
                }
                else
                {
                    TileDrawingList.Sort();
                }

                #endregion
            }
            finally
            {
                TileDrawingListLock.ReleaseWriterLock();
            }

#if NET40
            Interlocked.Exchange(ref _loadWaitCount, 0);
#else
            Monitor.Enter(TileLoadQueue);
            try
            {
#endif
            TileDrawingListLock.AcquireReaderLock();
            try
            {
                foreach (var p in TileDrawingList)
                {
                    var task = new LoadTask(p.PosXY, Zoom, this);
#if NET40
                    AddLoadTask(task);
#else
                        {
                            if (!TileLoadQueue.Contains(task))
                            {
                                TileLoadQueue.Push(task);
                            }
                        }
#endif
                }
            }
            finally
            {
                TileDrawingListLock.ReleaseReaderLock();
            }

#if !NET40
            #region -- starts loader threads if needed --

                lock (_gThreadPool)
                {
                    while (_gThreadPool.Count < GThreadPoolSize)
                    {
                        var t = new Thread(TileLoadThread);
                        {
                            t.Name = "TileLoader: " + _gThreadPool.Count;
                            t.IsBackground = true;
                            t.Priority = ThreadPriority.BelowNormal;
                        }

                        _gThreadPool.Add(t);

                        Debug.WriteLine("add " + t.Name + " to GThreadPool");

                        t.Start();
                    }
                }
            #endregion
#endif
            {
                _lastTileLoadStart = DateTime.Now;
                Debug.WriteLine("OnTileLoadStart - at zoom " + Zoom + ", time: " + _lastTileLoadStart.TimeOfDay);
            }
#if !NET40
                _loadWaitCount = 0;
                Monitor.PulseAll(TileLoadQueue);
            }
            finally
            {
                Monitor.Exit(TileLoadQueue);
            }
#endif
            UpdatingBounds = false;

            if (OnTileLoadStart != null)
            {
                OnTileLoadStart();
            }
        }

        /// <summary>
        ///     updates ground resolution info
        /// </summary>
        void UpdateGroundResolution()
        {
            double rez = Provider.Projection.GetGroundResolution(Zoom, Position.Lat);
            PxRes100M = (int)(100.0 / rez); // 100 meters
            PxRes1000M = (int)(1000.0 / rez); // 1km  
            PxRes10Km = (int)(10000.0 / rez); // 10km
            PxRes100Km = (int)(100000.0 / rez); // 100km
            PxRes1000Km = (int)(1000000.0 / rez); // 1000km
            PxRes5000Km = (int)(5000000.0 / rez); // 5000km
        }

        #region IDisposable Members

        ~Core()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (IsStarted)
            {
                if (_invalidator != null)
                {
                    _invalidator.CancelAsync();
                    _invalidator.DoWork -= InvalidatorWatch;
                    _invalidator.Dispose();
                    _invalidator = null;
                }

                if (Refresh != null)
                {
                    Refresh.Set();
                    Refresh.Close();
                    Refresh = null;
                }

                int x = Interlocked.Decrement(ref Instances);
                Debug.WriteLine("OnMapClose: " + x);

                CancelAsyncTasks();
                IsStarted = false;

                if (Matrix != null)
                {
                    Matrix.Dispose();
                    Matrix = null;
                }

                if (FailedLoads != null)
                {
                    lock (FailedLoads)
                    {
                        FailedLoads.Clear();
                        _raiseEmptyTileError = false;
                    }

                    FailedLoads = null;
                }

                TileDrawingListLock.AcquireWriterLock();
                try
                {
                    TileDrawingList.Clear();
                }
                finally
                {
                    TileDrawingListLock.ReleaseWriterLock();
                }

#if NET40
                //TODO: maybe
#else
                // cancel waiting loaders
                Monitor.Enter(TileLoadQueue);
                try
                {
                    Monitor.PulseAll(TileLoadQueue);
                }
                finally
                {
                    Monitor.Exit(TileLoadQueue);
                }
#endif

                if (TileDrawingListLock != null)
                {
                    TileDrawingListLock.Dispose();
                    TileDrawingListLock = null;
                    TileDrawingList = null;
                }

                if (x == 0)
                {
#if DEBUG
                    GMaps.Instance.CancelTileCaching();
#endif
                    GMaps.Instance.NoMapInstances = true;
                    GMaps.Instance.WaitForCache.Set();
                    if (disposing)
                    {
                        GMaps.Instance.MemoryCache.Clear();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
