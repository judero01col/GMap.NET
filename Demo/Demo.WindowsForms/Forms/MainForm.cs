using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Demo.WindowsForms.CustomMarkers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.Reflection;

namespace Demo.WindowsForms
{
    public partial class MainForm : Form
    {
        // layers
        readonly GMapOverlay _top = new GMapOverlay();
        internal readonly GMapOverlay Objects = new GMapOverlay("objects");
        internal readonly GMapOverlay Routes = new GMapOverlay("routes");
        internal readonly GMapOverlay Polygons = new GMapOverlay("polygons");

        // marker
        GMapMarker _currentMarker;

        // polygons
        GMapPolygon _polygon;

        // etc
        readonly Random _rnd = new Random();
        readonly DescendingComparer _comparerIpStatus = new DescendingComparer();
        GMapMarkerRect _curentRectMarker;
        string _mobileGpsLog = string.Empty;
        bool _isMouseDown;
        PointLatLng _start;
        PointLatLng _end;

        public MainForm()
        {
            InitializeComponent();

            if (!GMapControl.IsDesignerHosted)
            {
                // add your custom map db provider
                //MsSQLPureImageCache ch = new MsSQLPureImageCache();
                //ch.ConnectionString = @"data source = sql5040.site4now.net;User Id=DB_A3B2C9_GMapNET_admin; initial catalog = DB_A3B2C9_GMapNET; password = Usuario@2018;";                
                //MainMap.Manager.SecondaryCache = ch;

                // set your proxy here if need
                //GMapProvider.IsSocksProxy = true;
                //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                //GMapProvider.WebProxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");
                // or
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                //                          

                // set cache mode only if no internet available
                if (!Stuff.PingNetwork("google.com"))
                {
                    MainMap.Manager.Mode = AccessMode.CacheOnly;
                    MessageBox.Show("No internet connection available, going to CacheOnly mode.", "GMap.NET - Demo.WindowsForms", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                //----------------------------------------
                // Config Map at Startup
                //----------------------------------------
                MainMap.MapProvider = GMapProviders.GoogleMap;

                OpenStreetMapGraphHopperProvider.Instance.ApiKey = Stuff.OpenStreetMapsGraphHopperApiKey;
                GoogleMapProvider.Instance.ApiKey = Stuff.GoogleMapsApiKey;

                // Custom Map Provider
                //MainMap.MapProvider = GMapProviders.CustomMap;
                //GMapProviders.CustomMap.CustomServerUrl = "https://{l}.tile.openstreetmap.org/{z}/{x}/{y}.png";
                //GMapProviders.CustomMap.CustomServerLetters = "abc";

                //----------------------------------------
                // Initial Position
                //----------------------------------------
                MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783); // Lithuania, Vilnius
                MainMap.MinZoom = 0;
                MainMap.MaxZoom = 24;
                MainMap.Zoom = 9;

                textBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
                textBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);
                textBoxGeo.Text = "Lithuania, Vilnius";
              

                MainMap.ScaleMode = ScaleModes.Fractional;

                //----------------------------------------
                // Map Events
                //----------------------------------------
                MainMap.OnPositionChanged += MainMap_OnPositionChanged;
                MainMap.OnTileLoadStart += MainMap_OnTileLoadStart;
                MainMap.OnTileLoadComplete += MainMap_OnTileLoadComplete;
                MainMap.OnMapClick += MainMap_OnMapClick;

                MainMap.OnMapZoomChanged += MainMap_OnMapZoomChanged;
                MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;

                MainMap.OnMarkerClick += MainMap_OnMarkerClick;
                MainMap.OnMarkerDoubleClick += MainMap_OnMarkerDoubleClick;

                MainMap.OnMarkerEnter += MainMap_OnMarkerEnter;
                MainMap.OnMarkerLeave += MainMap_OnMarkerLeave;

                MainMap.OnPolygonEnter += MainMap_OnPolygonEnter;
                MainMap.OnPolygonLeave += MainMap_OnPolygonLeave;

                MainMap.OnPolygonClick += MainMap_OnPolygonClick;
                MainMap.OnPolygonDoubleClick += MainMap_OnPolygonDoubleClick;

                MainMap.OnRouteEnter += MainMap_OnRouteEnter;
                MainMap.OnRouteLeave += MainMap_OnRouteLeave;

                MainMap.OnRouteClick += MainMap_OnRouteClick;
                MainMap.OnRouteDoubleClick += MainMap_OnRouteDoubleClick;

                MainMap.Manager.OnTileCacheComplete += OnTileCacheComplete;
                MainMap.Manager.OnTileCacheStart += OnTileCacheStart;
                MainMap.Manager.OnTileCacheProgress += OnTileCacheProgress;

                MainMap.MouseMove += MainMap_MouseMove;
                MainMap.MouseDown += MainMap_MouseDown;
                MainMap.MouseUp += MainMap_MouseUp;
                MainMap.MouseDoubleClick += MainMap_MouseDoubleClick;

                //----------------------------------------
                // Custom Layers
                //----------------------------------------
                MainMap.Overlays.Add(Routes);
                MainMap.Overlays.Add(Polygons);
                MainMap.Overlays.Add(Objects);
                MainMap.Overlays.Add(_top);

                //----------------------------------------
                // Other Events
                //----------------------------------------
                Routes.Routes.CollectionChanged += Routes_CollectionChanged;
                Objects.Markers.CollectionChanged += Markers_CollectionChanged;

                //----------------------------------------
                // Background Workers
                //----------------------------------------

                // vehicle demo ([jokubokla]: Doesn't seem to work anymore, to be investigated)
                _transportWorker.DoWork += transport_DoWork;
                _transportWorker.ProgressChanged += transport_ProgressChanged;
                _transportWorker.WorkerSupportsCancellation = true;
                _transportWorker.WorkerReportsProgress = true;

                // performance demo
                _timerPerf.Tick += timer_Tick;

                //----------------------------------------
                // Populate Map ComboBox
                //----------------------------------------
#if !MONO       // mono doesn't handle it, so we 'lost' provider list ;]
                comboBoxMapType.ValueMember = "Name";
                comboBoxMapType.DataSource = GMapProviders.List;
                comboBoxMapType.SelectedItem = MainMap.MapProvider;
#endif
                // acccess mode
                comboBoxMode.DataSource = Enum.GetValues(typeof(AccessMode));
                comboBoxMode.SelectedItem = MainMap.Manager.Mode;

                // get cache modes
                checkBoxUseRouteCache.Checked = MainMap.Manager.UseRouteCache;

                // get zoom  
                //trackBarZoom.Minimum = MainMap.MinZoom * 100;
                //trackBarZoom.Maximum = MainMap.MaxZoom * 100;
                //trackBarZoom.TickFrequency = 100;

#if DEBUG
                checkBoxDebug.Checked = true;
                MainMap.ShowTileGridLines = checkBoxDebug.Checked;
#endif

                // set current marker
                _currentMarker = new GMarkerGoogle(MainMap.Position, GMarkerGoogleType.arrow);
                _currentMarker.IsHitTestVisible = false;
                _top.Markers.Add(_currentMarker);


                // add my city location for demo
                // [jokubokla]: The stuff down below doesn't work anymore either, but I leave it in case someone wants to fix it
                GeoCoderStatusCode status;
                var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);

                if (pos != null && status == GeoCoderStatusCode.OK)
                {
                    _currentMarker.Position = pos.Value;

                    GMapMarker myCity = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green_small);
                    myCity.ToolTipMode = MarkerTooltipMode.Always;
                    myCity.ToolTipText = "Welcome to Lithuania! ;}";
                    Objects.Markers.Add(myCity);

                    // add some more points in lithuania
                    AddLocationLithuania("Kaunas");
                    AddLocationLithuania("Klaipėda");
                    AddLocationLithuania("Šiauliai");
                    AddLocationLithuania("Panevėžys");
                }
                else
                {
                    Debug.WriteLine("GeoCoderStatusCode = " + status.ToString()); // REQUEST_DENIED I assume
                }

