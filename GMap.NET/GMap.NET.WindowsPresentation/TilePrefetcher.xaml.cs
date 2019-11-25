using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GMap.NET.Internals;
using GMap.NET.MapProviders;

namespace GMap.NET.WindowsPresentation
{
    /// <summary>
    ///     form helping to prefetch tiles on local db
    /// </summary>
    public partial class TilePrefetcher : Window
    {
        readonly BackgroundWorker _worker = new BackgroundWorker();
        List<GPoint> _list = new List<GPoint>();
        int _zoom;
        GMapProvider _provider;
        int _sleep;
        int _all;
        public bool ShowCompleteMessage = false;
        RectLatLng _area;
        GSize _maxOfTiles;

        public TilePrefetcher()
        {
            InitializeComponent();

            GMaps.Instance.OnTileCacheComplete += OnTileCacheComplete;
            GMaps.Instance.OnTileCacheStart += OnTileCacheStart;
            GMaps.Instance.OnTileCacheProgress += OnTileCacheProgress;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.ProgressChanged += worker_ProgressChanged;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        readonly AutoResetEvent _done = new AutoResetEvent(true);

        void OnTileCacheComplete()
        {
            if (IsVisible)
            {
                _done.Set();

                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        Label2.Text = "all tiles saved";
                    }));
            }
        }

        void OnTileCacheStart()
        {
            if (IsVisible)
            {
                _done.Reset();

                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        Label2.Text = "saving tiles...";
                    }));
            }
        }

        void OnTileCacheProgress(int left)
        {
            if (IsVisible)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        Label2.Text = left + " tile to save...";
                    }));
            }
        }

        public void Start(RectLatLng area, int zoom, GMapProvider provider, int sleep)
        {
            if (!_worker.IsBusy)
            {
                Label1.Text = "...";
                ProgressBar1.Value = 0;

                _area = area;
                _zoom = zoom;
                _provider = provider;
                _sleep = sleep;

                GMaps.Instance.UseMemoryCache = false;
                GMaps.Instance.CacheOnIdleRead = false;
                GMaps.Instance.BoostCacheEngine = true;

                _worker.RunWorkerAsync();

                ShowDialog();
            }
        }

        volatile bool _stopped;

        public void Stop()
        {
            GMaps.Instance.OnTileCacheComplete -= OnTileCacheComplete;
            GMaps.Instance.OnTileCacheStart -= OnTileCacheStart;
            GMaps.Instance.OnTileCacheProgress -= OnTileCacheProgress;

            _done.Set();

            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }

            GMaps.Instance.CancelTileCaching();

            _stopped = true;

            _done.Close();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ShowCompleteMessage)
            {
                if (!e.Cancelled)
                {
                    MessageBox.Show("Prefetch Complete! => " + ((int)e.Result).ToString() + " of " + _all);
                }
                else
                {
                    MessageBox.Show("Prefetch Canceled! => " + ((int)e.Result).ToString() + " of " + _all);
                }
            }

            _list.Clear();

            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.CacheOnIdleRead = true;
            GMaps.Instance.BoostCacheEngine = false;

            Close();
        }

        bool CacheTiles(int zoom, GPoint p)
        {
            foreach (var type in _provider.Overlays)
            {
                Exception ex;
                PureImage img;

                // tile number inversion(BottomLeft -> TopLeft) for pergo maps
                if (type is TurkeyMapProvider)
                {
                    img = GMaps.Instance.GetImageFrom(type, new GPoint(p.X, _maxOfTiles.Height - p.Y), zoom, out ex);
                }
                else // ok
                {
                    img = GMaps.Instance.GetImageFrom(type, p, zoom, out ex);
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
            int retry = 0;

            Stuff.Shuffle(_list);

            for (int i = 0; i < _all; i++)
            {
                if (_worker.CancellationPending)
                    break;

                var p = _list[i];
                {
                    if (CacheTiles(_zoom, p))
                    {
                        countOk++;
                        retry = 0;
                    }
                    else
                    {
                        if (++retry <= 1) // retry only one
                        {
                            i--;
                            Thread.Sleep(1111);
                            continue;
                        }
                        else
                        {
                            retry = 0;
                        }
                    }
                }

                _worker.ReportProgress((i + 1) * 100 / _all, i + 1);

                Thread.Sleep(_sleep);
            }

            e.Result = countOk;

            if (!_stopped)
            {
                _done.WaitOne();
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Label1.Text = "Fetching tile at zoom (" + _zoom + "): " + ((int)e.UserState).ToString() + " of " +
                               _all + ", complete: " + e.ProgressPercentage.ToString() + "%";
            ProgressBar1.Value = e.ProgressPercentage;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Stop();

            base.OnClosed(e);
        }
    }
}
