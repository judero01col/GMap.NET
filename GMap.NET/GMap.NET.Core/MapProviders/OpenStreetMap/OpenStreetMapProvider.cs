using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Xml;
using GMap.NET.Internals;
using GMap.NET.Projections;
using Newtonsoft.Json;

namespace GMap.NET.MapProviders
{
    public abstract class OpenStreetMapProviderBase : GMapProvider, RoutingProvider, GeocodingProvider
    {
        public OpenStreetMapProviderBase()
        {
            MaxZoom = null;
            //Tile usage policy of openstreetmap (https://operations.osmfoundation.org/policies/tiles/) define as optional and providing referer 
            //only if one valid available. by providing http://www.openstreetmap.org/ a 418 error is given by the server.
            //RefererUrl = "http://www.openstreetmap.org/";
            Copyright = string.Format("© OpenStreetMap - Map data ©{0} OpenStreetMap", DateTime.Today.Year);
        }

        public readonly string ServerLetters = "abc";
        public int MinExpectedRank = 0;

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

        public override GMapProvider[] Overlays
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region GMapRoutingProvider Members

        public virtual MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode,
            int zoom)
        {
            //List<PointLatLng> points = GetRoutePoints(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
            //MapRoute route = points != null ? new MapRoute(points, walkingMode ? WalkingStr : DrivingStr) : null;
            //return route;
            return GetRoute(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
        }

        /// <summary>
        ///     NotImplemented
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="avoidHighways"></param>
        /// <param name="walkingMode"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public virtual MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
        {
            throw new NotImplementedException("use GetRoute(PointLatLng start, PointLatLng end...");
        }

        #region -- internals --

        string MakeRoutingUrl(PointLatLng start, PointLatLng end, string travelType)
        {
            return string.Format(CultureInfo.InvariantCulture,
                RoutingUrlFormat,
                start.Lat,
                start.Lng,
                end.Lat,
                end.Lng,
                travelType);
        }

        MapRoute GetRoute(string url)
        {
            throw new NotImplementedException("use GetRoute(PointLatLng start, PointLatLng end...");
        }

        static readonly string RoutingUrlFormat =
            "http://www.yournavigation.org/api/1.0/gosmore.php?format=kml&flat={0}&flon={1}&tlat={2}&tlon={3}&v={4}&fast=1&layer=mapnik";

        static readonly string TravelTypeFoot = "foot";
        static readonly string TravelTypeMotorCar = "motorcar";

        static readonly string WalkingStr = "Walking";
        static readonly string DrivingStr = "Driving";

        #endregion

        #endregion

        #region GeocodingProvider Members

        public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
        {
            // http://nominatim.openstreetmap.org/search?q=lithuania,vilnius&format=xml
            return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), out pointList);
        }

        public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetPoints(keywords, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
        {
            // http://nominatim.openstreetmap.org/search?street=&city=vilnius&county=&state=&country=lithuania&postalcode=&format=xml
            return GetLatLngFromGeocoderUrl(MakeDetailedGeocoderUrl(placemark), out pointList);
        }

        public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetPoints(placemark, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
        {
            GeoCoderStatusCode status;
            placemarkList = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out status);
            return status;
        }

        public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
        {
            List<Placemark> pointList;
            pointList = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out status);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (Placemark?)null;
        }

        #region -- internals --

        string MakeGeocoderUrl(string keywords)
        {
            return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'));
        }

        string MakeDetailedGeocoderUrl(Placemark placemark)
        {
            string street = String.Join(" ", new[] {placemark.HouseNo, placemark.ThoroughfareName}).Trim();

            return string.Format(GeocoderDetailedUrlFormat,
                street.Replace(' ', '+'),
                placemark.LocalityName.Replace(' ', '+'),
                placemark.SubAdministrativeAreaName.Replace(' ', '+'),
                placemark.AdministrativeAreaName.Replace(' ', '+'),
                placemark.CountryName.Replace(' ', '+'),
                placemark.PostalCodeNumber.Replace(' ', '+'));
        }

        string MakeReverseGeocoderUrl(PointLatLng pt)
        {
            return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng);
        }

        GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
        {
            var status = GeoCoderStatusCode.UNKNOWN_ERROR;
            pointList = null;
            List<OpenStreetMapGeocodeStruct> routeResult = null;

            try
            {
                string geo = GMaps.Instance.UseGeocoderCache
                    ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(TTLCache))
                    : string.Empty;

                if (string.IsNullOrEmpty(geo))
                {
                    geo = GetContentUsingHttp(url);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        routeResult = JsonConvert.DeserializeObject<List<OpenStreetMapGeocodeStruct>>(geo);

                        if (GMaps.Instance.UseGeocoderCache && routeResult != null && routeResult.Count > 0)
                        {
                            Cache.Instance.SaveContent(url, CacheType.GeocoderCache, geo);
                        }
                    }
                }
                else
                {
                    routeResult = JsonConvert.DeserializeObject<List<OpenStreetMapGeocodeStruct>>(geo);
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    pointList = new List<PointLatLng>();

                    foreach (var item in routeResult)
                    {
                        pointList.Add(new PointLatLng { Lat = item.lat, Lng = item.lon });
                    }

                    status = GeoCoderStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.EXCEPTION_IN_CODE;
                Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
            }

            return status;
        }
                

        List<Placemark> GetPlacemarkFromReverseGeocoderUrl(string url, out GeoCoderStatusCode status)
        {
            status = GeoCoderStatusCode.UNKNOWN_ERROR;
            List<Placemark> ret = null;
            OpenStreetMapGeocodeStruct routeResult = null;

            try
            {
                string geo = GMaps.Instance.UsePlacemarkCache
                    ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(TTLCache))
                    : string.Empty;

                if (string.IsNullOrEmpty(geo))
                {
                    geo = GetContentUsingHttp(url);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        routeResult = JsonConvert.DeserializeObject<OpenStreetMapGeocodeStruct>(geo);

                        if (GMaps.Instance.UsePlacemarkCache && routeResult != null)
                        {
                            Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, geo);
                        }
                    }
                }
                else
                {
                    routeResult = JsonConvert.DeserializeObject<OpenStreetMapGeocodeStruct>(geo);
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    ret = new List<Placemark>();

                    var p = new Placemark(routeResult.display_name);

                    p = new Placemark
                    {
                        PlacemarkId = routeResult.place_id,
                        Address = routeResult.address.ToString(),
                        CountryName = routeResult.address.country,
                        CountryNameCode = routeResult.address.country_code,
                        PostalCodeNumber = routeResult.address.postcode,
                        AdministrativeAreaName = routeResult.address.state,
                        SubAdministrativeAreaName = routeResult.address.city,
                        LocalityName = routeResult.address.suburb,
                        ThoroughfareName = routeResult.address.road
                    };

                    ret.Add(p);

                    status = GeoCoderStatusCode.OK;                    
                }
            }
            catch (Exception ex)
            {
                ret = null;
                status = GeoCoderStatusCode.EXCEPTION_IN_CODE;
                Debug.WriteLine("GetPlacemarkFromReverseGeocoderUrl: " + ex);
            }

            return ret;
        }

        static readonly string ReverseGeocoderUrlFormat =
            "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}&zoom=18&addressdetails=1";

        static readonly string GeocoderUrlFormat = "https://nominatim.openstreetmap.org/search?q={0}&format=json";

        static readonly string GeocoderDetailedUrlFormat =
            "https://nominatim.openstreetmap.org/search?street={0}&city={1}&county={2}&state={3}&country={4}&postalcode={5}&format=json";

        #endregion

        #endregion
    }

    /// <summary>
    ///     OpenStreetMap provider - http://www.openstreetmap.org/
    /// </summary>
    public class OpenStreetMapProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenStreetMapProvider Instance;

        OpenStreetMapProvider()
        {
        }

        static OpenStreetMapProvider()
        {
            Instance = new OpenStreetMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("0521335C-92EC-47A8-98A5-6FD333DDA9C0");

        public override string Name
        {
            get;
        } = "OpenStreetMap";

        public string YoursClientName { get; set; }

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] {this};
                }

                return _overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, string.Empty);

            return GetTileImageUsingHttp(url);
        }

        protected override void InitializeWebRequest(WebRequest request)
        {
            base.InitializeWebRequest(request);

            if (!string.IsNullOrEmpty(YoursClientName))
            {
                request.Headers.Add("X-Yours-client", YoursClientName);
            }
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            char letter = ServerLetters[GetServerNum(pos, 3)];
            return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
        }

        static readonly string UrlFormat = "https://{0}.tile.openstreetmap.org/{1}/{2}/{3}.png";
    }
}
