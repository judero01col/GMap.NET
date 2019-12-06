using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using GMap.NET.CacheProviders;
using GMap.NET.Internals;
using GMap.NET.MapProviders;

namespace GMap.NET
{
    /// <summary>
    ///     maps manager
    /// </summary>
    public class GMaps : Singleton<GMaps>
    {
        /// <summary>
        ///     tile access mode
        /// </summary>
        public AccessMode Mode = AccessMode.ServerAndCache;

        /// <summary>
        ///     is map ussing cache for routing
        /// </summary>
        public bool UseRouteCache = true;

        /// <summary>
        ///     is map using cache for geocoder
        /// </summary>
        public bool UseGeocoderCache = true;

        /// <summary>
        ///     is map using cache for directions
        /// </summary>
        public bool UseDirectionsCache = true;

        /// <summary>
        ///     is map using cache for placemarks
        /// </summary>
        public bool UsePlacemarkCache = true;

        /// <summary>
        ///     is map ussing cache for other url
        /// </summary>
        public bool UseUrlCache = true;

        /// <summary>
        ///     is map using memory cache for tiles
        /// </summary>
        public bool UseMemoryCache = true;

        /// <summary>
        ///     primary cache provider, by default: ultra fast SQLite!
        /// </summary>
        public PureImageCache PrimaryCache
        {
            get
            {
                return Cache.Instance.ImageCache;
            }
            set
            {
                Cache.Instance.ImageCache = value;
            }
        }

        /// <summary>
        ///     secondary cache provider, by default: none,
        ///     use it if you have server in your local network
        /// </summary>
        public PureImageCache SecondaryCache
        {
            get
            {
                return Cache.Instance.ImageCacheSecond;
            }
            set
            {
                Cache.Instance.ImageCacheSecond = value;
            }
        }

        /// <summary>
        ///     MemoryCache provider
        /// </summary>
        public readonly MemoryCache MemoryCache = new MemoryCache();

        /// <summary>
        ///     load tiles in random sequence
        /// </summary>
        public bool ShuffleTilesOnLoad = false;

        /// <summary>
        ///     tile queue to cache
        /// </summary>
        private readonly Queue<CacheQueueItem> _tileCacheQueue = new Queue<CacheQueueItem>();

        private bool? _isRunningOnMono;

