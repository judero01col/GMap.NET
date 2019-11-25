using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     CzechSatelliteMap provider, http://www.mapy.cz/
    /// </summary>
    public class CzechSatelliteMapProviderOld : CzechMapProviderBaseOld
    {
        public static readonly CzechSatelliteMapProviderOld Instance;

        CzechSatelliteMapProviderOld()
        {
        }

        static CzechSatelliteMapProviderOld()
        {
            Instance = new CzechSatelliteMapProviderOld();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("7846D655-5F9C-4042-8652-60B6BF629C3C");

        public override string Name
        {
            get;
        } = "CzechSatelliteOldMap";

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            //http://m3.mapserver.mapy.cz/ophoto/9_7a80000_7a80000

            long xx = pos.X << (28 - zoom);
            long yy = ((long)Math.Pow(2.0, zoom) - 1 - pos.Y) << (28 - zoom);

            return string.Format(UrlFormat, GetServerNum(pos, 3) + 1, zoom, xx, yy);
        }

        static readonly string UrlFormat = "http://m{0}.mapserver.mapy.cz/ophoto/{1}_{2:x7}_{3:x7}";
    }
}
