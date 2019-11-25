using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     GoogleChinaTerrainMap provider
    /// </summary>
    public class GoogleChinaTerrainMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleChinaTerrainMapProvider Instance;

        GoogleChinaTerrainMapProvider()
        {
            RefererUrl = string.Format("http://ditu.{0}/", ServerChina);
        }

        static GoogleChinaTerrainMapProvider()
        {
            Instance = new GoogleChinaTerrainMapProvider();
        }

        public string Version = "t@132,r@298";

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("831EC3CC-B044-4097-B4B7-FC9D9F6D2CFC");

        public override string Name
        {
            get;
        } = "GoogleChinaTerrainMap";

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
                ChinaLanguage,
                pos.X,
                sec1,
                pos.Y,
                zoom,
                sec2,
                ServerChina);
        }

        static readonly string ChinaLanguage = "zh-CN";
        static readonly string UrlFormatServer = "mt";
        static readonly string UrlFormatRequest = "vt";
        static readonly string UrlFormat = "http://{0}{1}.{10}/{2}/lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}";
    }
}
