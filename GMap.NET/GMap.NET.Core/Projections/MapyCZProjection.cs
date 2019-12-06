using System;
using static System.Math;

namespace GMap.NET.Projections
{
    /// <summary>
    ///     GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS
    ///     84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]]
    ///     PROJCS["Mapy.cz",GEOGCS["GCS_WGS_1984",DATUM["D_WGS_1984",SPHEROID["WGS_1984",6378137,298.257223563]],PRIMEM["Greenwich",0],UNIT["Degree",0.017453292519943295]],PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",0],PARAMETER["central_meridian",15],PARAMETER["scale_factor",0.9996],PARAMETER["false_easting",134400000],PARAMETER["false_northing",-41600000],UNIT["1/32meter",0.03125]]
    /// </summary>
    public class MapyCZProjection : PureProjection
    {
        public static readonly MapyCZProjection Instance = new MapyCZProjection();

        static readonly double MinLatitude = 26;
        static readonly double MaxLatitude = 76;
        static readonly double MinLongitude = -26;
        static readonly double MaxLongitude = 38;

        #region -- Common --

        static int GetLCM(int zone)
        {
            if (zone < 1 || zone > 60)
            {
                throw new Exception("MapyCZProjection: UTM Zone number is not between 1 and 60.");
            }
            else
            {
                return zone * 6 - 183;
            }
        }

        static double Roundoff(double xx, double yy)
        {
            double x = xx;
            double y = yy;
            x = Round(x * Pow(10, y)) / Pow(10, y);
            return x;
        }

        static readonly double UTMSIZE = 2;
        static readonly double UNITS = 1;

        #endregion

        #region -- WGSToMapyCZ --

        public long[] WGSToPP(double la, double lo)
        {
            var utmEE = WGSToUTM(DegreesToRadians(la), DegreesToRadians(lo), 33);
            var pp = UTMEEToPP(utmEE[0], utmEE[1]);
            return pp;
        }

        static long[] UTMEEToPP(double east, double north)
        {
            double x = (Round(east) - -3700000.0) * Pow(2, 5);
            double y = (Round(north) - 1300000.0) * Pow(2, 5);

            return new[] {(long)x, (long)y};
        }

        double[] WGSToUTM(double la, double lo, int zone)
        {
            double latrad = la;
            double lonrad = lo;
            double latddd = RadiansToDegrees(la);
            double londdd = RadiansToDegrees(lo);

            float k = 0.9996f;
            double a = Axis;
            double f = Flattening;
            double b = a * (1.0 - f);
            double e2 = (a * a - b * b) / (a * a);
            double e = Sqrt(e2);
            double ei2 = (a * a - b * b) / (b * b);
            double ei = Sqrt(ei2);
            double n = (a - b) / (a + b);
            double g = a * (1.0 - n) * (1.0 - n * n) * (1.0 + 9 / 4.0 * n * n + 255.0 / 64.0 * Pow(n, 4)) *
                       (PI / 180.0);
            double w = londdd - (zone * 6 - 183);
            w = DegreesToRadians(w);
            double t = Tan(latrad);
            double rho = a * (1.0 - e2) / Pow(1.0 - e2 * Sin(latrad) * Sin(latrad), 3 / 2.0);
            double nu = a / Sqrt(1.0 - e2 * Sin(latrad) * Sin(latrad));
            double psi = nu / rho;
            double coslat = Cos(latrad);
            double sinlat = Sin(latrad);
            double a0 = 1 - e2 / 4.0 - 3 * e2 * e2 / 64.0 - 5 * Pow(e2, 3) / 256.0;
            double a2 = 3 / 8.0 * (e2 + e2 * e2 / 4.0 + 15 * Pow(e2, 3) / 128.0);
            double a4 = 15 / 256.0 * (e2 * e2 + 3 * Pow(e2, 3) / 4.0);
            double a6 = 35 * Pow(e2, 3) / 3072.0;
            double m = a * (a0 * latrad - a2 * Sin(2 * latrad) + a4 * Sin(4 * latrad) - a6 * Sin(6 * latrad));
            double eterm1 = w * w / 6.0 * coslat * coslat * (psi - t * t);
            double eterm2 = Pow(w, 4) / 120.0 * Pow(coslat, 4) *
                            (4 * Pow(psi, 3) * (1.0 - 6 * t * t) + psi * psi * (1.0 + 8 * t * t) - psi * 2 * t * t +
                             Pow(t, 4));
            double eterm3 = Pow(w, 6) / 5040.0 * Pow(coslat, 6) * (61.0 - 479 * t * t + 179 * Pow(t, 4) - Pow(t, 6));
            double dE = k * nu * w * coslat * (1.0 + eterm1 + eterm2 + eterm3);
            double east = 500000.0 + dE / UNITS;
            east = Roundoff(east, UTMSIZE);
            double nterm1 = w * w / 2.0 * nu * sinlat * coslat;
            double nterm2 = Pow(w, 4) / 24.0 * nu * sinlat * Pow(coslat, 3) * (4 * psi * psi + psi - t * t);
            double nterm3 = Pow(w, 6) / 720.0 * nu * sinlat * Pow(coslat, 5) *
                            (8 * Pow(psi, 4) * (11.0 - 24 * t * t) - 28 * Pow(psi, 3) * (1.0 - 6 * t * t) +
                             psi * psi * (1.0 - 32 * t * t) - psi * 2 * t * t + Pow(t, 4));
            double nterm4 = Pow(w, 8) / 40320.0 * nu * sinlat * Pow(coslat, 7) *
                            (1385.0 - 3111 * t * t + 543 * Pow(t, 4) - Pow(t, 6));
            double dN = k * (m + nterm1 + nterm2 + nterm3 + nterm4);
            double north = 0.0 + dN / UNITS;
            north = Roundoff(north, UTMSIZE);

            return new[] {east, north, zone};
        }

