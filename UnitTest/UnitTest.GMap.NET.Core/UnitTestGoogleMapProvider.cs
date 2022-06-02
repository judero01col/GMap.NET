using GMap.NET;
using GMap.NET.MapProviders;

namespace UnitTest.GMap.NET.Core
{
    [TestClass]
    public class UnitTestGoogleMapProvider
    {
        string ApiKey = "AIzaSyDn8qjiDcnGHOriIrmCnbHs8RK4h_WoGpg";

        [TestMethod]
        public void TestGetPoint()
        {
            var mapProvider = GMapProviders.GoogleMap;
            mapProvider.ApiKey = ApiKey;

            GeoCoderStatusCode status;

            var Point = mapProvider.GetPoint("Barranquilla", out status);

            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(Point, null);            
        }

        [TestMethod]
        public void TestGetPoints()
        {
            var mapProvider = GMapProviders.GoogleMap;
            mapProvider.ApiKey = ApiKey;
            
            List<PointLatLng> pointList;
            var status = mapProvider.GetPoints("Barranquilla", out pointList);

            Assert.AreEqual(status, GeoCoderStatusCode.OK);
            Assert.AreNotEqual(pointList, null);
        }

        [TestMethod]
        public void TestGetRoute()
        {
            var mapProvider = GMapProviders.GoogleMap;
            mapProvider.ApiKey = ApiKey;

            var Data = new List<PointLatLng> 
            {
                new PointLatLng(10.981233, -74.798384),
                new PointLatLng(10.981086, -74.798009),
            };

            var point1 = new PointLatLng(10.981233, -74.798384);
            var point2 = new PointLatLng(10.981897, -74.792719);

            var mapRoute = mapProvider.GetRoute(point1, point2, false, false, 15);

            Assert.AreEqual(mapRoute?.Status, RouteStatusCode.OK);
            Assert.AreNotEqual(mapRoute, null);            
        }
    }
}
