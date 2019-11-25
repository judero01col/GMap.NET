using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     GoogleChinaHybridMap provider
    /// </summary>
    public class GoogleChinaHybridMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleChinaHybridMapProvider Instance;

        GoogleChinaHybridMapProvider()
        {
            RefererUrl = string.Format("http://ditu.{0}/", ServerChina);
        }

        static GoogleChinaHybridMapProvider()
        {
            Instance = new GoogleChinaHybridMapProvider();
        }

        public string Version = "h@298";

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("B8A2A78D-1C49-45D0-8F03-9B95C83116B7");

        public override string Name
        {
            get;
        } = "GoogleChinaHybridMap";

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] {GoogleChinaSatelliteMapProvider.Instance, this};
                }

                return _overlays;
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

        static readonly string UrlFormat =
            "http://{0}{1}.{10}/{2}/imgtp=png32&lyrs={3}&hl={4}&gl=cn&x={5}{6}&y={7}&z={8}&s={9}";
    }
}
