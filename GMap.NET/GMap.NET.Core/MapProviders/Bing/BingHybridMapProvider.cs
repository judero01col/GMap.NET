using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     BingHybridMap provider
    /// </summary>
    public class BingHybridMapProvider : BingMapProviderBase
    {
        public static readonly BingHybridMapProvider Instance;

        BingHybridMapProvider()
        {
        }

        static BingHybridMapProvider()
        {
            Instance = new BingHybridMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("94E2FCB4-CAAC-45EA-A1F9-8147C4B14970");

        public override string Name
        {
            get;
        } = "BingHybridMap";

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
                //UrlFormat[AerialWithLabels]: http://ecn.{subdomain}.tiles.virtualearth.net/tiles/h{quadkey}.jpeg?g=3179&mkt={culture}

                _urlDynamicFormat = GetTileUrl("AerialWithLabels");
                if (!string.IsNullOrEmpty(_urlDynamicFormat))
                {
                    _urlDynamicFormat = _urlDynamicFormat.Replace("{subdomain}", "t{0}").Replace("{quadkey}", "{1}")
                        .Replace("{culture}", "{2}");
                }
            }
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            string key = TileXYToQuadKey(pos.X, pos.Y, zoom);

            if (!DisableDynamicTileUrlFormat && !string.IsNullOrEmpty(_urlDynamicFormat))
            {
                return string.Format(_urlDynamicFormat, GetServerNum(pos, 4), key, language);
            }

            return string.Format(UrlFormat,
                GetServerNum(pos, 4),
                key,
                Version,
                language,
                ForceSessionIdOnTileAccess ? "&key=" + SessionId : string.Empty);
        }

        string _urlDynamicFormat = string.Empty;

        // http://ecn.dynamic.t3.tiles.virtualearth.net/comp/CompositionHandler/12030012020203?mkt=en-us&it=A,G,L&n=z

        static readonly string UrlFormat =
            "http://ecn.t{0}.tiles.virtualearth.net/tiles/h{1}.jpeg?g={2}&mkt={3}&n=z{4}";
    }
}
