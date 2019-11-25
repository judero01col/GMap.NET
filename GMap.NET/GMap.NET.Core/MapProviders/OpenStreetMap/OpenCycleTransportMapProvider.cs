using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     OpenCycleMap Transport provider - http://www.opencyclemap.org
    /// </summary>
    public class OpenCycleTransportMapProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenCycleTransportMapProvider Instance;

        OpenCycleTransportMapProvider()
        {
            RefererUrl = "http://www.opencyclemap.org/";
        }

        static OpenCycleTransportMapProvider()
        {
            Instance = new OpenCycleTransportMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("AF66DF88-AD25-43A9-8F82-56FCA49A748A");

        public override string Name
        {
            get;
        } = "OpenCycleTransportMap";

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] {this};
                }

                return _overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, string.Empty);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            char letter = ServerLetters[GetServerNum(pos, 3)];
            return string.Format(UrlFormat, letter, zoom, pos.X, pos.Y);
        }

        static readonly string UrlFormat = "http://{0}.tile2.opencyclemap.org/transport/{1}/{2}/{3}.png";
    }
}
