using System;
using System.Globalization;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     MapBender provider, WMS demo http://www.mapbender.org/
    /// </summary>
    public class MapBenderWMSProvider : GMapProvider
    {
        public static readonly MapBenderWMSProvider Instance;

        MapBenderWMSProvider()
        {
        }

        static MapBenderWMSProvider()
        {
            Instance = new MapBenderWMSProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("45742F8D-B552-4CAF-89AE-F20951BBDB2B");

        public override string Name
        {
            get;
        } = "MapBender, WMS demo";

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

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
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
            var px1 = Projection.FromTileXYToPixel(pos);
            var px2 = px1;

            px1.Offset(0, Projection.TileSize.Height);
            var p1 = Projection.FromPixelToLatLng(px1, zoom);

            px2.Offset(Projection.TileSize.Width, 0);
            var p2 = Projection.FromPixelToLatLng(px2, zoom);

            string ret = string.Format(CultureInfo.InvariantCulture,
                UrlFormat,
                p1.Lng,
                p1.Lat,
                p2.Lng,
                p2.Lat,
                Projection.TileSize.Width,
                Projection.TileSize.Height);

            return ret;
        }

        static readonly string UrlFormat =
            "http://mapbender.wheregroup.com/cgi-bin/mapserv?map=/data/umn/osm/osm_basic.map&VERSION=1.1.1&REQUEST=GetMap&SERVICE=WMS&LAYERS=OSM_Basic&styles=&bbox={0},{1},{2},{3}&width={4}&height={5}&srs=EPSG:4326&format=image/png";
    }
}