        /// <summary>
        ///     return true if running on mono
        /// </summary>
        /// <returns></returns>
        public bool IsRunningOnMono
        {
            get
            {
                if (!_isRunningOnMono.HasValue)
                {
                    try
                    {
                        _isRunningOnMono = Type.GetType("Mono.Runtime") != null;
                        return _isRunningOnMono.Value;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    return _isRunningOnMono.Value;
                }

                return false;
            }
        }

        /// <summary>
        ///     cache worker
        /// </summary>
        private Thread _cacheEngine;

        internal readonly AutoResetEvent WaitForCache = new AutoResetEvent(false);

        static GMaps()
        {
            if (GMapProvider.TileImageProxy == null)
            {
                try
                {
                    var d = AppDomain.CurrentDomain;
                    var assembliesLoaded = d.GetAssemblies();

                    Assembly l = null;

                    foreach (var a in assembliesLoaded)
                    {
                        if (a.FullName.Contains("GMap.NET.WindowsForms") ||
                            a.FullName.Contains("GMap.NET.WindowsPresentation"))
                        {
                            l = a;
                            break;
                        }
                    }

                    if (l == null)
                    {
                        string jj = Assembly.GetExecutingAssembly().Location;
                        string hh = Path.GetDirectoryName(jj);
                        string f1 = hh + Path.DirectorySeparatorChar + "GMap.NET.WindowsForms.dll";
                        string f2 = hh + Path.DirectorySeparatorChar + "GMap.NET.WindowsPresentation.dll";
                        if (File.Exists(f1))
                        {
                            l = Assembly.LoadFile(f1);
                        }
                        else if (File.Exists(f2))
                        {
                            l = Assembly.LoadFile(f2);
                        }
                    }

                    if (l != null)
                    {
                        Type t = null;

                        if (l.FullName.Contains("GMap.NET.WindowsForms"))
                        {
                            t = l.GetType("GMap.NET.WindowsForms.GMapImageProxy");
                        }
                        else if (l.FullName.Contains("GMap.NET.WindowsPresentation"))
                        {
                            t = l.GetType("GMap.NET.WindowsPresentation.GMapImageProxy");
                        }

                        if (t != null)
                        {
                            t.InvokeMember("Enable",
                                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static |
                                BindingFlags.InvokeMethod,
                                null,
                                null,
                                null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GMaps, try set TileImageProxy failed: " + ex.Message);
                }
            }
        }

        public GMaps()
        {
            #region singleton check

            if (Instance != null)
            {
                throw new Exception(
                    "You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\"");
            }

            #endregion

            ServicePointManager.DefaultConnectionLimit = 5;
        }

        /// <summary>
        ///     triggers dynamic sqlite loading,
        ///     call this before you use sqlite for other reasons than caching maps
        /// </summary>
        public void SQLitePing()
        {
#if SQLite
#if !MONO
            SQLitePureImageCache.Ping();
#endif
#endif
        }

        #region -- Stuff --

        /// <summary>
        ///     exports current map cache to GMDB file
        ///     if file exsist only new records will be added
        ///     otherwise file will be created and all data exported
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool ExportToGMDB(string file)
        {
#if SQLite
            if (PrimaryCache is SQLitePureImageCache)
            {
                var db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
                db.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}{1}Data.gmdb",
                    GMapProvider.LanguageStr,
                    Path.DirectorySeparatorChar);

                return SQLitePureImageCache.ExportMapDataToDB(db.ToString(), file);
            }
#endif
            return false;
        }

        /// <summary>
        ///     imports GMDB file to current map cache
        ///     only new records will be added
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool ImportFromGMDB(string file)
        {
#if SQLite
            if (PrimaryCache is SQLitePureImageCache)
            {
                var db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
                db.AppendFormat(CultureInfo.InvariantCulture,
                    "{0}{1}Data.gmdb",
                    GMapProvider.LanguageStr,
                    Path.DirectorySeparatorChar);

                return SQLitePureImageCache.ExportMapDataToDB(file, db.ToString());
            }
#endif
            return false;
        }

#if SQLite

        /// <summary>
        ///     optimizes map database, *.gmdb
        /// </summary>
        /// <param name="file">database file name or null to optimize current user db</param>
        /// <returns></returns>
        public bool OptimizeMapDb(string file)
        {
            if (PrimaryCache is SQLitePureImageCache)
            {
                if (string.IsNullOrEmpty(file))
                {
                    var db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
                    db.AppendFormat(CultureInfo.InvariantCulture,
                        "{0}{1}Data.gmdb",
                        GMapProvider.LanguageStr,
                        Path.DirectorySeparatorChar);

                    return SQLitePureImageCache.VacuumDb(db.ToString());
                }
                else
                {
                    return SQLitePureImageCache.VacuumDb(file);
                }
            }

            return false;
        }
#endif

        /// <summary>
        ///     enqueueens tile to cache
        /// </summary>
        /// <param name="task"></param>
        void EnqueueCacheTask(CacheQueueItem task)
        {
            lock (_tileCacheQueue)
            {
                if (!_tileCacheQueue.Contains(task))
                {
                    Debug.WriteLine("EnqueueCacheTask: " + task);

                    _tileCacheQueue.Enqueue(task);

                    if (_cacheEngine != null && _cacheEngine.IsAlive)
                    {
                        WaitForCache.Set();
                    }
                    else if (_cacheEngine == null || _cacheEngine.ThreadState == System.Threading.ThreadState.Stopped ||
                             _cacheEngine.ThreadState == System.Threading.ThreadState.Unstarted)
                    {
                        _cacheEngine = null;
                        _cacheEngine = new Thread(CacheEngineLoop);
                        _cacheEngine.Name = "CacheEngine";
                        _cacheEngine.IsBackground = false;
                        _cacheEngine.Priority = ThreadPriority.Lowest;

                        _abortCacheLoop = false;
                        _cacheEngine.Start();
                    }
                }
            }
        }

        volatile bool _abortCacheLoop;
        internal volatile bool NoMapInstances = false;

        public TileCacheComplete OnTileCacheComplete;
        public TileCacheStart OnTileCacheStart;
        public TileCacheProgress OnTileCacheProgress;

        /// <summary>
        ///     immediately stops background tile caching, call it if you want fast exit the process
        /// </summary>
        public void CancelTileCaching()
        {
            Debug.WriteLine("CancelTileCaching...");

            _abortCacheLoop = true;
            lock (_tileCacheQueue)
            {
                _tileCacheQueue.Clear();
                WaitForCache.Set();
            }
        }

        private int _readingCache;
        private volatile bool _cacheOnIdleRead = true;

        /// <summary>
        ///     delays writing tiles to cache while performing reads
        /// </summary>
        public bool CacheOnIdleRead
        {
            get
            {
                return _cacheOnIdleRead;
            }
            set
            {
                _cacheOnIdleRead = value;
            }
        }

        volatile bool _boostCacheEngine;

        /// <summary>
        ///     disables delay between saving tiles into database/cache
        /// </summary>
        public bool BoostCacheEngine
        {
            get
            {
                return _boostCacheEngine;
            }
            set
            {
                _boostCacheEngine = value;
            }
        }

        /// <summary>
        ///     live for cache ;}
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void CacheEngineLoop()
        {
            Debug.WriteLine("CacheEngine: start");
            int left;

            if (OnTileCacheStart != null)
            {
                OnTileCacheStart();
            }

            bool startEvent = false;

            while (!_abortCacheLoop)
            {
                try
                {
                    CacheQueueItem? task = null;

                    lock (_tileCacheQueue)
                    {
                        left = _tileCacheQueue.Count;
                        if (left > 0)
                        {
                            task = _tileCacheQueue.Dequeue();
                        }
                    }

                    if (task.HasValue)
                    {
                        if (startEvent)
                        {
                            startEvent = false;

                            if (OnTileCacheStart != null)
                            {
                                OnTileCacheStart();
                            }
                        }

                        if (OnTileCacheProgress != null)
                        {
                            OnTileCacheProgress(left);
                        }

                        #region -- save --

                        // check if stream wasn't disposed somehow
                        if (task.Value.Img != null)
                        {
                            Debug.WriteLine("CacheEngine[" + left + "]: storing tile " + task.Value + ", " +
                                            task.Value.Img.Length / 1024 + "kB...");

                            if ((task.Value.CacheType & CacheUsage.First) == CacheUsage.First && PrimaryCache != null)
                            {
                                if (_cacheOnIdleRead)
                                {
                                    while (Interlocked.Decrement(ref _readingCache) > 0)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }

                                PrimaryCache.PutImageToCache(task.Value.Img,
                                    task.Value.Tile.Type,
                                    task.Value.Tile.Pos,
                                    task.Value.Tile.Zoom);
                            }

                            if ((task.Value.CacheType & CacheUsage.Second) == CacheUsage.Second &&
                                SecondaryCache != null)
                            {
                                if (_cacheOnIdleRead)
                                {
                                    while (Interlocked.Decrement(ref _readingCache) > 0)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }

                                SecondaryCache.PutImageToCache(task.Value.Img,
                                    task.Value.Tile.Type,
                                    task.Value.Tile.Pos,
                                    task.Value.Tile.Zoom);
                            }

                            task.Value.Clear();

                            if (!_boostCacheEngine)
                            {
                                Thread.Sleep(333);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("CacheEngineLoop: skip, tile disposed to early -> " + task.Value);
                        }

                        #endregion
                    }
                    else
                    {
                        if (!startEvent)
                        {
                            startEvent = true;

                            if (OnTileCacheComplete != null)
                            {
                                OnTileCacheComplete();
                            }
                        }

                        if (_abortCacheLoop || NoMapInstances || !WaitForCache.WaitOne(33333, false) || NoMapInstances)
                        {
                            break;
                        }
                    }
                }
                catch (AbandonedMutexException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("CacheEngineLoop: " + ex.ToString());
                }
            }

            Debug.WriteLine("CacheEngine: stop");

            if (!startEvent)
            {
                if (OnTileCacheComplete != null)
                {
                    OnTileCacheComplete();
                }
            }
        }

        class StringWriterExt : StringWriter
        {
            public StringWriterExt(IFormatProvider info)
                : base(info)
            {
            }

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }
        }

        public string SerializeGPX(gpxType targetInstance)
        {
            string retVal;
            using (var writer = new StringWriterExt(CultureInfo.InvariantCulture))
            {
                var serializer = new XmlSerializer(targetInstance.GetType());
                serializer.Serialize(writer, targetInstance);
                retVal = writer.ToString();
            }

            return retVal;
        }

        public gpxType DeserializeGPX(string objectXml)
        {
            gpxType retVal;

            using (var stringReader = new StringReader(objectXml))
            {
                var xmlReader = new XmlTextReader(stringReader);

                var serializer = new XmlSerializer(typeof(gpxType));
                retVal = serializer.Deserialize(xmlReader) as gpxType;

                xmlReader.Close();
            }

            return retVal;
        }

        /// <summary>
        ///     exports gps data to gpx file
        /// </summary>
        /// <param name="log">gps data</param>
        /// <param name="gpxFile">file to export</param>
        /// <returns>true if success</returns>
        public bool ExportGPX(IEnumerable<List<GpsLog>> log, string gpxFile)
        {
            try
            {
                var gpx = new gpxType();
                {
                    gpx.creator = "GMap.NET - http://greatmaps.codeplex.com";
                    gpx.trk = new trkType[1];
                    gpx.trk[0] = new trkType();
                }

                var sessions = new List<List<GpsLog>>(log);
                gpx.trk[0].trkseg = new trksegType[sessions.Count];

                int sesid = 0;

                foreach (var session in sessions)
                {
                    var seg = new trksegType();
                    {
                        seg.trkpt = new wptType[session.Count];
                    }
                    gpx.trk[0].trkseg[sesid++] = seg;

                    for (int i = 0; i < session.Count; i++)
                    {
                        var point = session[i];

                        var t = new wptType();
                        {
                            #region -- set values --

                            t.lat = new decimal(point.Position.Lat);
                            t.lon = new decimal(point.Position.Lng);

                            t.time = point.TimeUTC;
                            t.timeSpecified = true;

                            if (point.FixType != FixType.Unknown)
                            {
                                t.fix = point.FixType == FixType.XyD ? fixType.Item2d : fixType.Item3d;
                                t.fixSpecified = true;
                            }

                            if (point.SeaLevelAltitude.HasValue)
                            {
                                t.ele = new decimal(point.SeaLevelAltitude.Value);
                                t.eleSpecified = true;
                            }

                            if (point.EllipsoidAltitude.HasValue)
                            {
                                t.geoidheight = new decimal(point.EllipsoidAltitude.Value);
                                t.geoidheightSpecified = true;
                            }

                            if (point.VerticalDilutionOfPrecision.HasValue)
                            {
                                t.vdopSpecified = true;
                                t.vdop = new decimal(point.VerticalDilutionOfPrecision.Value);
                            }

                            if (point.HorizontalDilutionOfPrecision.HasValue)
                            {
                                t.hdopSpecified = true;
                                t.hdop = new decimal(point.HorizontalDilutionOfPrecision.Value);
                            }

                            if (point.PositionDilutionOfPrecision.HasValue)
                            {
                                t.pdopSpecified = true;
                                t.pdop = new decimal(point.PositionDilutionOfPrecision.Value);
                            }

                            if (point.SatelliteCount.HasValue)
                            {
                                t.sat = point.SatelliteCount.Value.ToString();
                            }

                            #endregion
                        }
                        seg.trkpt[i] = t;
                    }
                }

                sessions.Clear();

                File.WriteAllText(gpxFile, SerializeGPX(gpx), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ExportGPX: " + ex.ToString());
                return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        ///     gets image from tile server
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="pos"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public PureImage GetImageFrom(GMapProvider provider, GPoint pos, int zoom, out Exception result)
        {
            PureImage ret = null;
            result = null;

            try
            {
                var rtile = new RawTile(provider.DbId, pos, zoom);

                // let't check memmory first
                if (UseMemoryCache)
                {
                    var m = MemoryCache.GetTileFromMemoryCache(rtile);
                    if (m != null)
                    {
                        if (GMapProvider.TileImageProxy != null)
                        {
                            ret = GMapProvider.TileImageProxy.FromArray(m);
                            if (ret == null)
                            {
#if DEBUG
                                Debug.WriteLine("Image disposed in MemoryCache o.O, should never happen ;} " +
                                                new RawTile(provider.DbId, pos, zoom));
                                if (Debugger.IsAttached)
                                {
                                    Debugger.Break();
                                }
#endif
                            }
                        }
                    }
                }

                if (ret == null)
                {
                    if (Mode != AccessMode.ServerOnly && !provider.BypassCache)
                    {
                        if (PrimaryCache != null)
                        {
                            // hold writer for 5s
                            if (_cacheOnIdleRead)
                            {
                                Interlocked.Exchange(ref _readingCache, 5);
                            }

                            ret = PrimaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                            if (ret != null)
                            {
                                if (UseMemoryCache)
                                {
                                    MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                                }

                                return ret;
                            }
                        }

                        if (SecondaryCache != null)
                        {
                            // hold writer for 5s
                            if (_cacheOnIdleRead)
                            {
                                Interlocked.Exchange(ref _readingCache, 5);
                            }

                            ret = SecondaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                            if (ret != null)
                            {
                                if (UseMemoryCache)
                                {
                                    MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                                }

                                EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.First));
                                return ret;
                            }
                        }
                    }

                    if (Mode != AccessMode.CacheOnly)
                    {
                        ret = provider.GetTileImage(pos, zoom);
                        {
                            // Enqueue Cache
                            if (ret != null)
                            {
                                if (UseMemoryCache)
                                {
                                    MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                                }

                                if (Mode != AccessMode.ServerOnly && !provider.BypassCache)
                                {
                                    EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.Both));
                                }
                            }
                        }
                    }
                    else
                    {
                        result = _noDataException;
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex;
                ret = null;
                Debug.WriteLine("GetImageFrom: " + ex.ToString());
            }

            return ret;
        }

        private readonly Exception _noDataException = new Exception("No data in local tile cache...");

        private TileHttpHost _host;

        /// <summary>
        ///     turns on tile host
        /// </summary>
        /// <param name="port"></param>
        public void EnableTileHost(int port)
        {
            if (_host == null)
            {
                _host = new TileHttpHost();
            }

            _host.Start(port);
        }

        /// <summary>
        ///     turns off tile host
        /// </summary>
        /// <param name="port"></param>
        public void DisableTileHost()
        {
            if (_host != null)
            {
                _host.Stop();
            }
        }
    }
}
