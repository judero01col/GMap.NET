
namespace GMap.NET.MapProviders
{
    using System;
    using GMap.NET.Projections;
    using System.Security.Cryptography;
    using System.Diagnostics;
    using System.Net;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using GMap.NET.Internals;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using System.Text;
    using Newtonsoft.Json;
    
#if PocketPC
    using OpenNETCF.Security.Cryptography;
#endif

    public abstract class GoogleMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider, DirectionsProvider, RoadsProvider
    {
        public GoogleMapProviderBase()
        {
            MaxZoom = null;
            RefererUrl = string.Format("https://maps.{0}/", Server);
            Copyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
        }

        public readonly string ServerAPIs /* ;}~~ */ = Stuff.GString(/*{^_^}*/"9gERyvblybF8iMuCt/LD6w=="/*d{'_'}b*/);
        public readonly string Server /* ;}~~~~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoS+bXaIxt6XWg=="/*d{'_'}b*/);
        public readonly string ServerChina /* ;}~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoTEJoJJuO25gQ=="/*d{'_'}b*/);
        public readonly string ServerKorea /* ;}~~ */ = Stuff.GString(/*{^_^}*/"8ZVBOEsBinzi+zmP7y7pPA=="/*d{'_'}b*/);
        public readonly string ServerKoreaKr /* ;}~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoQyz1gkC4QLfg=="/*d{'_'}b*/);

        public string SecureWord = "Galileo";

        /// <summary>
        /// Your application's API key, obtained from the Google Developers Console.
        /// This key identifies your application for purposes of quota management. 
        /// Must provide either API key or Maps for Work credentials.
        /// </summary>
        public string ApiKey = string.Empty;

        #region GMapProvider Members

        public override Guid Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }

        GMapProvider [] overlays;

        public override GMapProvider [] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider [] { this };
                }
                return overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            throw new NotImplementedException();
        }

        #endregion

        public bool TryCorrectVersion = true;
        static bool init = false;

        public override void OnInitialized()
        {
            if (!init && TryCorrectVersion)
            {               
                string url = string.Format("https://maps.{0}/maps/api/js?client=google-maps-lite&amp;libraries=search&amp;language=en&amp;region=", ServerAPIs);
                try
                {
                    string html = GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(TTLCache)) : string.Empty;

                    if (string.IsNullOrEmpty(html))
                    {
                        html = GetContentUsingHttp(url);

                        if (!string.IsNullOrEmpty(html))
                        {
                            if (GMaps.Instance.UseUrlCache)
                            {
                                Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(html))
                    {
                        #region -- match versions --
                        Regex reg = new Regex(string.Format(@"https?://mts?\d.{0}/maps/vt\?lyrs=m@(\d*)", Server), RegexOptions.IgnoreCase);
                        Match mat = reg.Match(html);

                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;

                            if (count > 0)
                            {
                                string ver = string.Format("m@{0}", gc [1].Value);
                                string old = GMapProviders.GoogleMap.Version;

                                GMapProviders.GoogleMap.Version = ver;
                                GMapProviders.GoogleChinaMap.Version = ver;
                                
                                string verh = string.Format("h@{0}", gc [1].Value);
                                string oldh = GMapProviders.GoogleHybridMap.Version;

                                GMapProviders.GoogleHybridMap.Version = verh;
                                GMapProviders.GoogleChinaHybridMap.Version = verh;
#if DEBUG
                                Debug.WriteLine("GMapProviders.GoogleMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                                Debug.WriteLine("GMapProviders.GoogleHybridMap.Version: " + verh + ", " + (verh == oldh ? "OK" : "old: " + oldh + ", consider updating source"));

                                if (Debugger.IsAttached && ver != old)
                                {
                                    Thread.Sleep(1111);
                                }
#endif
                            }
                        }

                        reg = new Regex(string.Format(@"https?://khms?\d.{0}/kh\?v=(\d*)", Server), RegexOptions.IgnoreCase);
                        mat = reg.Match(html);

                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;

                            if (count > 0)
                            {
                                string ver = gc [1].Value;
                                string old = GMapProviders.GoogleSatelliteMap.Version;

                                GMapProviders.GoogleSatelliteMap.Version = ver;
                                GMapProviders.GoogleKoreaSatelliteMap.Version = ver;
                                GMapProviders.GoogleChinaSatelliteMap.Version = "s@" + ver;
#if DEBUG
                                Debug.WriteLine("GMapProviders.GoogleSatelliteMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));
                                if (Debugger.IsAttached && ver != old)
                                {
                                    Thread.Sleep(1111);
                                }
#endif
                            }
                        }

                        reg = new Regex(string.Format(@"https?://mts?\d.{0}/maps/vt\?lyrs=t@(\d*),r@(\d*)", Server), RegexOptions.IgnoreCase);
                        mat = reg.Match(html);

                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;

                            if (count > 1)
                            {
                                string ver = string.Format("t@{0},r@{1}", gc [1].Value, gc [2].Value);
                                string old = GMapProviders.GoogleTerrainMap.Version;

                                GMapProviders.GoogleTerrainMap.Version = ver;
                                GMapProviders.GoogleChinaTerrainMap.Version = ver;
#if DEBUG
                                Debug.WriteLine("GMapProviders.GoogleTerrainMap.Version: " + ver + ", " + (ver == old ? "OK" : "old: " + old + ", consider updating source"));

                                if (Debugger.IsAttached && ver != old)
                                {
                                    Thread.Sleep(1111);
                                }
#endif
                            }
                        }
                        #endregion
                    }

                    init = true; // try it only once
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TryCorrectGoogleVersions failed: " + ex.ToString());
                }
            }
        }

        internal void GetSecureWords(GPoint pos, out string sec1, out string sec2)
        {
            sec1 = string.Empty; // after &x=...
            sec2 = string.Empty; // after &zoom=...
            int seclen = (int)((pos.X * 3) + pos.Y) % 8;
            sec2 = SecureWord.Substring(0, seclen);

            if (pos.Y >= 10000 && pos.Y < 100000)
            {
                sec1 = Sec1;
            }
        }

        static readonly string Sec1 = "&s=";

        #region RoutingProvider Members

        public virtual MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode, int Zoom)
        {
            return GetRoute(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode));
        }

        public virtual MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int Zoom)
        {
            return GetRoute(MakeRouteUrl(start, end, LanguageStr, avoidHighways, walkingMode));
        }

        #region -- internals --

        string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
        {
            string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
            return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, language, opt, start.Lat, start.Lng, end.Lat, end.Lng, ServerAPIs);
        }

        string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
        {
            string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
            return string.Format(RouteUrlFormatStr, language, opt, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
        }

        MapRoute GetRoute(string url)
        {
            MapRoute ret = null;
            StrucRute RouteResult = null;

            try
            {
                string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(TTLCache)) : string.Empty;

                if (string.IsNullOrEmpty(route))
                {
                    route = GetContentUsingHttp(!string.IsNullOrEmpty(ClientId) ? GetSignedUri(url) : (!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url));

                    if (!string.IsNullOrEmpty(route))
                    {
                        RouteResult = JsonConvert.DeserializeObject<StrucRute>(route);

                        if (GMaps.Instance.UseRouteCache && RouteResult != null && RouteResult.status == RouteStatusCode.OK)
                        {
                            Cache.Instance.SaveContent(url, CacheType.RouteCache, route);
                        }
                    }
                }
                else
                {
                    RouteResult = JsonConvert.DeserializeObject<StrucRute>(route);
                }

                if (RouteResult != null)
                {
                    if (RouteResult.error == null && RouteResult.routes != null && RouteResult.routes.Count > 0)
                    {
                        ret = new MapRoute(RouteResult.routes[0].summary);
                    }
                    else
                    {
                        ret = new MapRoute("Route");
                    }

                    if (RouteResult.error == null)
                    {
                        ret.Status = RouteResult.status;

                        if (RouteResult.routes != null && RouteResult.routes.Count > 0)
                        {
                            if (RouteResult.routes.Count > 0)
                            {
                                List<PointLatLng> points = new List<PointLatLng>();

                                if (RouteResult.routes[0].overview_polyline != null && RouteResult.routes[0].overview_polyline.points != null)
                                {
                                    points = new List<PointLatLng>();
                                    DecodePointsInto(points, RouteResult.routes[0].overview_polyline.points);

                                    if (points != null)
                                    {
                                        ret.Points.Clear();
                                        ret.Points.AddRange(points);

                                        ret.Duration = RouteResult.routes[0].legs[0].duration.text;
                                        //ret.DistanceRoute = Math.Round((RouteResult.routes[0].legs[0].distance.value / 1000.0), 1);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        ret.ErrorCode = RouteResult.error.code;
                        ret.ErrorMessage = RouteResult.error.message;

                        RouteStatusCode code;

                        if (Enum.TryParse(RouteResult.error.status, false, out code))
                            ret.Status = code;
                    }
                }
            }
            catch (Exception ex)
            {
                ret = null;
                Debug.WriteLine("GetRoutePoints: " + ex);
            }

            return ret;
        }

        static readonly string RouteUrlFormatPointLatLng = "https://maps.{6}/maps/api/directions/json?origin={2},{3}&destination={4},{5}&mode=driving";
        static readonly string RouteUrlFormatStr = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";

        static readonly string WalkingStr = "&mra=ls&dirflg=w";
        static readonly string RouteWithoutHighwaysStr = "&mra=ls&dirflg=dh";
        static readonly string RouteStr = "&mra=ls&dirflg=d";

        #endregion

        #endregion

        #region GeocodingProvider Members

        public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
        {
            return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords, LanguageStr), out pointList);
        }

        public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetPoints(keywords, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList [0] : (PointLatLng?)null;
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="placemark"></param>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
        {
            throw new NotImplementedException("use GetPoints(string keywords...");
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="placemark"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
        {
            throw new NotImplementedException("use GetPoint(string keywords...");
        }

        public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
        {
            return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, LanguageStr), out placemarkList);
        }

        public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
        {
            List<Placemark> pointList;
            status = GetPlacemarks(location, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList [0] : (Placemark?)null;
        }

        #region -- internals --

        // The Coogle Geocoding API: http://tinyurl.com/cdlj889

        string MakeGeocoderUrl(string keywords, string language)
        {
            return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, ServerAPIs, Uri.EscapeDataString(keywords).Replace(' ', '+'), language);
        }

        string MakeReverseGeocoderUrl(PointLatLng pt, string language)
        {
            return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, ServerAPIs, pt.Lat, pt.Lng, language);
        }

        GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
        {
            var status = GeoCoderStatusCode.Unknow;

            pointList = null;

            try
            {
                string geo = GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(TTLCache)) : string.Empty;

                bool cache = false;

                if (string.IsNullOrEmpty(geo))
                {
                    string urls = url;

                    // Must provide either API key or Maps for Work credentials.
                    if (!string.IsNullOrEmpty(ClientId))
                    {
                        urls = GetSignedUri(url);
                    }
                    else if (!string.IsNullOrEmpty(ApiKey))
                    {
                        urls += "&key=" + ApiKey;
                    }

                    geo = GetContentUsingHttp(urls);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        cache = true;
                    }
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    StrucGeocode GeoResult = JsonConvert.DeserializeObject<StrucGeocode>(geo);

                    if (GeoResult != null)
                    {
                        if (GeoResult.status != "OK")
                        {
                            Debug.WriteLine("GetLatLngFromGeocoderUrl: " + GeoResult.status);
                        }
                        else if(GeoResult.status == "OK")
                        {
                            status = GeoCoderStatusCode.G_GEO_SUCCESS;

                            if (cache && GMaps.Instance.UseGeocoderCache)
                            {
                                Cache.Instance.SaveContent(url, CacheType.GeocoderCache, geo);
                            }

                            pointList = new List<PointLatLng>();

                            if (GeoResult.results != null && GeoResult.results.Count > 0)
                            {
                                for (int i = 0; i < GeoResult.results.Count; i++)
                                {
                                    pointList.Add(new PointLatLng(GeoResult.results[i].geometry.location.lat, GeoResult.results[i].geometry.location.lng));
                                }
                            }
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.ExceptionInCode;
                Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
            }

            return status;
        }

        GeoCoderStatusCode GetPlacemarkFromReverseGeocoderUrl(string url, out List<Placemark> placemarkList)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            placemarkList = null;

            try
            {
                string reverse = GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(TTLCache)) : string.Empty;

                bool cache = false;

                if (string.IsNullOrEmpty(reverse))
                {
                    string urls = url;

                    // Must provide either API key or Maps for Work credentials.
                    if (!string.IsNullOrEmpty(ClientId))
                    {
                        urls = GetSignedUri(url);
                    }
                    else if (!string.IsNullOrEmpty(ApiKey))
                    {
                        urls += "&key=" + ApiKey;
                    }

                    reverse = GetContentUsingHttp(urls);

                    if (!string.IsNullOrEmpty(reverse))
                    {
                        cache = true;
                    }
                }

                if (!string.IsNullOrEmpty(reverse))
                {
                    StrucGeocode GeoResult = JsonConvert.DeserializeObject<StrucGeocode>(reverse);

                    if (GeoResult != null)
                    {
                        if (GeoResult.status != "OK")
                        {
                            Debug.WriteLine("GetPlacemarkFromReverseGeocoderUrl: " + GeoResult.status);
                        }
                        else if (GeoResult.status == "OK")
                        {
                            status = GeoCoderStatusCode.G_GEO_SUCCESS;

                            if (cache && GMaps.Instance.UseGeocoderCache)
                            {
                                Cache.Instance.SaveContent(url, CacheType.GeocoderCache, reverse);
                            }

                            placemarkList = new List<Placemark>();

                            if (GeoResult.results != null && GeoResult.results.Count > 0)
                            {
                                Debug.WriteLine("---------------------");

                                for (int i = 0; i < GeoResult.results.Count; i++)
                                {
                                    var ret = new Placemark(GeoResult.results[i].formatted_address);

                                    Debug.WriteLine("formatted_address: [" + GeoResult.results[i].formatted_address + "]");

                                    if (GeoResult.results[i].types != null)
                                    {
                                        Debug.WriteLine("type: " + GeoResult.results[i].types);
                                    }

                                    if (GeoResult.results[i].address_components != null && GeoResult.results[i].address_components.Count > 0)
                                    {
                                        for (int j = 0; j < GeoResult.results[i].address_components.Count; j++)
                                        {
                                            if (GeoResult.results[i].address_components[j].types != null && GeoResult.results[i].address_components[j].types.Count > 0)
                                            {
                                                Debug.Write("Type: [" + GeoResult.results[i].address_components[j].types[0] + "], ");
                                                Debug.WriteLine("long_name: [" + GeoResult.results[i].address_components[j].long_name + "]");

                                                switch (GeoResult.results[i].address_components[j].types[0])
                                                {
                                                    case "street_number":
                                                        {
                                                            ret.StreetNumber = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "street_address":
                                                        {
                                                            ret.StreetAddress = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "route":
                                                        {
                                                            ret.ThoroughfareName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "postal_code":
                                                        {
                                                            ret.PostalCodeNumber = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "country":
                                                        {
                                                            ret.CountryName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "locality":
                                                        {
                                                            ret.LocalityName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "administrative_area_level_2":
                                                        {
                                                            ret.DistrictName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "administrative_area_level_1":
                                                        {
                                                            ret.AdministrativeAreaName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "administrative_area_level_3":
                                                        {
                                                            ret.SubAdministrativeAreaName = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;

                                                    case "neighborhood":
                                                        {
                                                            ret.Neighborhood = GeoResult.results[i].address_components[j].long_name;
                                                        }
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }

                                        placemarkList.Add(ret);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.ExceptionInCode;
                placemarkList = null;
                Debug.WriteLine("GetPlacemarkReverseGeocoderUrl: " + ex.ToString());
            }

            return status;
        }

        static readonly string ReverseGeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/json?latlng={1},{2}&language={3}&sensor=false";
        static readonly string GeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/json?address={1}&language={2}&sensor=false";

        #endregion

        #endregion

        #region DirectionsProvider Members

        public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            return GetDirectionsUrl(MakeDirectionsUrl(start, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
        }

        public DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            return GetDirectionsUrl(MakeDirectionsUrl(start, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="status"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="avoidHighways"></param>
        /// <param name="avoidTolls"></param>
        /// <param name="walkingMode"></param>
        /// <param name="sensor"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, string start, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            // TODO: add alternative directions

            throw new NotImplementedException();
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="status"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="avoidHighways"></param>
        /// <param name="avoidTolls"></param>
        /// <param name="walkingMode"></param>
        /// <param name="sensor"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, PointLatLng start, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            // TODO: add alternative directions

            throw new NotImplementedException();
        }

        public DirectionsStatusCode GetDirections(out GDirections direction, PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            return GetDirectionsUrl(MakeDirectionsUrl(start, wayPoints, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
        }

        public DirectionsStatusCode GetDirections(out GDirections direction, string start, IEnumerable<string> wayPoints, string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            return GetDirectionsUrl(MakeDirectionsUrl(start, wayPoints, end, LanguageStr, avoidHighways, avoidTolls, walkingMode, sensor, metric), out direction);
        }

        #region -- internals --

        // The Coogle Directions API: http://tinyurl.com/6vv4cac

        string MakeDirectionsUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial");     // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatPoint, start.Lat, start.Lng, end.Lat, end.Lng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs);
        }

        string MakeDirectionsUrl(string start, string end, string language, bool avoidHighways, bool walkingMode, bool avoidTolls, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 4
            string mt = "&units=" + (metric ? "metric" : "imperial");     // 5
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 6

            return string.Format(DirectionUrlFormatStr, start.Replace(' ', '+'), end.Replace(' ', '+'), sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs);
        }

        string MakeDirectionsUrl(PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial"); // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            string wpLatLng = string.Empty;
            int i = 0;

            foreach (var wp in wayPoints)
            {
                wpLatLng += string.Format(CultureInfo.InvariantCulture, i++ == 0 ? "{0},{1}" : "|{0},{1}", wp.Lat, wp.Lng);
            }

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypoint, start.Lat, start.Lng, wpLatLng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs, end.Lat, end.Lng);
        }

        string MakeDirectionsUrl(string start, IEnumerable<string> wayPoints, string end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial"); // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            string wpLatLng = string.Empty;
            int i = 0;

            foreach (var wp in wayPoints)
            {
                wpLatLng += string.Format(CultureInfo.InvariantCulture, i++ == 0 ? "{0}" : "|{0}", wp.Replace(' ', '+'));
            }

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypointStr, start.Replace(' ', '+'), wpLatLng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs, end.Replace(' ', '+'));
        }

        DirectionsStatusCode GetDirectionsUrl(string url, out GDirections direction)
        {
            DirectionsStatusCode ret = DirectionsStatusCode.UNKNOWN_ERROR;
            direction = null;

            try
            {                
                string kml = GMaps.Instance.UseDirectionsCache ? Cache.Instance.GetContent(url, CacheType.DirectionsCache, TimeSpan.FromHours(TTLCache)) : string.Empty;
                bool cache = false;

                if (string.IsNullOrEmpty(kml))
                {
                    kml = GetContentUsingHttp(!string.IsNullOrEmpty(ClientId) ? GetSignedUri(url) : (!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url));

                    if (!string.IsNullOrEmpty(kml))
                    {
                        cache = true;
                    }
                }

                if (!string.IsNullOrEmpty(kml))
                {
                    StrucDirection DirectionResult = JsonConvert.DeserializeObject<StrucDirection>(kml);

                    if (DirectionResult != null)
                    {
                        if (GMaps.Instance.UseDirectionsCache && cache)
                        {
                            Cache.Instance.SaveContent(url, CacheType.DirectionsCache, kml);
                        }

                        ret = DirectionResult.status; // (DirectionsStatusCode)Enum.Parse(typeof(DirectionsStatusCode), DirectionResult.status, false);

                        if (ret == DirectionsStatusCode.OK)
                        {
                            direction = new GDirections();

                            if (DirectionResult.routes != null && DirectionResult.routes.Count > 0)
                            {
                                direction.Summary = DirectionResult.routes[0].summary;
                                Debug.WriteLine("summary: " + direction.Summary);

                                if (DirectionResult.routes[0].copyrights != null)
                                {
                                    direction.Copyrights = DirectionResult.routes[0].copyrights;
                                    Debug.WriteLine("copyrights: " + direction.Copyrights);
                                }

                                if (DirectionResult.routes[0].overview_polyline != null && DirectionResult.routes[0].overview_polyline.points != null)
                                {
                                    direction.Route = new List<PointLatLng>();
                                    DecodePointsInto(direction.Route, DirectionResult.routes[0].overview_polyline.points);
                                }

                                if (DirectionResult.routes[0].legs != null && DirectionResult.routes[0].legs.Count > 0)
                                {
                                    direction.Duration = DirectionResult.routes[0].legs[0].duration.text;
                                    Debug.WriteLine("duration: " + direction.Duration);

                                    direction.DurationValue = (uint)DirectionResult.routes[0].legs[0].duration.value;
                                    Debug.WriteLine("value: " + direction.DurationValue);

                                    if (DirectionResult.routes[0].legs[0].distance != null)
                                    {
                                        direction.Distance = DirectionResult.routes[0].legs[0].distance.text;
                                        Debug.WriteLine("distance: " + direction.Distance);

                                        direction.DistanceValue = (uint)DirectionResult.routes[0].legs[0].distance.value;
                                        Debug.WriteLine("value: " + direction.DistanceValue);
                                    }

                                    if (DirectionResult.routes[0].legs[0].start_location != null)
                                    {
                                        direction.StartLocation.Lat = DirectionResult.routes[0].legs[0].start_location.lat;
                                        direction.StartLocation.Lng = DirectionResult.routes[0].legs[0].start_location.lng;
                                    }

                                    if (DirectionResult.routes[0].legs[0].end_location != null)
                                    {
                                        direction.EndLocation.Lat = DirectionResult.routes[0].legs[0].end_location.lat;
                                        direction.EndLocation.Lng = DirectionResult.routes[0].legs[0].end_location.lng;
                                    }

                                    if (DirectionResult.routes[0].legs[0].start_address != null)
                                    {
                                        direction.StartAddress = DirectionResult.routes[0].legs[0].start_address;
                                        Debug.WriteLine("start_address: " + direction.StartAddress);
                                    }

                                    if (DirectionResult.routes[0].legs[0].end_address != null)
                                    {
                                        direction.EndAddress = DirectionResult.routes[0].legs[0].end_address;
                                        Debug.WriteLine("end_address: " + direction.EndAddress);
                                    }

                                    direction.Steps = new List<GDirectionStep>();

                                    for (int i = 0; i < DirectionResult.routes[0].legs[0].steps.Count; i++)
                                    {
                                        GDirectionStep step = new GDirectionStep();
                                        Debug.WriteLine("----------------------");

                                        step.TravelMode = DirectionResult.routes[0].legs[0].steps[i].travel_mode;
                                        Debug.WriteLine("travel_mode: " + step.TravelMode);

                                        step.Duration = DirectionResult.routes[0].legs[0].steps[i].duration.text;
                                        Debug.WriteLine("duration: " + step.Duration);

                                        step.Distance = DirectionResult.routes[0].legs[0].steps[i].distance.text;
                                        Debug.WriteLine("distance: " + step.Distance);

                                        step.HtmlInstructions = DirectionResult.routes[0].legs[0].steps[i].html_instructions;
                                        Debug.WriteLine("html_instructions: " + step.HtmlInstructions);

                                        if (DirectionResult.routes[0].legs[0].steps[i].start_location != null)
                                        {
                                            step.StartLocation.Lat = DirectionResult.routes[0].legs[0].steps[i].start_location.lat;
                                            step.StartLocation.Lng = DirectionResult.routes[0].legs[0].steps[i].start_location.lng;
                                        }

                                        if (DirectionResult.routes[0].legs[0].steps[i].end_location != null)
                                        {
                                            step.EndLocation.Lat = DirectionResult.routes[0].legs[0].steps[i].end_location.lat;
                                            step.EndLocation.Lng = DirectionResult.routes[0].legs[0].steps[i].end_location.lng;
                                        }

                                        if (DirectionResult.routes[0].legs[0].steps[i].polyline != null && DirectionResult.routes[0].legs[0].steps[i].polyline.points != null)
                                        {
                                            step.Points = new List<PointLatLng>();
                                            DecodePointsInto(step.Points, DirectionResult.routes[0].legs[0].steps[i].polyline.points);
                                        }

                                        direction.Steps.Add(step);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                direction = null;
                ret = DirectionsStatusCode.ExceptionInCode;
                Debug.WriteLine("GetDirectionsUrl: " + ex);
            }
            return ret;
        }

        static void DecodePointsInto(List<PointLatLng> path, string encodedPath)
        {
            // https://github.com/googlemaps/google-maps-services-java/blob/master/src/main/java/com/google/maps/internal/PolylineEncoding.java
            int len = encodedPath.Length;
            int index = 0;
            int lat = 0;
            int lng = 0;

            while (index < len)
            {
                int result = 1;
                int shift = 0;
                int b;

                do
                {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f && index < len);

                lat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                result = 1;
                shift = 0;

                if (index < len)
                {
                    do
                    {
                        b = encodedPath[index++] - 63 - 1;
                        result += b << shift;
                        shift += 5;
                    } while (b >= 0x1f && index < len);

                    lng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
                }

                path.Add(new PointLatLng(lat * 1e-5, lng * 1e-5));
            }
        }

        static readonly string DirectionUrlFormatStr = "https://maps.{7}/maps/api/directions/json?origin={0}&destination={1}&sensor={2}&language={3}{4}{5}{6}";
        static readonly string DirectionUrlFormatPoint = "https://maps.{9}/maps/api/directions/json?origin={0},{1}&destination={2},{3}&sensor={4}&language={5}{6}{7}{8}";
        static readonly string DirectionUrlFormatWaypoint = "https://maps.{8}/maps/api/directions/json?origin={0},{1}&waypoints={2}&destination={9},{10}&sensor={3}&language={4}{5}{6}{7}";
        static readonly string DirectionUrlFormatWaypointStr = "https://maps.{7}/maps/api/directions/json?origin={0}&waypoints={1}&destination={8}&sensor={2}&language={3}{4}{5}{6}";

        #endregion

        #endregion

        #region RoadsProvider Members

        public virtual MapRoute GetRoadsRoute(List<PointLatLng> points, bool interpolate)
        {
            return GetRoadsRoute(MakeRoadsUrl(points, interpolate.ToString()));
        }

        public virtual MapRoute GetRoadsRoute(string points, bool interpolate)
        {
            return GetRoadsRoute(MakeRoadsUrl(points, interpolate.ToString()));
        }

        #region -- internals --

        string MakeRoadsUrl(List<PointLatLng> points, string interpolate)
        {
            string pointstr = "";

            foreach (var item in points)
            {
                pointstr += string.Format("{2}{0},{1}", item.Lat, item.Lng, (pointstr == "" ? "" : "|"));
            }

            return string.Format(RoadsUrlFormatStr, interpolate, pointstr, ServerAPIs);
        }

        string MakeRoadsUrl(string points, string interpolate)
        {
            return string.Format(RoadsUrlFormatStr, interpolate, points, Server);
        }

        MapRoute GetRoadsRoute(string url)
        {
            MapRoute ret = null;
            StrucRoads RoadsResult = null;

            try
            {
                string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(TTLCache)) : string.Empty;

                if (string.IsNullOrEmpty(route))
                {
                    route = GetContentUsingHttp(!string.IsNullOrEmpty(ClientId) ? GetSignedUri(url) : (!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url));

                    if (!string.IsNullOrEmpty(route))
                    {
                        RoadsResult = JsonConvert.DeserializeObject<StrucRoads>(route);

                        if (GMaps.Instance.UseRouteCache && RoadsResult != null && RoadsResult.error == null && RoadsResult.snappedPoints != null && RoadsResult.snappedPoints.Count > 0)
                        {
                            Cache.Instance.SaveContent(url, CacheType.RouteCache, route);
                        }
                    }
                }
                else
                {
                    RoadsResult = JsonConvert.DeserializeObject<StrucRoads>(route);
                }

                // parse values
                if (RoadsResult != null)
                {
                    ret = new MapRoute("Route");

                    ret.WarningMessage = RoadsResult.warningMessage;

                    if (RoadsResult.error == null)
                    {
                        if (RoadsResult.snappedPoints != null && RoadsResult.snappedPoints.Count > 0)
                        {
                            ret.Points.Clear();

                            foreach (var item in RoadsResult.snappedPoints)
                            {
                                ret.Points.Add(new PointLatLng(item.location.latitude, item.location.longitude));
                            }

                            ret.Status = RouteStatusCode.OK;
                        }
                    }
                    else
                    {
                        ret.ErrorCode = RoadsResult.error.code;
                        ret.ErrorMessage = RoadsResult.error.message;

                        RouteStatusCode code;

                        if (Enum.TryParse(RoadsResult.error.status, false, out code))
                            ret.Status = code;
                    }
                }
                
            }
            catch (Exception ex)
            {
                ret = null;
                Debug.WriteLine("GetRoutePoints: " + ex);
            }

            return ret;
        }

        static readonly string RoadsUrlFormatStr = "https://roads.{2}/v1/snapToRoads?interpolate={0}&path={1}";

        #endregion

        #endregion

        #region -- Maps API for Work --

        /// <summary>
        /// https://developers.google.com/maps/documentation/business/webservices/auth#how_do_i_get_my_signing_key
        /// To access the special features of the Google Maps API for Work you must provide a client ID
        /// when accessing any of the API libraries or services.
        /// When registering for Google Google Maps API for Work you will receive this client ID from Enterprise Support.
        /// All client IDs begin with a gme- prefix. Your client ID is passed as the value of the client parameter.
        /// Generally, you should store your private key someplace safe and read them into your code
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="privateKey"></param>
        public void SetEnterpriseCredentials(string clientId, string privateKey)
        {
            privateKey = privateKey.Replace("-", "+").Replace("_", "/");
            _privateKeyBytes = Convert.FromBase64String(privateKey);
            _clientId = clientId;
        }

        private byte[] _privateKeyBytes;

        private string _clientId = string.Empty;

        /// <summary>
        /// Your client ID. To access the special features of the Google Maps API for Work
        /// you must provide a client ID when accessing any of the API libraries or services.
        /// When registering for Google Google Maps API for Work you will receive this client ID
        /// from Enterprise Support. All client IDs begin with a gme- prefix.
        /// </summary>
        public string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        string GetSignedUri(Uri uri)
        {
            var builder = new UriBuilder(uri);
            builder.Query = builder.Query.Substring(1) + "&client=" + _clientId;
            uri = builder.Uri;
            string signature = GetSignature(uri);

            return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
        }

        string GetSignedUri(string url)
        {
            return GetSignedUri(new Uri(url));
        }

        string GetSignature(Uri uri)
        {
            byte[] encodedPathQuery = Encoding.ASCII.GetBytes(uri.LocalPath + uri.Query);
            var hashAlgorithm = new HMACSHA1(_privateKeyBytes);
            byte[] hashed = hashAlgorithm.ComputeHash(encodedPathQuery);
            return Convert.ToBase64String(hashed).Replace("+", "-").Replace("/", "_");
        } 
        #endregion
    }

    /// <summary>
    /// GoogleMap provider
    /// </summary>
    public class GoogleMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleMapProvider Instance;

        GoogleMapProvider()
        {
        }

        static GoogleMapProvider()
        {
            Instance = new GoogleMapProvider();
        }

        public string Version = "m@333000000";

        #region GMapProvider Members

        readonly Guid id = new Guid("D7287DA0-A7FF-405F-8166-B6BAF26D066C");
        public override Guid Id
        {
            get
            {
                return id;
            }
        }

        readonly string name = "GoogleMap";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            string sec1 = string.Empty; // after &x=...
            string sec2 = string.Empty; // after &zoom=...
            GetSecureWords(pos, out sec1, out sec2);

            return string.Format(UrlFormat, UrlFormatServer, GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec1, pos.Y, zoom, sec2, Server);
        }

        static readonly string UrlFormatServer = "mt";
        static readonly string UrlFormatRequest = "vt";
        static readonly string UrlFormat = "https://{0}{1}.{10}/maps/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
    }
}