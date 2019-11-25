using System;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     ArcGIS_World_Physical_Map provider, http://server.arcgisonline.com
    /// </summary>
    public class ArcGIS_World_Physical_MapProvider : ArcGISMapMercatorProviderBase
    {
        public static readonly ArcGIS_World_Physical_MapProvider Instance;

        ArcGIS_World_Physical_MapProvider()
        {
        }

        static ArcGIS_World_Physical_MapProvider()
        {
            Instance = new ArcGIS_World_Physical_MapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("0C0E73E3-5EA6-4F08-901C-AE85BCB1BFC8");

        public override string Name
        {
            get;
        } = "ArcGIS_World_Physical_Map";

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            // http://services.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/2/0/2.jpg

            return string.Format(UrlFormat, zoom, pos.Y, pos.X);
        }

        static readonly string UrlFormat =
            "http://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{0}/{1}/{2}";
    }
}