        #endregion

        #region -- MapyCZToWGS --

        public double[] PPToWGS(double x, double y)
        {
            var utmEE = PPToUTMEE(x, y);
            var ret = UTMToWGS(utmEE[0], utmEE[1], 33);
            return ret;
        }

        double[] PPToUTMEE(double x, double y)
        {
            double north = y * Pow(2, -5) + 1300000.0;
            double east = x * Pow(2, -5) + -3700000.0;
            east = Roundoff(east, UTMSIZE);
            north = Roundoff(north, UTMSIZE);

            return new[] {east, north};
        }

        double[] UTMToWGS(double eastIn, double northIn, int zone)
        {
            float k = 0.9996f;
            double a = Axis;
            double f = Flattening;
            double b = a * (1.0 - f);
            double e2 = (a * a - b * b) / (a * a);
            double e = Sqrt(e2);
            double ei2 = (a * a - b * b) / (b * b);
            double ei = Sqrt(ei2);
            double n = (a - b) / (a + b);
            double g = a * (1.0 - n) * (1.0 - n * n) * (1.0 + 9 / 4.0 * n * n + 255 / 64.0 * Pow(n, 4)) * (PI / 180.0);
            double north = (northIn - 0) * UNITS;
            double east = (eastIn - 500000.0) * UNITS;
            double m = north / k;
            double sigma = m * PI / (180.0 * g);
            double footlat = sigma + (3 * n / 2.0 - 27 * Pow(n, 3) / 32.0) * Sin(2 * sigma) +
                             (21 * n * n / 16.0 - 55 * Pow(n, 4) / 32.0) * Sin(4 * sigma) +
                             151 * Pow(n, 3) / 96.0 * Sin(6 * sigma) + 1097 * Pow(n, 4) / 512.0 * Sin(8 * sigma);
            double rho = a * (1.0 - e2) / Pow(1.0 - e2 * Sin(footlat) * Sin(footlat), 3 / 2.0);
            double nu = a / Sqrt(1.0 - e2 * Sin(footlat) * Sin(footlat));
            double psi = nu / rho;
            double t = Tan(footlat);
            double x = east / (k * nu);
            double laterm1 = t / (k * rho) * (east * x / 2.0);
            double laterm2 = t / (k * rho) * (east * Pow(x, 3) / 24.0) *
                             (-4 * psi * psi + 9 * psi * (1 - t * t) + 12 * t * t);
            double laterm3 = t / (k * rho) * (east * Pow(x, 5) / 720.0) *
                             (8 * Pow(psi, 4) * (11 - 24 * t * t) - 12 * Pow(psi, 3) * (21.0 - 71 * t * t) +
                              15 * psi * psi * (15.0 - 98 * t * t + 15 * Pow(t, 4)) +
                              180 * psi * (5 * t * t - 3 * Pow(t, 4)) + 360 * Pow(t, 4));
            double laterm4 = t / (k * rho) * (east * Pow(x, 7) / 40320.0) *
                             (1385.0 + 3633 * t * t + 4095 * Pow(t, 4) + 1575 * Pow(t, 6));
            double latrad = footlat - laterm1 + laterm2 - laterm3 + laterm4;
            double lat = RadiansToDegrees(latrad);
            double seclat = 1 / Cos(footlat);
            double loterm1 = x * seclat;
            double loterm2 = Pow(x, 3) / 6.0 * seclat * (psi + 2 * t * t);
            double loterm3 = Pow(x, 5) / 120.0 * seclat * (-4 * Pow(psi, 3) * (1 - 6 * t * t) +
                                                           psi * psi * (9 - 68 * t * t) + 72 * psi * t * t +
                                                           24 * Pow(t, 4));
            double loterm4 = Pow(x, 7) / 5040.0 * seclat * (61.0 + 662 * t * t + 1320 * Pow(t, 4) + 720 * Pow(t, 6));
            double w = loterm1 - loterm2 + loterm3 - loterm4;
            double longrad = DegreesToRadians(GetLCM(zone)) + w;
            double lon = RadiansToDegrees(longrad);

            return new[] {lat, lon, latrad, longrad};
        }

