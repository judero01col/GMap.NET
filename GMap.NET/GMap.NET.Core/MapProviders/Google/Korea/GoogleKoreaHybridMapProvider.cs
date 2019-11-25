using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     GoogleKoreaHybridMap provider
    /// </summary>
    public class GoogleKoreaHybridMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleKoreaHybridMapProvider Instance;

        GoogleKoreaHybridMapProvider()
        {
        }

        static GoogleKoreaHybridMapProvider()
        {
            Instance = new GoogleKoreaHybridMapProvider();
        }

        public string Version = "kr1t.12";

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("41A91842-04BC-442B-9AC8-042156238A5B");

        public override string Name
        {
            get;
        } = "GoogleKoreaHybridMap";

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] {GoogleKoreaSatelliteMapProvider.Instance, this};
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
            GetSecureWords(pos, out string sec1, out string sec2);

            return string.Format(UrlFormat,
                UrlFormatServer,
                GetServerNum(pos, 4),
                UrlFormatRequest,
                Version,
                language,
                pos.X,
                sec1,
                pos.Y,
                zoom,
                sec2,
                ServerKorea);
        }

        static readonly string UrlFormatServer = "mt";
        static readonly string UrlFormatRequest = "mt";
        static readonly string UrlFormat = "https://{0}{1}.{10}/{2}/v={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
    }
}