                if (Objects.Markers.Count > 0)
                {
                    MainMap.ZoomAndCenterMarkers(null);
                }

                RegeneratePolygon();

                try
                {
                    var overlay = DeepClone(Objects);
                    Debug.WriteLine("ISerializable status for markers: OK");

                    var overlay2 = DeepClone(Polygons);
                    Debug.WriteLine("ISerializable status for polygons: OK");

                    var overlay3 = DeepClone(Routes);
                    Debug.WriteLine("ISerializable status for routes: OK");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ISerializable failure: " + ex.Message);
#if DEBUG
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
#endif
                }
            }
        }

        // center markers on start
        private void MainForm_Load(object sender, EventArgs e)
        {
            //trackBarZoom.Value = (int)MainMap.Zoom * 100;
            Activate();
            TopMost = true;
            TopMost = false;
        }


        private void MainMap_OnMapClick(PointLatLng pointClick, MouseEventArgs e)
        {
            MainMap.FromLocalToLatLng(e.X, e.Y);
        }

        private void MainMap_OnRouteDoubleClick(GMapRoute item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnRouteClick(GMapRoute item, MouseEventArgs e)
        {
           
        }

        private void MainMap_OnPolygonDoubleClick(GMapPolygon item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            
        }

        private void MainMap_OnMarkerDoubleClick(GMapMarker item, MouseEventArgs e)
        {
            
        }

        public T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                formatter.Serialize(ms, obj);

                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        void Markers_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
        {
            //textBoxMarkerCount.Text = Objects.Markers.Count.ToString();
        }

        void Routes_CollectionChanged(object sender, GMap.NET.ObjectModel.NotifyCollectionChangedEventArgs e)
        {
            //textBoxRouteCount.Text = Routes.Routes.Count.ToString();
        }

        #region -- performance test --

        double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        int _tt;
        void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(_rnd, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(_rnd, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
            GMapMarker m = new GMarkerGoogle(pos, GMarkerGoogleType.green_pushpin);
            {
                m.ToolTipText = (_tt++).ToString();
                m.ToolTipMode = MarkerTooltipMode.Always;
            }

            Objects.Markers.Add(m);

            if (_tt >= 333)
            {
                _timerPerf.Stop();
                _tt = 0;
            }
        }

        System.Windows.Forms.Timer _timerPerf = new System.Windows.Forms.Timer();
        #endregion


        #region -- transport demo --

        // [jokubokla]: The transport demo doesn't seem to work. Presumably because a public transportation
        // webservice in Vilnius has a new API

        BackgroundWorker _transportWorker = new BackgroundWorker();


        readonly List<VehicleData> _trolleybus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _trolleybusMarkers = new Dictionary<int, GMapMarker>();

        readonly List<VehicleData> _bus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _busMarkers = new Dictionary<int, GMapMarker>();

        bool _firstLoadTrasport = true;
        GMapMarker _currentTransport;

        void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // stops immediate marker/route/polygon invalidations;
            // call Refresh to perform single refresh and reset invalidation state
            MainMap.HoldInvalidation = true;

            lock (_trolleybus)
            {
                foreach (var d in _trolleybus)
                {
                    GMapMarker marker;

                    if (!_trolleybusMarkers.TryGetValue(d.Id, out marker))
                    {
                        marker = new GMarkerGoogle(new PointLatLng(d.Lat, d.Lng), GMarkerGoogleType.red_small);
                        marker.Tag = d.Id;
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                        _trolleybusMarkers[d.Id] = marker;
                        Objects.Markers.Add(marker);
                    }
                    else
                    {
                        marker.Position = new PointLatLng(d.Lat, d.Lng);
                        //(marker as GMarkerGoogle).Bearing = (float?) d.Bearing;
                    }
                    marker.ToolTipText = "Trolley " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

                    if (_currentTransport != null && _currentTransport == marker)
                    {
                        MainMap.Position = marker.Position;
                        if (d.Bearing.HasValue)
                        {
                            MainMap.Bearing = (float)d.Bearing.Value;
                        }
                    }
                }
            }

            lock (_bus)
            {
                foreach (var d in _bus)
                {
                    GMapMarker marker;

                    if (!_busMarkers.TryGetValue(d.Id, out marker))
                    {
                        marker = new GMarkerGoogle(new PointLatLng(d.Lat, d.Lng), GMarkerGoogleType.green_small);
                        marker.Tag = d.Id;
                        marker.ToolTipMode = MarkerTooltipMode.OnMouseOver;

                        _busMarkers[d.Id] = marker;
                        Objects.Markers.Add(marker);
                    }
                    else
                    {
                        marker.Position = new PointLatLng(d.Lat, d.Lng);
                        //(marker as GMarkerGoogle).Bearing = (float?) d.Bearing;
                    }
                    marker.ToolTipText = "Bus " + d.Line + (d.Bearing.HasValue ? ", bearing: " + d.Bearing.Value.ToString() : string.Empty) + ", " + d.Time;

                    if (_currentTransport != null && _currentTransport == marker)
                    {
                        MainMap.Position = marker.Position;
                        if (d.Bearing.HasValue)
                        {
                            MainMap.Bearing = (float)d.Bearing.Value;
                        }
                    }
                }
            }

            if (_firstLoadTrasport)
            {
                MainMap.Zoom = 5;
                MainMap.ZoomAndCenterMarkers("objects");
                _firstLoadTrasport = false;
            }
            MainMap.Refresh();
        }

        void transport_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_transportWorker.CancellationPending)
            {
                try
                {
                    #region -- old vehicle demo --
                    lock (_trolleybus)
                    {
                        Stuff.GetVilniusTransportData(TransportType.TrolleyBus, string.Empty, _trolleybus);
                    }

                    lock (_bus)
                    {
                        Stuff.GetVilniusTransportData(TransportType.Bus, string.Empty, _bus);
                    }
                    #endregion

                    _transportWorker.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("transport_DoWork: " + ex.ToString());
                }
                Thread.Sleep(2 * 1000);
            }

            _trolleybusMarkers.Clear();
            _busMarkers.Clear();
        }

        #endregion


        #region -- some functions --

        void RegeneratePolygon()
        {
            var polygonPoints = new List<PointLatLng>();

            foreach (var m in Objects.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    m.Tag = polygonPoints.Count;
                    polygonPoints.Add(m.Position);
                }
            }

            if (_polygon == null)
            {
                _polygon = new GMapPolygon(polygonPoints, "polygon test");
                _polygon.IsHitTestVisible = true;
                Polygons.Polygons.Add(_polygon);
            }
            else
            {
                _polygon.Points.Clear();
                _polygon.Points.AddRange(polygonPoints);

                if (Polygons.Polygons.Count == 0)
                {
                    Polygons.Polygons.Add(_polygon);
                }
                else
                {
                    MainMap.UpdatePolygonLocalPosition(_polygon);
                }
            }
        }


        /// <summary>
        /// adds marker using geocoder
        /// </summary>
        /// <param name="place"></param>
        void AddLocationLithuania(string place)
        {
            GeoCoderStatusCode status;
            var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, " + place, out status);
            if (pos != null && status == GeoCoderStatusCode.OK)
            {
                var m = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green);
                m.ToolTip = new GMapRoundedToolTip(m);

                var mBorders = new GMapMarkerRect(pos.Value);
                {
                    mBorders.InnerMarker = m;
                    mBorders.ToolTipText = place;
                    mBorders.ToolTipMode = MarkerTooltipMode.Always;
                }

                Objects.Markers.Add(m);
                Objects.Markers.Add(mBorders);
            }
        }

        bool TryExtractLeafletjs()
        {
            try
            {
                string launch = string.Empty;

                string[] x = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                foreach (string f in x)
                {
                    if (f.Contains("leafletjs"))
                    {
                        string fName = f.Replace("Demo.WindowsForms.", string.Empty);
                        fName = fName.Replace(".", "\\");
                        int ll = fName.LastIndexOf("\\");
                        string name = fName.Substring(0, ll) + "." + fName.Substring(ll + 1, fName.Length - ll - 1);

                        //Demo.WindowsForms.leafletjs.dist.leaflet.js

                        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(f))
                        {
                            string fileFullPath = MainMap.CacheLocation + name;

                            if (fileFullPath.Contains("gmap.html"))
                            {
                                launch = fileFullPath;
                            }

                            string dir = Path.GetDirectoryName(fileFullPath);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            using (var fileStream = File.Create(fileFullPath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data
                                byte[] bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);

                                // Use FileStream object to write to the specified file
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(launch))
                {
                    Process.Start(launch);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TryExtractLeafletjs: " + ex);
                return false;
            }
            return true;
        }

        #endregion


        #region -- cache events --

        void OnTileCacheComplete()
        {
            Debug.WriteLine("OnTileCacheComplete");
            long size = 0;
            int db = 0;
            try
            {
                var di = new DirectoryInfo(MainMap.CacheLocation);
                var dbs = di.GetFiles("*.gmdb", SearchOption.AllDirectories);
                foreach (var d in dbs)
                {
                    size += d.Length;
                    db++;
                }
            }
            catch
            {
            }

            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    //textBoxCacheSize.Text = string.Format(CultureInfo.InvariantCulture, "{0} db in {1:00} MB", db, size / (1024.0 * 1024.0));
                    progressBar1.Visible = false;
                    groupBoxProgress.Text = "all tiles saved!";
                };

                if (!IsDisposed)
                {
                    try
                    {
                        Invoke(m);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        void OnTileCacheStart()
        {
            Debug.WriteLine("OnTileCacheStart");

            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    progressBar1.Visible = true;
                    progressBar1.Value = progressBar1.Minimum;
                    groupBoxProgress.Text = "saving tiles...";
                };
                Invoke(m);
            }
        }

        void OnTileCacheProgress(int left)
        {
            if (!IsDisposed)
            {
                MethodInvoker m = delegate
                {
                    progressBar1.Visible = true;
                    progressBar1.Value = (progressBar1.Value < progressBar1.Maximum) ?  (progressBar1.Value + 1) : progressBar1.Minimum;
                    groupBoxProgress.Text = left + " tiles to save...";
                };
                Invoke(m);
            }
        }

        #endregion


        #region -- map events --


        void MainMap_OnMarkerLeave(GMapMarker item)
        {
            if (item is GMapMarkerRect)
            {
                _curentRectMarker = null;

                var rc = item as GMapMarkerRect;
                rc.Pen.Color = Color.Blue;

                Debug.WriteLine("OnMarkerLeave: " + item.Position);
            }
        }

        void MainMap_OnMarkerEnter(GMapMarker item)
        {
            if (item is GMapMarkerRect)
            {
                var rc = item as GMapMarkerRect;
                rc.Pen.Color = Color.Red;

                _curentRectMarker = rc;
            }
            Debug.WriteLine("OnMarkerEnter: " + item.Position);
        }

        GMapPolygon _currentPolygon;
        void MainMap_OnPolygonLeave(GMapPolygon item)
        {
            _currentPolygon = null;
            item.Stroke.Color = Color.MidnightBlue;
            Debug.WriteLine("OnPolygonLeave: " + item.Name);
        }

        void MainMap_OnPolygonEnter(GMapPolygon item)
        {
            _currentPolygon = item;
            item.Stroke.Color = Color.Red;
            Debug.WriteLine("OnPolygonEnter: " + item.Name);
        }

        GMapRoute _currentRoute;
        void MainMap_OnRouteLeave(GMapRoute item)
        {
            _currentRoute = null;
            item.Stroke.Color = Color.MidnightBlue;
            Debug.WriteLine("OnRouteLeave: " + item.Name);
        }

        void MainMap_OnRouteEnter(GMapRoute item)
        {
            _currentRoute = item;
            item.Stroke.Color = Color.Red;
            Debug.WriteLine("OnRouteEnter: " + item.Name);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            comboBoxMapType.SelectedItem = type;

            //trackBarZoom.Minimum = MainMap.MinZoom * 100;
            //trackBarZoom.Maximum = MainMap.MaxZoom * 100;

        }

        void MainMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = false;
            }
        }

        // add demo circle
        void MainMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cc = new GMapMarkerCircle(MainMap.FromLocalToLatLng(e.X, e.Y));
            Objects.Markers.Add(cc);
        }

        void MainMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;

                if (_currentMarker.IsVisible)
                {
                    _currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);

                    var px = MainMap.MapProvider.Projection.FromLatLngToPixel(_currentMarker.Position.Lat, _currentMarker.Position.Lng, (int)MainMap.Zoom);
                    var tile = MainMap.MapProvider.Projection.FromPixelToTileXY(px);

                    Debug.WriteLine("MouseDown: geo: " + _currentMarker.Position + " | px: " + px + " | tile: " + tile);
                }
            }
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _isMouseDown)
            {
                if (_curentRectMarker == null)
                {
                    if (_currentMarker.IsVisible)
                    {
                        _currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
                    }
                }
                else // move rect marker
                {
                    var pnew = MainMap.FromLocalToLatLng(e.X, e.Y);

                    int? pIndex = (int?)_curentRectMarker.Tag;
                    if (pIndex.HasValue)
                    {
                        if (pIndex < _polygon.Points.Count)
                        {
                            _polygon.Points[pIndex.Value] = pnew;
                            MainMap.UpdatePolygonLocalPosition(_polygon);
                        }
                    }

                    if (_currentMarker.IsVisible)
                    {
                        _currentMarker.Position = pnew;
                    }
                    _curentRectMarker.Position = pnew;

                    if (_curentRectMarker.InnerMarker != null)
                    {
                        _curentRectMarker.InnerMarker.Position = pnew;
                    }
                }

                MainMap.Refresh(); // force instant invalidation
            }
        }

        // MapZoomChanged
        void MainMap_OnMapZoomChanged()
        {
            //trackBarZoom.Value = (int)(MainMap.Zoom * 100.0);
            //textBoxZoomCurrent.Text = MainMap.Zoom.ToString();
        }

        // click on some marker
        void MainMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (item is GMapMarkerRect)
                {
                    GeoCoderStatusCode status;
                    var pos = GMapProviders.GoogleMap.GetPlacemark(item.Position, out status);
                    if (status == GeoCoderStatusCode.OK && pos != null)
                    {
                        var v = item as GMapMarkerRect;
                        {
                            v.ToolTipText = pos.Value.Address;
                        }
                        MainMap.Invalidate(false);
                    }
                }
                else
                {
                    if (item.Tag != null)
                    {
                        if (_currentTransport != null)
                        {
                            _currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                            _currentTransport = null;
                        }
                        _currentTransport = item;
                        _currentTransport.ToolTipMode = MarkerTooltipMode.Always;
                    }
                }
            }
        }

        // loader start loading tiles
        void MainMap_OnTileLoadStart()
        {
            MethodInvoker m = delegate ()
            {
                // HACK JKU CLEAN
                //panelMenu.Text = "Menu: loading tiles...";
            };
            try
            {
                BeginInvoke(m);
            }
            catch
            {
            }
        }

        // loader end loading tiles
        void MainMap_OnTileLoadComplete(long elapsedMilliseconds)
        {
            // HACK JKU CLEAN
            //MainMap.ElapsedMilliseconds = elapsedMilliseconds;

            MethodInvoker m = delegate ()
            {
                
                // HACK JKU CLEAN
                //panelMenu.Text = "Menu, last load in " + MainMap.ElapsedMilliseconds + "ms";
                //textBoxMemory.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.00} MB of {1:0.00} MB", MainMap.Manager.MemoryCache.Size, MainMap.Manager.MemoryCache.Capacity);
            };
            try
            {
                BeginInvoke(m);
            }
            catch
            {
            }
        }

        // current point changed
        void MainMap_OnPositionChanged(PointLatLng point)
        {
            //textBoxLatCurrent.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
            //textBoxLngCurrent.Text = point.Lng.ToString(CultureInfo.InvariantCulture);

        }

        #endregion


        #region -- ui events --

        bool _userAcceptedLicenseOnce;

        // change map type
        private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
        {
            if (!_userAcceptedLicenseOnce)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
                {
                    string txt = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
                    
                    var d = new Demo.WindowsForms.Forms.Message();
                    d.richTextBox1.Text = txt;

                    if (DialogResult.Yes == d.ShowDialog())
                    {
                        _userAcceptedLicenseOnce = true;
                        Text += " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
                    }
                }
                else
                {
                    // user deleted License.txt ;}
                    _userAcceptedLicenseOnce = true;
                }
            }

            if (_userAcceptedLicenseOnce)
            {
                MainMap.MapProvider = comboBoxMapType.SelectedItem as GMapProvider;
            }
            else
            {
                MainMap.MapProvider = GMapProviders.OpenStreetMap;
                comboBoxMapType.SelectedItem = MainMap.MapProvider;
            }
        }

        // change mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedValue;
            MainMap.ReloadMap();
        }

        // goto by geocoder
        private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((Keys)e.KeyChar == Keys.Enter)
            {
                var status = MainMap.SetPositionByKeywords(textBoxGeo.Text);

                if (status != GeoCoderStatusCode.OK)
                {
                    MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        // go to
        private void btnGoTo_Click(object sender, EventArgs e)
        {
            var status = MainMap.SetPositionByKeywords(textBoxGeo.Text);

            if (status != GeoCoderStatusCode.OK)
            {
                MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + status.ToString(), "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // reload map
        private void btnReload_Click(object sender, EventArgs e)
        {
            MainMap.ReloadMap();
        }

        // cache config
        private void checkBoxUseCache_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.Manager.UseRouteCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UseGeocoderCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UsePlacemarkCache = checkBoxUseRouteCache.Checked;
            MainMap.Manager.UseDirectionsCache = checkBoxUseRouteCache.Checked;
        }

        // clear cache
        private void btnCacheClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                try
                {
                    MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                    MessageBox.Show("Done. Cache is clear.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // add test route
        private void btnAddRoute_Click(object sender, EventArgs e)
        {
            var rp = MainMap.RoutingProvider;

            var route = rp.GetRoute(_start, _end, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                // add route
                var r = new GMapRoute(route.Points, route.Name);
                r.IsHitTestVisible = true;
                Routes.Routes.Add(r);

                // add route start/end marks
                GMapMarker m1 = new GMarkerGoogle(_start, GMarkerGoogleType.green_big_go);
                m1.ToolTipText = "Start: " + route.Name;
                m1.ToolTipMode = MarkerTooltipMode.Always;

                GMapMarker m2 = new GMarkerGoogle(_end, GMarkerGoogleType.red_big_stop);
                m2.ToolTipText = "End: " + _end.ToString();
                m2.ToolTipMode = MarkerTooltipMode.Always;

                Objects.Markers.Add(m1);
                Objects.Markers.Add(m2);

                MainMap.ZoomAndCenterRoute(r);
            }
        }

        // add marker on current position
        private void btnAddMarker_Click(object sender, EventArgs e)
        {
            var m = new GMarkerGoogle(_currentMarker.Position, GMarkerGoogleType.green_pushpin);
            var mBorders = new GMapMarkerRect(_currentMarker.Position);
            {
                mBorders.InnerMarker = m;
                if (_polygon != null)
                {
                    mBorders.Tag = _polygon.Points.Count;
                }
                mBorders.ToolTipMode = MarkerTooltipMode.Always;
            }

            Placemark? p = null;
            if (checkBoxPlacemarkInfo.Checked)
            {
                GeoCoderStatusCode status;
                var ret = GMapProviders.GoogleMap.GetPlacemark(_currentMarker.Position, out status);
                if (status == GeoCoderStatusCode.OK && ret != null)
                {
                    p = ret;
                }
            }

            if (p != null)
            {
                mBorders.ToolTipText = p.Value.Address;
            }
            else
            {
                mBorders.ToolTipText = _currentMarker.Position.ToString();
            }

            Objects.Markers.Add(m);
            Objects.Markers.Add(mBorders);

            RegeneratePolygon();
        }

        // clear markers, routes and polygons
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            Objects.Markers.Clear();
            Routes.Routes.Clear();
            Polygons.Polygons.Clear();
        }

        // show current marker
        private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
        {
            _currentMarker.IsVisible = checkBoxCurrentMarker.Checked;
        }

        // can drag
        private void checkBoxCanDrag_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.CanDragMap = checkBoxCanDrag.Checked;
        }

        // set route start
        private void btnSetStart_Click(object sender, EventArgs e)
        {
            _start = _currentMarker.Position;
        }

        // set route end
        private void btnSetEnd_Click(object sender, EventArgs e)
        {
            _end = _currentMarker.Position;
        }

        // zoom to max for markers
        private void btnZoomCenter_Click(object sender, EventArgs e)
        {
            MainMap.ZoomAndCenterMarkers("objects");
        }

        // export map data
        private void btnExport_Click(object sender, EventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import map data
        private void btnImport_Click(object sender, EventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // prefetch
        private void btnCachePrefetch_Click(object sender, EventArgs e)
        {
            var area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    var res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButtons.YesNoCancel);

                    if (res == DialogResult.Yes)
                    {
                        using (var obj = new TilePrefetcher())
                        {
                            obj.Overlay = Objects; // set overlay if you want to see cache progress on the map

                            obj.Shuffle = MainMap.Manager.Mode != AccessMode.CacheOnly;

                            obj.Owner = this;
                            obj.ShowCompleteMessage = true;
                            obj.Start(area, i, MainMap.MapProvider, MainMap.Manager.Mode == AccessMode.CacheOnly ? 0 : 100, MainMap.Manager.Mode == AccessMode.CacheOnly ? 0 : 1);
                        }
                    }
                    else if (res == DialogResult.No)
                    {
                        continue;
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // debug tile grid
        private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
        {
            MainMap.ShowTileGridLines = checkBoxDebug.Checked;
        }

        // launch static map maker
        private void btnSave_Click(object sender, EventArgs e)
        {
            var st = new StaticImage(this);
            st.Owner = this;
            st.Show();
        }

        // key-up events
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            int offset = -22;

            if (e.KeyCode == Keys.Left)
            {
                MainMap.Offset(-offset, 0);
            }
            else if (e.KeyCode == Keys.Right)
            {
                MainMap.Offset(offset, 0);
            }
            else if (e.KeyCode == Keys.Up)
            {
                MainMap.Offset(0, -offset);
            }
            else if (e.KeyCode == Keys.Down)
            {
                MainMap.Offset(0, offset);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (_currentPolygon != null)
                {
                    Polygons.Polygons.Remove(_currentPolygon);
                    _currentPolygon = null;
                }

                if (_currentRoute != null)
                {
                    Routes.Routes.Remove(_currentRoute);
                    _currentRoute = null;
                }

                if (_curentRectMarker != null)
                {
                    Objects.Markers.Remove(_curentRectMarker);

                    if (_curentRectMarker.InnerMarker != null)
                    {
                        Objects.Markers.Remove(_curentRectMarker.InnerMarker);
                    }
                    _curentRectMarker = null;

                    RegeneratePolygon();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                MainMap.Bearing = 0;

                if (_currentTransport != null && !MainMap.IsMouseOverMarker)
                {
                    _currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    _currentTransport = null;
                }
            }
        }

        // key-press events
        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (MainMap.Focused)
            {
                if (e.KeyChar == '+')
                {
                    MainMap.Zoom = ((int)MainMap.Zoom) + 1;
                }
                else if (e.KeyChar == '-')
                {
                    MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
                }
                else if (e.KeyChar == 'a')
                {
                    MainMap.Bearing--;
                }
                else if (e.KeyChar == 'z')
                {
                    MainMap.Bearing++;
                }
            }
        }

        // engage some live demo
        private void RealTimeChanged(object sender, EventArgs e)
        {
            Objects.Markers.Clear();
            Polygons.Polygons.Clear();
            Routes.Routes.Clear();

            // start performance test
            if (radioButtonPerf.Checked)
            {
                _timerPerf.Interval = 44;
                _timerPerf.Start();
            }
            else
            {
                // stop performance test
                _timerPerf.Stop();
            }

            // vehicle demo
            if (radioButtonVehicle.Checked)
            {
                if (!_transportWorker.IsBusy)
                {
                    _firstLoadTrasport = true;
                    _transportWorker.RunWorkerAsync();
                }
            }
            else
            {
                if (_transportWorker.IsBusy)
                {
                    _transportWorker.CancelAsync();
                }
            }
        }

        // http://leafletjs.com/
        // Leaflet is a modern open-source JavaScript library for mobile-friendly interactive maps
        // Leaflet is designed with simplicity, performance and usability in mind. It works efficiently across all major desktop and mobile platforms out of the box
        private void checkBoxTileHost_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxTileHost.Checked)
            {
                try
                {
                    MainMap.Manager.EnableTileHost(8844);
                    TryExtractLeafletjs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("EnableTileHost: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MainMap.Manager.DisableTileHost();
            }
        }

        #endregion

        #region -- UNUSED CODE --

        // zoom
        private void UNUSED_trackBarZoom_ValueChanged(object sender, EventArgs e)
        {
            //MainMap.Zoom = trackBarZoom.Value / 100.0;
        }

        private void UNUSED_btnZoomUp_Click(object sender, EventArgs e)
        {
            MainMap.Zoom = ((int)MainMap.Zoom) + 1;
        }

        private void UNUSED_btnZoomDown_Click(object sender, EventArgs e)
        {
            MainMap.Zoom = ((int)(MainMap.Zoom + 0.99)) - 1;
        }

        // saves current map view 
        private void UNUSED_btnSaveCurrentMapView_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PNG (*.png)|*.png";
                    sfd.FileName = "GMap.NET image";

                    var tmpImage = MainMap.ToImage();
                    if (tmpImage != null)
                    {
                        using (tmpImage)
                        {
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                tmpImage.Save(sfd.FileName);

                                MessageBox.Show("Image saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Image failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // load gpx file
        private void UNUSED_btnLoadGpxFile(object sender, EventArgs e)
        {
            using (FileDialog dlg = new OpenFileDialog())
            {
                dlg.CheckPathExists = true;
                dlg.CheckFileExists = false;
                dlg.AddExtension = true;
                dlg.DefaultExt = "gpx";
                dlg.ValidateNames = true;
                dlg.Title = "GMap.NET: open gpx log";
                dlg.Filter = "gpx files (*.gpx)|*.gpx";
                dlg.FilterIndex = 1;
                dlg.RestoreDirectory = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string gpx = File.ReadAllText(dlg.FileName);

                        var r = MainMap.Manager.DeserializeGPX(gpx);
                        if (r != null)
                        {
                            if (r.trk.Length > 0)
                            {
                                foreach (var trk in r.trk)
                                {
                                    var points = new List<PointLatLng>();

                                    foreach (var seg in trk.trkseg)
                                    {
                                        foreach (var p in seg.trkpt)
                                        {
                                            points.Add(new PointLatLng((double)p.lat, (double)p.lon));
                                        }
                                    }

                                    var rt = new GMapRoute(points, string.Empty);
                                    {
                                        rt.Stroke = new Pen(Color.FromArgb(144, Color.Red));
                                        rt.Stroke.Width = 5;
                                        rt.Stroke.DashStyle = DashStyle.DashDot;
                                    }
                                    Routes.Routes.Add(rt);
                                }

                                MainMap.ZoomAndCenterRoutes(null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("GPX import: " + ex.ToString());
                        MessageBox.Show("Error importing gpx: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }



        // open disk cache location
        private void UNUSED_btnOpenDiskCacheLocation_Click(object sender, EventArgs e)
        {
            try
            {
                string argument = "/select, \"" + MainMap.CacheLocation + "TileDBv5\"";
                Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // export mobile gps log to gpx file
        /*
        private void buttonExportToGpx_Click(object sender, EventArgs e)
        {
            try
            {
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "GPX (*.gpx)|*.gpx";
                    sfd.FileName = "mobile gps log";

                    DateTime? date = null;
                    DateTime? dateEnd = null;

                    if (MobileLogFrom.Checked)
                    {
                        date = MobileLogFrom.Value.ToUniversalTime();

                        sfd.FileName += " from " + MobileLogFrom.Value.ToString("yyyy-MM-dd HH-mm");
                    }

                    if (MobileLogTo.Checked)
                    {
                        dateEnd = MobileLogTo.Value.ToUniversalTime();

                        sfd.FileName += " to " + MobileLogTo.Value.ToString("yyyy-MM-dd HH-mm");
                    }

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        var log = Stuff.GetRoutesFromMobileLog(_mobileGpsLog, date, dateEnd, 3.3);
                        if (log != null)
                        {
                            if (MainMap.Manager.ExportGPX(log, sfd.FileName))
                            {
                                MessageBox.Show("GPX saved: " + sfd.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GPX failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        */


        #endregion


    }
}
