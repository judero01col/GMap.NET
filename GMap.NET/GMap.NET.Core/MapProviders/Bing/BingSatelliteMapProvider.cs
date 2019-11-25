using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     BingSatelliteMapProvider provider
    /// </summary>
    public class BingSatelliteMapProvider : BingMapProviderBase
    {
        public static readonly BingSatelliteMapProvider Instance;

        BingSatelliteMapProvider()
        {
        }

        static BingSatelliteMapProvider()
        {
            Instance = new BingSatelliteMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("3AC742DD-966B-4CFB-B67D-33E7F82F2D37");

        public override string Name
        {
            get;
        } = "BingSatelliteMap";

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        public override void OnInitialized()
        {
            base.OnInitialized();

            if (!DisableDynamicTileUrlFormat)
            {
                //UrlFormat[Aerial]: http://ecn.{subdomain}.tiles.virtualearth.net/tiles/a{quadkey}.jpeg?g=3179

                _urlDynamicFormat = GetTileUrl("Aerial");
                if (!string.IsNullOrEmpty(_urlDynamicFormat))
                {
                    _urlDynamicFormat = _urlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}");
                }
            }
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            string key = TileXYToQuadKey(pos.X, pos.Y, zoom);

            if (!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(_urlDynamicFormat))
            {
                return string.Format(_urlDynamicFormat, GetServerNum(pos, 4), key);
            }

            return string.Format(UrlFormat,
                GetServerNum(pos, 4),
                key,
                Version,
                language,
                ForceSessionIdOnTileAccess ? "&key=" + SessionId : string.Empty);
        }

        string _urlDynamicFormat = string.Empty;

        // http://ecn.t1.tiles.virtualearth.net/tiles/a12030003131321231.jpeg?g=875&mkt=en-us&n=z

        static readonly string UrlFormat =
            "http://ecn.t{0}.tiles.virtualearth.net/tiles/a{1}.jpeg?g={2}&mkt={3}&n=z{4}";
    }
}
