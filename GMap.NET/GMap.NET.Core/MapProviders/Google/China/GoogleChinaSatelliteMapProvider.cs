using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     GoogleChinaSatelliteMap provider
    /// </summary>
    public class GoogleChinaSatelliteMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleChinaSatelliteMapProvider Instance;

        GoogleChinaSatelliteMapProvider()
        {
            RefererUrl = string.Format("http://ditu.{0}/", ServerChina);
        }

        static GoogleChinaSatelliteMapProvider()
        {
            Instance = new GoogleChinaSatelliteMapProvider();
        }

        public string Version = "s@170";

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("543009AC-3379-4893-B580-DBE6372B1753");

        public override string Name
        {
            get;
        } = "GoogleChinaSatelliteMap";

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // sec1: after &x=...
            // sec2: after &zoom=...
            GetSecureWords(pos, out string sec1, out string sec2);

            return string.Format(UrlFormat,
                UrlFormatServer,
                GetServerNum(pos, 4),
                UrlFormatRequest,
                Version,
                pos.X,
                sec1,
                pos.Y,
                zoom,
                sec2,
                ServerChina);
        }

        static readonly string UrlFormatServer = "mt";
        static readonly string UrlFormatRequest = "vt";
        static readonly string UrlFormat = "http://{0}{1}.{9}/{2}/lyrs={3}&gl=cn&x={4}{5}&y={6}&z={7}&s={8}";
    }
}
