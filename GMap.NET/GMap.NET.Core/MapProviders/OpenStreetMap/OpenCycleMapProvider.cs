using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     OpenCycleMap provider - http://www.opencyclemap.org
    /// </summary>
    public class OpenCycleMapProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenCycleMapProvider Instance;

        OpenCycleMapProvider()
        {
            RefererUrl = "http://www.opencyclemap.org/";
        }

        static OpenCycleMapProvider()
        {
            Instance = new OpenCycleMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("D7E1826E-EE1E-4441-9F15-7C2DE0FE0B0A");

        public override string Name
        {
            get;
        } = "OpenCycleMap";

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

        static readonly string UrlFormat = "http://{0}.tile.opencyclemap.org/cycle/{1}/{2}/{3}.png";
    }
}
