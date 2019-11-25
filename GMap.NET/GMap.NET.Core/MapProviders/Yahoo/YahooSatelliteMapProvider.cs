using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     YahooSatelliteMap provider
    /// </summary>
    public class YahooSatelliteMapProvider : YahooMapProviderBase
    {
        public static readonly YahooSatelliteMapProvider Instance;

        YahooSatelliteMapProvider()
        {
        }

        static YahooSatelliteMapProvider()
        {
            Instance = new YahooSatelliteMapProvider();
        }

        public string Version = "1.9";

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("55D71878-913F-4320-B5B6-B4167A3F148F");

        public override string Name
        {
            get;
        } = "YahooSatelliteMap";

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // http://maps3.yimg.com/ae/ximg?v=1.9&t=a&s=256&.intl=en&x=15&y=7&z=7&r=1

            return string.Format(UrlFormat,
                GetServerNum(pos, 2) + 1,
                Version,
                language,
                pos.X,
                ((1 << zoom) >> 1) - 1 - pos.Y,
                zoom + 1);
        }

        static readonly string UrlFormat =
            "http://maps{0}.yimg.com/ae/ximg?v={1}&t=a&s=256&.intl={2}&x={3}&y={4}&z={5}&r=1";
    }
}
