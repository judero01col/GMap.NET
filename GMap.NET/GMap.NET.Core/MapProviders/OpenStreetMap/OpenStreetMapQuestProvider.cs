using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     OpenStreetMapQuest provider - http://wiki.openstreetmap.org/wiki/MapQuest
    /// </summary>
    public class OpenStreetMapQuestProvider : OpenStreetMapProviderBase
    {
        public static readonly OpenStreetMapQuestProvider Instance;

        OpenStreetMapQuestProvider()
        {
            Copyright = string.Format("© MapQuest - Map data ©{0} MapQuest, OpenStreetMap", DateTime.Today.Year);
        }

        static OpenStreetMapQuestProvider()
        {
            Instance = new OpenStreetMapQuestProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("D0A12840-973A-448B-B9C2-89B8A07DFF0F");

        public override string Name
        {
            get;
        } = "OpenStreetMapQuest";

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

        static readonly string UrlFormat = "http://otile{0}.mqcdn.com/tiles/1.0.0/osm/{1}/{2}/{3}.png";
    }
}