        #endregion

        public override RectLatLng Bounds
        {
            get
            {
                return RectLatLng.FromLTRB(MinLongitude, MaxLatitude, MaxLongitude, MinLatitude);
            }
        }

        public override GSize TileSize
        {
            get
            {
                return new GSize(256, 256);
            }
        }

        public override double Axis
        {
            get
            {
                return 6378137;
            }
        }

        public override double Flattening
        {
            get
            {
                return 1.0 / 298.257223563;
            }
        }

        public override GPoint FromLatLngToPixel(double lat, double lng, int zoom)
        {
            var ret = GPoint.Empty;

            lat = Clip(lat, MinLatitude, MaxLatitude);
            lng = Clip(lng, MinLongitude, MaxLongitude);

            var size = GetTileMatrixSizePixel(zoom);
            {
                var l = WGSToPP(lat, lng);
                ret.X = l[0] >> (20 - zoom);
                ret.Y = size.Height - (l[1] >> (20 - zoom));
            }
            return ret;
        }

        public override PointLatLng FromPixelToLatLng(long x, long y, int zoom)
        {
            var ret = PointLatLng.Empty;

            var size = GetTileMatrixSizePixel(zoom);

            long oX = x << (20 - zoom);
            long oY = (size.Height - y) << (20 - zoom);
            {
                var l = PPToWGS(oX, oY);
                ret.Lat = Clip(l[0], MinLatitude, MaxLatitude);
                ret.Lng = Clip(l[1], MinLongitude, MaxLongitude);
            }
            return ret;
        }

        public override GSize GetTileMatrixSizeXY(int zoom)
        {
            return new GSize((long)Pow(2, zoom), (long)Pow(2, zoom));
        }

        public override GSize GetTileMatrixSizePixel(int zoom)
        {
            var s = GetTileMatrixSizeXY(zoom);
            return new GSize(s.Width << 8, s.Height << 8);
        }

        public override GSize GetTileMatrixMinXY(int zoom)
        {
            long wh = zoom > 3 ? 3 * (long)Pow(2, zoom - 4) : 1;
            return new GSize(wh, wh);
        }

        public override GSize GetTileMatrixMaxXY(int zoom)
        {
            long wh = (long)Pow(2, zoom) - (long)Pow(2, zoom - 2);
            return new GSize(wh, wh);
        }
    }
}
