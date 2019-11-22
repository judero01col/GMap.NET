using System;
using System.Drawing;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ContinuousCoordinateSystem : CoordinateSystemIfc
    {
        private const double unlockedViewWidth = 1.0;
        private const int tileSize = 512;
        private const int DEFAULT_ZOOM_LEVEL = 5;
        private const double defaultPixelsPerDegree = 512.0;
        private FlatAddressLayout addressLayout = new FlatAddressLayout();
        private RangeInt ZoomRange = new RangeInt(1, 20);
        private RangeDouble LatRange = new RangeDouble(-85.0, 85.0);
        private RangeDouble LonRange = new RangeDouble(-180.0, 180.0);
        public static ContinuousCoordinateSystem theInstance = new ContinuousCoordinateSystem();

        protected double pixelsPerDegree(int zoomLevel)
        {
            return 512.0 * Math.Pow(2.0, zoomLevel - 5);
        }

        protected double exactZoom(double pixelsPerDegree)
        {
            return 5.0 + Math.Log(pixelsPerDegree / 512.0, 2.0);
        }

        protected double pixelsToDegrees(long pixels, int zoomLevel)
        {
            return pixels / pixelsPerDegree(zoomLevel);
        }

        protected long degreesToPixels(double degrees, int zoomLevel)
        {
            return (long)Math.Round(pixelsPerDegree(zoomLevel) * degrees);
        }

        public LatLonZoom GetDefaultView()
        {
            return new LatLonZoom(0.5, 0.5, 5);
        }

        public LatLonZoom GetTranslationInLatLon(LatLonZoom oldCenterPosition, Point mouseMotion)
        {
            return new LatLonZoom(
                oldCenterPosition.lat + pixelsToDegrees(mouseMotion.Y, oldCenterPosition.zoom),
                oldCenterPosition.lon - pixelsToDegrees(mouseMotion.X, oldCenterPosition.zoom),
                oldCenterPosition.zoom);
        }

        public RangeInt GetZoomRange()
        {
            return ZoomRange;
        }

        public RangeDouble GetLatRange()
        {
            return LatRange;
        }

        public RangeDouble GetLonRange()
        {
            return LonRange;
        }

        public Size GetTileSize()
        {
            return new Size(512, 512);
        }

        public Point GetTranslationInPixels(LatLonZoom center, LatLon point)
        {
            return new Point((int)degreesToPixels(point.lon - center.lon, center.zoom),
                (int)degreesToPixels(center.lat - point.lat, center.zoom));
        }

        public LatLonZoom GetBestViewContaining(MapRectangle newBounds, Size size)
        {
            LatLon center = newBounds.GetCenter();
            int zoom = (int)(exactZoom(size.Height / (newBounds.lat1 - newBounds.lat0)) + 0.99);
            return new LatLonZoom(center.lat, center.lon, zoom);
        }

        private TileAddress GetTileContainingLatLonZoom(LatLonZoom llz)
        {
            Point64 point = new Point64(degreesToPixels(llz.lon, llz.zoom),
                degreesToPixels(llz.lat, llz.zoom));
            return new TileAddress((int)Math.Floor(point.X / 512.0),
                (int)Math.Floor(point.Y / 512.0),
                llz.zoom);
        }

        public LatLon GetLatLonOfTileNW(TileAddress ta)
        {
            double lat = pixelsToDegrees((ta.TileY + 1L) * 512L - 1L, ta.ZoomLevel);
            double lon = pixelsToDegrees(ta.TileX * 512L, ta.ZoomLevel);
            return new LatLon(lat, lon);
        }

        public MapRectangle GetUnclippedMapWindow(LatLonZoom centerPosition, Size windowSize)
        {
            return new MapRectangle(
                GetTranslationInLatLon(centerPosition, new Point(windowSize.Width / 2, windowSize.Height / 2))
                    .latlon,
                GetTranslationInLatLon(centerPosition,
                    new Point(-(windowSize.Width / 2 + 1), -(windowSize.Height / 2 + 1))).latlon);
        }

        public TileDisplayDescriptorArray GetTileArrayDescriptor(LatLonZoom center, Size windowSize)
        {
            TileDisplayDescriptorArray tileDisplayDescriptorArray = new TileDisplayDescriptorArray();
            MapRectangle unclippedMapWindow = GetUnclippedMapWindow(center, windowSize);
            tileDisplayDescriptorArray.topLeftTile =
                GetTileContainingLatLonZoom(new LatLonZoom(unclippedMapWindow.GetNW(), center.zoom));
            TileAddress tileContainingLatLonZoom =
                GetTileContainingLatLonZoom(new LatLonZoom(unclippedMapWindow.GetSE(), center.zoom));
            tileDisplayDescriptorArray.tileCountX =
                tileContainingLatLonZoom.TileX - tileDisplayDescriptorArray.topLeftTile.TileX + 1;
            tileDisplayDescriptorArray.tileCountY =
                -(tileContainingLatLonZoom.TileY - tileDisplayDescriptorArray.topLeftTile.TileY) + 1;
            Point64 point = new Point64(degreesToPixels(unclippedMapWindow.GetNW().lon, center.zoom),
                degreesToPixels(unclippedMapWindow.GetNW().lat, center.zoom));
            tileDisplayDescriptorArray.topLeftTileOffset = new Point(
                (int)(tileDisplayDescriptorArray.topLeftTile.TileX * 512 - point.X),
                (int)(point.Y - (tileDisplayDescriptorArray.topLeftTile.TileY + 1) * 512));
            tileDisplayDescriptorArray.layout = addressLayout;
            tileDisplayDescriptorArray.tileSize = GetTileSize();
            return tileDisplayDescriptorArray;
        }

        public ITileAddressLayout GetTileAddressLayout()
        {
            return addressLayout;
        }
    }
}
