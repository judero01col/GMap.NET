using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Demo.WindowsForms;
using Demo.WindowsPresentation.CustomMarkers;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation
{
    public partial class MainWindow : Window
    {
        PointLatLng _start;
        PointLatLng _end;

        // marker
        GMapMarker currentMarker;

        // zones list
        List<GMapMarker> Circles = new List<GMapMarker>();

        public MainWindow()
        {
            InitializeComponent();

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.SecondaryCache = ch;

            // set your proxy here if need
            //GMapProvider.IsSocksProxy = true;
            //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
            //GMapProvider.WebProxy.Credentials = new NetworkCredential("ogrenci@bilgeadam.com", "bilgeada");
            // or
            //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
            //

            // set cache mode only if no internet avaible
            if (!Stuff.PingNetwork("pingtest.com"))
            {
                MainMap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection available, going to CacheOnly mode.",
                    "GMap.NET - Demo.WindowsPresentation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            GoogleMapProvider.Instance.ApiKey = Stuff.GoogleMapsApiKey;

            // config map
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);

            //MainMap.ScaleMode = ScaleModes.Dynamic;

            // map events
            MainMap.OnPositionChanged += MainMap_OnCurrentPositionChanged;
            MainMap.OnTileLoadComplete += MainMap_OnTileLoadComplete;
            MainMap.OnTileLoadStart += MainMap_OnTileLoadStart;
            MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;
            MainMap.MouseMove += MainMap_MouseMove;
            MainMap.MouseLeftButtonDown += MainMap_MouseLeftButtonDown;
            MainMap.MouseEnter += MainMap_MouseEnter;

            // get map types
            ComboBoxMapType.ItemsSource = GMapProviders.List;
            ComboBoxMapType.DisplayMemberPath = "Name";
            ComboBoxMapType.SelectedItem = MainMap.MapProvider;

            // acccess mode
            ComboBoxMode.ItemsSource = Enum.GetValues(typeof(AccessMode));
            ComboBoxMode.SelectedItem = MainMap.Manager.Mode;

            // get cache modes
            CheckBoxCacheRoute.IsChecked = MainMap.Manager.UseRouteCache;
            CheckBoxGeoCache.IsChecked = MainMap.Manager.UseGeocoderCache;

            // setup zoom min/max
            SliderZoom.Maximum = MainMap.MaxZoom;
            SliderZoom.Minimum = MainMap.MinZoom;

            // get position
            TextBoxLat.Text = MainMap.Position.Lat.ToString(CultureInfo.InvariantCulture);
            TextBoxLng.Text = MainMap.Position.Lng.ToString(CultureInfo.InvariantCulture);

            // get marker state
            CheckBoxCurrentMarker.IsChecked = true;

            // can drag map
            CheckBoxDragMap.IsChecked = MainMap.CanDragMap;

#if DEBUG
            CheckBoxDebug.IsChecked = true;
#endif

            //validator.Window = this;

            // set current marker
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "custom position marker");
                currentMarker.Offset = new Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }

            //if(false)
            {
                // add my city location for demo
                GeoCoderStatusCode status;

                var city = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius", out status);
                if (city != null && status == GeoCoderStatusCode.OK)
                {
                    var it = new GMapMarker(city.Value);
                    {
                        it.ZIndex = 55;
                        it.Shape = new CustomMarkerDemo(this, it, "Welcome to Lithuania! ;}");
                    }
                    MainMap.Markers.Add(it);

                    #region -- add some markers and zone around them --

                    //if(false)
                    {
                        var objects = new List<PointAndInfo>();
                        {
                            string area = "Antakalnis";
                            var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.OK)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Senamiestis";
                            var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.OK)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        {
                            string area = "Pilaite";
                            var pos = GMapProviders.GoogleMap.GetPoint("Lithuania, Vilnius, " + area, out status);
                            if (pos != null && status == GeoCoderStatusCode.OK)
                            {
                                objects.Add(new PointAndInfo(pos.Value, area));
                            }
                        }
                        AddDemoZone(8.8, city.Value, objects);
                    }

                    #endregion
                }

                if (MainMap.Markers.Count > 1)
                {
                    MainMap.ZoomAndCenterMarkers(null);
                }
            }

            // perfromance test
            timer.Interval = TimeSpan.FromMilliseconds(44);
            timer.Tick += timer_Tick;

            // transport demo
            transport.DoWork += transport_DoWork;
            transport.ProgressChanged += transport_ProgressChanged;
            transport.WorkerSupportsCancellation = true;
            transport.WorkerReportsProgress = true;
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        #region -- performance test--

        public RenderTargetBitmap ToImageSource(FrameworkElement obj)
        {
            // Save current canvas transform
            var transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            var margin = obj.Margin;
            obj.Margin = new Thickness(0, 0, margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            var size = new Size(obj.Width, obj.Height);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            var bmp = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(obj);

            if (bmp.CanFreeze)
            {
                bmp.Freeze();
            }

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }

        double NextDouble(Random rng, double min, double max)
        {
            return min + rng.NextDouble() * (max - min);
        }

        Random r = new Random();

        int _tt;

        void timer_Tick(object sender, EventArgs e)
        {
            var pos = new PointLatLng(NextDouble(r, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom),
                NextDouble(r, MainMap.ViewArea.Left, MainMap.ViewArea.Right));
            var m = new GMapMarker(pos);
            {
                var s = new Test((_tt++).ToString());

                var image = new Image();
                {
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.LowQuality);
                    image.Stretch = Stretch.None;
                    image.Opacity = s.Opacity;

                    image.MouseEnter += image_MouseEnter;
                    image.MouseLeave += image_MouseLeave;

                    image.Source = ToImageSource(s);
                }

                m.Shape = image;

                m.Offset = new Point(-s.Width, -s.Height);
            }
            MainMap.Markers.Add(m);

            if (_tt >= 333)
            {
                timer.Stop();
                _tt = 0;
            }
        }

        void image_MouseLeave(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            img.RenderTransform = null;
        }

        void image_MouseEnter(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            img.RenderTransform = new ScaleTransform(1.2, 1.2, 12.5, 12.5);
        }

        DispatcherTimer timer = new DispatcherTimer();

        #endregion

        #region -- transport demo --

        BackgroundWorker transport = new BackgroundWorker();

        readonly List<VehicleData> _trolleybus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _trolleybusMarkers = new Dictionary<int, GMapMarker>();

        readonly List<VehicleData> _bus = new List<VehicleData>();
        readonly Dictionary<int, GMapMarker> _busMarkers = new Dictionary<int, GMapMarker>();

        bool _firstLoadTrasport = true;

        void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            using (Dispatcher.DisableProcessing())
            {
                lock (_trolleybus)
                {
                    foreach (var d in _trolleybus)
                    {
                        GMapMarker marker;

                        if (!_trolleybusMarkers.TryGetValue(d.Id, out marker))
                        {
                            marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                            marker.Tag = d.Id;
                            marker.Shape = new CircleVisual(marker, Brushes.Red);

                            _trolleybusMarkers[d.Id] = marker;
                            MainMap.Markers.Add(marker);
                        }
                        else
                        {
                            marker.Position = new PointLatLng(d.Lat, d.Lng);
                            var shape = marker.Shape as CircleVisual;
                            {
                                shape.Text = d.Line;
                                shape.Angle = d.Bearing;
                                shape.Tooltip.SetValues("TrolleyBus", d);

                                if (shape.IsChanged)
                                {
                                    shape.UpdateVisual(false);
                                }
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
                            marker = new GMapMarker(new PointLatLng(d.Lat, d.Lng));
                            marker.Tag = d.Id;

                            var v = new CircleVisual(marker, Brushes.Blue);
                            {
                                v.Stroke = new Pen(Brushes.Gray, 2.0);
                            }
                            marker.Shape = v;

                            _busMarkers[d.Id] = marker;
                            MainMap.Markers.Add(marker);
                        }
                        else
                        {
                            marker.Position = new PointLatLng(d.Lat, d.Lng);
                            var shape = marker.Shape as CircleVisual;
                            {
                                shape.Text = d.Line;
                                shape.Angle = d.Bearing;
                                shape.Tooltip.SetValues("Bus", d);

                                if (shape.IsChanged)
                                {
                                    shape.UpdateVisual(false);
                                }
                            }
                        }
                    }
                }

                if (_firstLoadTrasport)
                {
                    _firstLoadTrasport = false;
                }
            }
        }

        void transport_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!transport.CancellationPending)
            {
                try
                {
                    lock (_trolleybus)
                    {
                        Stuff.GetVilniusTransportData(WindowsForms.TransportType.TrolleyBus, string.Empty, _trolleybus);
                    }

                    lock (_bus)
                    {
                        Stuff.GetVilniusTransportData(WindowsForms.TransportType.Bus, string.Empty, _bus);
                    }

                    transport.ReportProgress(100);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("transport_DoWork: " + ex.ToString());
                }

                Thread.Sleep(3333);
            }

            _trolleybusMarkers.Clear();
            _busMarkers.Clear();
        }

        #endregion

        // add objects and zone around them
        void AddDemoZone(double areaRadius, PointLatLng center, List<PointAndInfo> objects)
        {
            var objectsInArea = from p in objects
                where MainMap.MapProvider.Projection.GetDistance(center, p.Point) <= areaRadius
                select new {Obj = p, Dist = MainMap.MapProvider.Projection.GetDistance(center, p.Point)};
            if (objectsInArea.Any())
            {
                var maxDistObject = (from p in objectsInArea
                    orderby p.Dist descending
                    select p).First();

                // add objects to zone
                foreach (var o in objectsInArea)
                {
                    var it = new GMapMarker(o.Obj.Point);
                    {
                        it.ZIndex = 55;
                        var s = new CustomMarkerDemo(this,
                            it,
                            o.Obj.Info + ", distance from center: " + o.Dist + "km.");
                        it.Shape = s;
                    }

                    MainMap.Markers.Add(it);
                }

                // add zone circle
                //if(false)
                {
                    var it = new GMapMarker(center);
                    it.ZIndex = -1;

                    var c = new Circle();
                    c.Center = center;
                    c.Bound = maxDistObject.Obj.Point;
                    c.Tag = it;
                    c.IsHitTestVisible = false;

                    UpdateCircle(c);
                    Circles.Add(it);

                    it.Shape = c;
                    MainMap.Markers.Add(it);
                }
            }
        }

        // calculates circle radius
        void UpdateCircle(Circle c)
        {
            var pxCenter = MainMap.FromLatLngToLocal(c.Center);
            var pxBounds = MainMap.FromLatLngToLocal(c.Bound);

            double a = pxBounds.X - pxCenter.X;
            double b = pxBounds.Y - pxCenter.Y;
            double pxCircleRadius = Math.Sqrt(a * a + b * b);

            c.Width = 55 + pxCircleRadius * 2;
            c.Height = 55 + pxCircleRadius * 2;
            (c.Tag as GMapMarker).Offset = new Point(-c.Width / 2, -c.Height / 2);
        }

        void MainMap_OnMapTypeChanged(GMapProvider type)
        {
            SliderZoom.Minimum = MainMap.MinZoom;
            SliderZoom.Maximum = MainMap.MaxZoom;
        }

        void MainMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(MainMap);
            currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var p = e.GetPosition(MainMap);
                currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        // zoo max & center markers
        private void button13_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);

            /*
            PointAnimation panMap = new PointAnimation();
            panMap.Duration = TimeSpan.FromSeconds(1);
            panMap.From = new Point(MainMap.Position.Lat, MainMap.Position.Lng);
            panMap.To = new Point(0, 0);
            Storyboard.SetTarget(panMap, MainMap);
            Storyboard.SetTargetProperty(panMap, new PropertyPath(GMapControl.MapPointProperty));
   
            Storyboard panMapStoryBoard = new Storyboard();
            panMapStoryBoard.Children.Add(panMap);
            panMapStoryBoard.Begin(this);
             */
        }

        // tile louading starts
        void MainMap_OnTileLoadStart()
        {
            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        ProgressBar1.Visibility = Visibility.Visible;
                    }));
            }
            catch
            {
            }
        }

        // tile loading stops
        void MainMap_OnTileLoadComplete(long elapsedMilliseconds)
        {
            MainMap.ElapsedMilliseconds = elapsedMilliseconds;

            try
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                    new Action(() =>
                    {
                        ProgressBar1.Visibility = Visibility.Hidden;
                        GroupBox3.Header = "loading, last in " + MainMap.ElapsedMilliseconds + "ms";
                    }));
            }
            catch
            {
            }
        }

        // current location changed
        void MainMap_OnCurrentPositionChanged(PointLatLng point)
        {
            MapGroup.Header = "gmap: " + point;
        }

        // reload
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ReloadMap();
        }

        // enable current marker
        private void checkBoxCurrentMarker_Checked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Add(currentMarker);
            }
        }

        // disable current marker
        private void checkBoxCurrentMarker_Unchecked(object sender, RoutedEventArgs e)
        {
            if (currentMarker != null)
            {
                MainMap.Markers.Remove(currentMarker);
            }
        }

        // enable map dragging
        private void checkBoxDragMap_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = true;
        }

        // disable map dragging
        private void checkBoxDragMap_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.CanDragMap = false;
        }

        // goto!
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double lat = double.Parse(TextBoxLat.Text, CultureInfo.InvariantCulture);
                double lng = double.Parse(TextBoxLng.Text, CultureInfo.InvariantCulture);

                MainMap.Position = new PointLatLng(lat, lng);
            }
            catch (Exception ex)
            {
                MessageBox.Show("incorrect coordinate format: " + ex.Message);
            }
        }

        // goto by geocoder
        private void textBoxGeo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var status = MainMap.SetPositionByKeywords(TextBoxGeo.Text);
                if (status != GeoCoderStatusCode.OK)
                {
                    MessageBox.Show("Geocoder can't find: '" + TextBoxGeo.Text + "', reason: " + status.ToString(),
                        "GMap.NET",
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                }
                else
                {
                    currentMarker.Position = MainMap.Position;
                }
            }
        }

        // zoom changed
        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // updates circles on map
            foreach (var c in Circles)
            {
                UpdateCircle(c.Shape as Circle);
            }
        }

        // zoom up
        private void czuZoomUp_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = (int)MainMap.Zoom + 1;
        }

        // zoom down
        private void czuZoomDown_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = (int)(MainMap.Zoom + 0.99) - 1;
        }

        // prefetch
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var area = MainMap.SelectedArea;
            if (!area.IsEmpty)
            {
                for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
                {
                    var res = MessageBox.Show("Ready ripp at Zoom = " + i + " ?",
                        "GMap.NET",
                        MessageBoxButton.YesNoCancel);

                    if (res == MessageBoxResult.Yes)
                    {
                        var obj = new TilePrefetcher();
                        obj.Owner = this;
                        obj.ShowCompleteMessage = true;
                        obj.Start(area, i, MainMap.MapProvider, 100);
                    }
                    else if (res == MessageBoxResult.No)
                    {
                        continue;
                    }
                    else if (res == MessageBoxResult.Cancel)
                    {
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Select map area holding ALT",
                    "GMap.NET",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        // access mode
        private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
        {
            MainMap.Manager.Mode = (AccessMode)ComboBoxMode.SelectedItem;
            MainMap.ReloadMap();
        }

        // clear cache
        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are You sure?",
                    "Clear GMap.NET cache?",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning) == MessageBoxResult.OK)
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

        // export
        private void button6_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowExportDialog();
        }

        // import
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ShowImportDialog();
        }

        // use route cache
        private void checkBoxCacheRoute_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseRouteCache = CheckBoxCacheRoute.IsChecked.Value;
        }

        // use geocoding cahce
        private void checkBoxGeoCache_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.Manager.UseGeocoderCache = CheckBoxGeoCache.IsChecked.Value;
            MainMap.Manager.UsePlacemarkCache = MainMap.Manager.UseGeocoderCache;
        }

        // save currnt view
        private void button7_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var img = MainMap.ToImageSource();
                var en = new PngBitmapEncoder();
                en.Frames.Add(BitmapFrame.Create(img as BitmapSource));

                var dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "GMap.NET Image"; // Default file name
                dlg.DefaultExt = ".png"; // Default file extension
                dlg.Filter = "Image (.png)|*.png"; // Filter files by extension
                dlg.AddExtension = true;
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                // Show save file dialog box
                var result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;

                    using (Stream st = File.OpenWrite(filename))
                    {
                        en.Save(st);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // clear all markers
        private void button10_Click(object sender, RoutedEventArgs e)
        {
            var clear = MainMap.Markers.Where(p => p != null && p != currentMarker);
            if (clear != null)
            {
                for (int i = 0; i < clear.Count(); i++)
                {
                    MainMap.Markers.Remove(clear.ElementAt(i));
                    i--;
                }
            }

            if (RadioButtonPerformance.IsChecked == true)
            {
                _tt = 0;
                if (!timer.IsEnabled)
                {
                    timer.Start();
                }
            }
        }

        // add marker
        private void button8_Click(object sender, RoutedEventArgs e)
        {
            var m = new GMapMarker(currentMarker.Position);
            {
                Placemark? p = null;
                if (CheckBoxPlace.IsChecked.Value)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                    if (status == GeoCoderStatusCode.OK && plret != null)
                    {
                        p = plret;
                    }
                }

                string toolTipText;
                if (p != null)
                {
                    toolTipText = p.Value.Address;
                }
                else
                {
                    toolTipText = currentMarker.Position.ToString();
                }

                m.Shape = new CustomMarkerDemo(this, m, toolTipText);
                m.ZIndex = 55;
            }
            MainMap.Markers.Add(m);
        }

        // sets route start
        private void button11_Click(object sender, RoutedEventArgs e)
        {
            _start = currentMarker.Position;
        }

        // sets route end
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            _end = currentMarker.Position;
        }

        // adds route
        private void button12_Click(object sender, RoutedEventArgs e)
        {
            var rp = MainMap.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.OpenStreetMap; // use OpenStreetMap if provider does not implement routing
            }

            var route = rp.GetRoute(_start, _end, false, false, (int)MainMap.Zoom);
            if (route != null)
            {
                var m1 = new GMapMarker(_start);
                m1.Shape = new CustomMarkerDemo(this, m1, "Start: " + route.Name);

                var m2 = new GMapMarker(_end);
                m2.Shape = new CustomMarkerDemo(this, m2, "End: " + _start.ToString());

                var mRoute = new GMapRoute(route.Points);
                {
                    mRoute.ZIndex = -1;
                }

                MainMap.Markers.Add(m1);
                MainMap.Markers.Add(m2);
                MainMap.Markers.Add(mRoute);

                MainMap.ZoomAndCenterMarkers(null);
            }
        }

        // enables tile grid view
        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = true;
        }

        // disables tile grid view
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            MainMap.ShowTileGridLines = false;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            int offset = 22;

            if (MainMap.IsFocused)
            {
                if (e.Key == Key.Left)
                {
                    MainMap.Offset(-offset, 0);
                }
                else if (e.Key == Key.Right)
                {
                    MainMap.Offset(offset, 0);
                }
                else if (e.Key == Key.Up)
                {
                    MainMap.Offset(0, -offset);
                }
                else if (e.Key == Key.Down)
                {
                    MainMap.Offset(0, offset);
                }
                else if (e.Key == Key.Add)
                {
                    czuZoomUp_Click(null, null);
                }
                else if (e.Key == Key.Subtract)
                {
                    czuZoomDown_Click(null, null);
                }
            }
        }

        // set real time demo
        private void RealTimeChanged(object sender, RoutedEventArgs e)
        {
            MainMap.Markers.Clear();

            // start performance test
            if (RadioButtonPerformance.IsChecked == true)
            {
                timer.Start();
            }
            else
            {
                // stop performance test
                timer.Stop();
            }

            // start realtime transport tracking demo
            if (RadioButtonTransport.IsChecked == true)
            {
                if (!transport.IsBusy)
                {
                    _firstLoadTrasport = true;
                    transport.RunWorkerAsync();
                }
            }
            else
            {
                if (transport.IsBusy)
                {
                    transport.CancelAsync();
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                MainMap.Bearing--;
            }
            else if (e.Key == Key.Z)
            {
                MainMap.Bearing++;
            }
        }
    }

    public class MapValidationRule : ValidationRule
    {
        bool _userAcceptedLicenseOnce;
        internal MainWindow Window;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is OpenStreetMapProviderBase))
            {
                if (!_userAcceptedLicenseOnce)
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar +
                                    "License.txt"))
                    {
                        string ctn = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory +
                                                      Path.DirectorySeparatorChar + "License.txt");
                        int li = ctn.IndexOf("License");
                        string txt = ctn.Substring(li);

                        var d = new Windows.Message();
                        d.RichTextBox1.Text = txt;

                        if (true == d.ShowDialog())
                        {
                            _userAcceptedLicenseOnce = true;
                            if (Window != null)
                            {
                                Window.Title += " - license accepted by " + Environment.UserName + " at " +
                                                DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        // user deleted License.txt ;}
                        _userAcceptedLicenseOnce = true;
                    }
                }

                if (!_userAcceptedLicenseOnce)
                {
                    return new ValidationResult(false, "user do not accepted license ;/");
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
