using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     OpenStreetMapQuestSatellite provider - http://wiki.openstreetmap.org/wiki/MapQuest
    /// </summary>
    public class OpenStreetMapQuestSatelliteProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenStreetMapQuestSatelliteProvider Instance;

        OpenStreetMapQuestSatelliteProvider()
        {
            Copyright = string.Format("© MapQuest - Map data ©{0} MapQuest, OpenStreetMap", DateTime.Today.Year);
        }

        static OpenStreetMapQuestSatelliteProvider()
        {
            Instance = new OpenStreetMapQuestSatelliteProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("E590D3B1-37F4-442B-9395-ADB035627F67");

        public override string Name
        {
            get;
        } = "OpenStreetMapQuestSatellite";

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
            return string.Format(UrlFormat, GetServerNum(pos, 3) + 1, zoom, pos.X, pos.Y);
        }

        static readonly string UrlFormat = "http://otile{0}.mqcdn.com/tiles/1.0.0/sat/{1}/{2}/{3}.jpg";
    }
}
