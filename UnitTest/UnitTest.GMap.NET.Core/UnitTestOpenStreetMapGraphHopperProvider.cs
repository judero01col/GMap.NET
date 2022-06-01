using GMap.NET;
using GMap.NET.MapProviders;

namespace UnitTest.GMap.NET.Core
{
    [TestClass]
    public class UnitTestOpenStreetMapGraphHopperProvider
    {
        string ApiKey = "c2aa79b0-4ee1-4ca3-86e6-f6a013de26d2";

        [TestMethod]
        public void TestGetPoint()
        {
            var mapProvider = GMapProviders.OpenStreetMapGraphHopper;
            mapProvider.ApiKey = ApiKey;

            GeoCoderStatusCode status;

            var point = mapProvider.GetPoint("Barranquilla", out status);
                        
            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(point, null);
        }
        
        [TestMethod]
        public void TestGetPoints()
        {
            var mapProvider = GMapProviders.OpenStreetMapGraphHopper;
            mapProvider.ApiKey = ApiKey;

            GeoCoderStatusCode status;
            List<PointLatLng> pointList;

            status = mapProvider.GetPoints("Barranquilla", out pointList);

            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(pointList, null);
        }

        [TestMethod]
        public void TestGetPlacemark()
        {
            var mapProvider = GMapProviders.OpenStreetMapGraphHopper;
            mapProvider.ApiKey = ApiKey;

            GeoCoderStatusCode status;

            var location = new PointLatLng { Lat = 10.98335, Lng = -74.802319 };

            var point = mapProvider.GetPlacemark(location, out status);

            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(point, null);
        }

        [TestMethod]
        public void TestGetPlacemarks()
        {
            var mapProvider = GMapProviders.OpenStreetMapGraphHopper;
            mapProvider.ApiKey = ApiKey;

            List<Placemark> placemarkList;

            var location = new PointLatLng { Lat = 10.98335, Lng = -74.802319 };

            var status = mapProvider.GetPlacemarks(location, out placemarkList);

            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(placemarkList, null);
        }

        [TestMethod]
        public void TestGetRoute()
        {
            var mapProvider = GMapProviders.OpenStreetMapGraphHopper;
            mapProvider.ApiKey = ApiKey;

            var point1 = new PointLatLng(8.681495, 49.41461);
            var point2 = new PointLatLng(8.687872, 49.420318);

            var mapRoute = mapProvider.GetRoute(point1, point2, false, false, 15);

            Assert.AreEqual(mapRoute?.Status, RouteStatusCode.OK);
            Assert.AreNotEqual(mapRoute, null);            
        }
    }
}
