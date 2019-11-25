using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GMap.NET.Internals;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;

namespace GMap.NET
{
    /// <summary>
    ///     form helping to prefetch tiles on local db
    /// </summary>
    public partial class TilePrefetcher : Form
    {
        BackgroundWorker worker = new BackgroundWorker();
        List<GPoint> _list;
        int _zoom;
        GMapProvider _provider;
        int _sleep;
        int _all;
        public bool ShowCompleteMessage = false;
        RectLatLng _area;
        GSize _maxOfTiles;
        public GMapOverlay Overlay;
        int _retry;
        public bool Shuffle = true;

        public TilePrefetcher()
        {
            InitializeComponent();

            GMaps.Instance.OnTileCacheComplete += OnTileCacheComplete;
            GMaps.Instance.OnTileCacheStart += OnTileCacheStart;
            GMaps.Instance.OnTileCacheProgress += OnTileCacheProgress;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        readonly AutoResetEvent _done = new AutoResetEvent(true);

        void OnTileCacheComplete()
        {
            if (!IsDisposed)
            {
                _done.Set();

                MethodInvoker m = delegate
                {
                    label2.Text = "all tiles saved";
                };
                Invoke(m);
            }
        }

        void OnTileCacheStart()
        {
            if (!IsDisposed)
            {
                _done.Reset();

                MethodInvoker m = delegate
                {
                    label2.Text = "saving tiles...";
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
                    label2.Text = left + " tile to save...";
                };
                Invoke(m);
            }
        }

        public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep, int retry)
        {
            if (!worker.IsBusy)
            {
                label1.Text = "...";
                progressBarDownload.Value = 0;

                _area = area;
                _zoom = zoom;
                _provider = provider;
                _sleep = sleep;
                _retry = retry;

                GMaps.Instance.UseMemoryCache = false;
                GMaps.Instance.CacheOnIdleRead = false;
                GMaps.Instance.BoostCacheEngine = true;

                if (Overlay != null)
                {
                    Overlay.Markers.Clear();
                }

                worker.RunWorkerAsync();

                ShowDialog();
            }
        }

        public void Stop()
        {
            GMaps.Instance.OnTileCacheComplete -= OnTileCacheComplete;
            GMaps.Instance.OnTileCacheStart -= OnTileCacheStart;
            GMaps.Instance.OnTileCacheProgress -= OnTileCacheProgress;

            _done.Set();

            if (worker.IsBusy)
            {
                worker.CancelAsync();
            }

            GMaps.Instance.CancelTileCaching();

            _done.Close();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ShowCompleteMessage)
            {
                if (!e.Cancelled)
                {
                    MessageBox.Show(this, "Prefetch Complete! => " + ((int)e.Result).ToString() + " of " + _all);
                }
                else
                {
                    MessageBox.Show(this, "Prefetch Canceled! => " + ((int)e.Result).ToString() + " of " + _all);
                }
            }

            _list.Clear();

            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.CacheOnIdleRead = true;
            GMaps.Instance.BoostCacheEngine = false;

            worker.Dispose();

            Close();
        }

        bool CacheTiles(int zoom, GPoint p)
        {
            foreach (var pr in _provider.Overlays)
            {
                Exception ex;
                PureImage img;

                // tile number inversion(BottomLeft -> TopLeft)
                if (pr.InvertedAxisY)
                {
                    img = GMaps.Instance.GetImageFrom(pr, new GPoint(p.X, _maxOfTiles.Height - p.Y), zoom, out ex);
                }
                else // ok
                {
                    img = GMaps.Instance.GetImageFrom(pr, p, zoom, out ex);
                }

                if (img != null)
                {
                    img.Dispose();
                    img = null;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public readonly Queue<GPoint> CachedTiles = new Queue<GPoint>();

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_list != null)
            {
                _list.Clear();
                _list = null;
            }

            _list = _provider.Projection.GetAreaTileList(_area, _zoom, 0);
            _maxOfTiles = _provider.Projection.GetTileMatrixMaxXY(_zoom);
            _all = _list.Count;

            int countOk = 0;
            int retryCount = 0;

            if (Shuffle)
            {
                Stuff.Shuffle(_list);
            }

            lock (this)
            {
                CachedTiles.Clear();
            }

            for (int i = 0; i < _all; i++)
            {
                if (worker.CancellationPending)
                    break;

                var p = _list[i];
                {
                    if (CacheTiles(_zoom, p))
                    {
                        if (Overlay != null)
                        {
                            lock (this)
                            {
                                CachedTiles.Enqueue(p);
                            }
                        }

                        countOk++;
                        retryCount = 0;
                    }
                    else
                    {
                        if (++retryCount <= _retry) // retry only one
                        {
                            i--;
                            Thread.Sleep(1111);
                            continue;
                        }
                        else
                        {
                            retryCount = 0;
                        }
                    }
                }

                worker.ReportProgress((i + 1) * 100 / _all, i + 1);

                if (_sleep > 0)
                {
                    Thread.Sleep(_sleep);
                }
            }

            e.Result = countOk;

            if (!IsDisposed)
            {
                _done.WaitOne();
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            label1.Text = "Fetching tile at zoom (" + _zoom + "): " + ((int)e.UserState).ToString() + " of " + _all +
                               ", complete: " + e.ProgressPercentage.ToString() + "%";
            progressBarDownload.Value = e.ProgressPercentage;

            if (Overlay != null)
            {
                GPoint? l = null;

                lock (this)
                {
                    if (CachedTiles.Count > 0)
                    {
                        l = CachedTiles.Dequeue();
                    }
                }

                if (l.HasValue)
                {
                    var px = Overlay.Control.MapProvider.Projection.FromTileXYToPixel(l.Value);
                    var p = Overlay.Control.MapProvider.Projection.FromPixelToLatLng(px, _zoom);

                    double r1 = Overlay.Control.MapProvider.Projection.GetGroundResolution(_zoom, p.Lat);
                    double r2 = Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom,
                        p.Lat);
                    double sizeDiff = r2 / r1;

                    var m = new GMapMarkerTile(p,
                        (int)(Overlay.Control.MapProvider.Projection.TileSize.Width / sizeDiff));
                    Overlay.Markers.Add(m);
                }
            }
        }

        private void Prefetch_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void Prefetch_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
        }
    }

    class GMapMarkerTile : GMapMarker
    {
        static Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

        public GMapMarkerTile(PointLatLng p, int size) : base(p)
        {
            Size = new Size(size, size);
        }

        public override void OnRender(Graphics g)
        {
            g.FillRectangle(Fill, new Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
        }
    }
}
