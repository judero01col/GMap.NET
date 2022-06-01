using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using GMap.NET.Internals;
using Newtonsoft.Json;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     OpenStreetMapGraphHopper provider - http://graphhopper.com
    /// </summary>
    public class OpenStreetMapGraphHopperProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenStreetMapGraphHopperProvider Instance;

        OpenStreetMapGraphHopperProvider()
        {
            RefererUrl = "http://openseamap.org/";
        }

        static OpenStreetMapGraphHopperProvider()
        {
            Instance = new OpenStreetMapGraphHopperProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("FAACDE73-4B90-5AE6-BB4A-ADE4F3545559");

        public override string Name
        {
            get;
        } = "OpenStreetMapGraphHopper";

        public string ApiKey = string.Empty;

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] { OpenStreetMapProvider.Instance, this };
                }

                return _overlays;
            }
        }

        #endregion

        #region GMapRoutingProvider Members

        public override MapRoute GetRoute(PointLatLng start, PointLatLng end, bool avoidHighways, bool walkingMode,
           int zoom)
        {
            return GetRoute(MakeRoutingUrl(start, end, walkingMode ? TravelTypeFoot : TravelTypeMotorCar));
        }

        public override MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom)
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
            MapRoute ret = null;
            OpenStreetMapGraphHopperStruct routeResult = null;

            try
            {
                string route = GMaps.Instance.UseRouteCache
                    ? Cache.Instance.GetContent(url, CacheType.RouteCache, TimeSpan.FromHours(TTLCache))
                    : string.Empty;

                if (string.IsNullOrEmpty(route))
                {
                    route = GetContentUsingHttp(!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url);

                    if (!string.IsNullOrEmpty(route))
                    {
                        routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperStruct>(route);

                        if (GMaps.Instance.UseRouteCache && routeResult != null &&
                            routeResult.paths != null && routeResult.paths.Count > 0)
                        {
                            Cache.Instance.SaveContent(url, CacheType.RouteCache, route);
                        }
                    }
                }
                else
                {
                    routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperStruct>(route);
                }

                if (!string.IsNullOrEmpty(route))
                {
                    ret = new MapRoute("Route");

                    if (routeResult != null)
                    {
                        if (routeResult.paths != null && routeResult.paths.Count > 0)
                        {
                            ret.Status = RouteStatusCode.OK;

                            ret.Duration = routeResult.paths[0].time.ToString();

                            var points = new List<PointLatLng>();
                            PureProjection.PolylineDecode(points, routeResult.paths[0].points);
                            ret.Points.AddRange(points);

                            foreach (var item in routeResult.paths[0].instructions)
                            {
                                ret.Instructions.Add(item.text);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ret = null;
                Debug.WriteLine("GetRoute: " + ex);
            }

            return ret;
        }

        static readonly string TravelTypeFoot = "foot";
        static readonly string TravelTypeMotorCar = "car";

        static readonly string RoutingUrlFormat = "https://graphhopper.com/api/1/route?point={0},{1}&point={2},{3}&vehicle={4}&type=json";

        #endregion

        #endregion

        #region GeocodingProvider Members

        public new GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
        {
            return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords), out pointList);
        }

        public new PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
        {
            List<PointLatLng> pointList;
            status = GetPoints(keywords, out pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        public new GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
        {
            throw new NotImplementedException("use GetPoint");
            //return GetLatLngFromGeocoderUrl(MakeDetailedGeocoderUrl(placemark), out pointList);
        }

        public new PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
        {
            throw new NotImplementedException("use GetPoint");
            //List<PointLatLng> pointList;
            //status = GetPoints(placemark, out pointList);
            //return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        public new GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
        {
            GeoCoderStatusCode status;
            placemarkList = GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location), out status);
            return status;
        }

        public new Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
        {            
            List<Placemark> placemarkList;
            status = GetPlacemarks(location, out placemarkList);
            return placemarkList != null && placemarkList.Count > 0 ? placemarkList[0] : (Placemark?)null;
        }

        #region -- internals --

        string MakeGeocoderUrl(string keywords)
        {
            return string.Format(GeocoderUrlFormat, keywords.Replace(' ', '+'));
        }

        string MakeReverseGeocoderUrl(PointLatLng pt)
        {
            return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, pt.Lat, pt.Lng);
        }

        GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
        {
            var status = GeoCoderStatusCode.UNKNOWN_ERROR;
            pointList = null;
            OpenStreetMapGraphHopperGeocodeStruct routeResult = null;

            try
            {
                string geo = GMaps.Instance.UseGeocoderCache
                    ? Cache.Instance.GetContent(url, CacheType.GeocoderCache, TimeSpan.FromHours(TTLCache))
                    : string.Empty;

                if (string.IsNullOrEmpty(geo))
                {
                    geo = GetContentUsingHttp(!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeStruct>(geo);

                        if (GMaps.Instance.UseRouteCache && routeResult != null &&
                            routeResult.hits != null && routeResult.hits.Count > 0)
                        {
                            Cache.Instance.SaveContent(url, CacheType.GeocoderCache, geo);
                        }
                    }
                }
                else
                {
                    routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeStruct>(geo);
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    pointList = new List<PointLatLng>();

                    foreach (var item in routeResult.hits)
                    {
                        pointList.Add(new PointLatLng(item.point.lat, item.point.lng));
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
            OpenStreetMapGraphHopperGeocodeStruct routeResult = null;

            try
            {
                string geo = GMaps.Instance.UsePlacemarkCache
                    ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache, TimeSpan.FromHours(TTLCache))
                    : string.Empty;

                if (string.IsNullOrEmpty(geo))
                {
                    geo = GetContentUsingHttp(!string.IsNullOrEmpty(ApiKey) ? url + "&key=" + ApiKey : url);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeStruct>(geo);

                        if (GMaps.Instance.UsePlacemarkCache && routeResult != null &&
                            routeResult.hits != null && routeResult.hits.Count > 0)
                        {
                            Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, geo);
                        }
                    }
                }
                else
                {
                    routeResult = JsonConvert.DeserializeObject<OpenStreetMapGraphHopperGeocodeStruct>(geo);
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    ret = new List<Placemark>();

                    foreach (var item in routeResult.hits)
                    {
                        var p = new Placemark(item.name);

                        p = new Placemark
                        {
                            PlacemarkId = item.osm_id,
                            Address = item.name,
                            CountryName = item.country,
                            CountryNameCode = item.countrycode,
                            PostalCodeNumber = item.postcode,
                            AdministrativeAreaName = item.state,
                            SubAdministrativeAreaName = item.city,
                            LocalityName = null,
                            ThoroughfareName = null
                        };

                        ret.Add(p);
                    }

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

        static readonly string ReverseGeocoderUrlFormat = "https://graphhopper.com/api/1/geocode?point={0},{1}&locale=en&reverse=true";

        static readonly string GeocoderUrlFormat = "https://graphhopper.com/api/1/geocode?q={0}&locale=en";       

        #endregion

        #endregion
    }
}
