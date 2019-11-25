using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders
{
    /// <summary>
    ///     UMP-pcPL map Provider
    ///     http://ump.waw.pl/en/index.html
    ///     UMP-pcPL – is a free map dedicated for Garmin GPS devices, developed by users.You can find a plenty of such
    ///     maps(others than ours) which cover different areas of the world on Mapcenter and Mapcenter2,
    ///     or using search service.
    ///     UMP-pcPL covers "almost whole" Poland, and therefore we call it "pcPL" (prawie cała PL). Almost – because we have
    ///     not finished nor completed it yet, and the map is continuously growing.
    ///     Other countries are also included
    ///     Added by Lukasz Knap
    /// </summary>
    public class UMPMapProvider : GMapProvider
    {
        public static readonly UMPMapProvider Instance;

        UMPMapProvider()
        {
            RefererUrl = "http://ump.waw.pl/";
            Copyright = "Data by UMP-pcPL";
        }

        static UMPMapProvider()
        {
            Instance = new UMPMapProvider();
        }

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("E36E311E-256A-4639-9AF7-FEB7BDEA6ABE");

        public override string Name
        {
            get;
        } = "UMP";

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }

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
            return string.Format(UrlFormat, zoom, pos.X, pos.Y);
        }

        static readonly string UrlFormat = "http://tiles.ump.waw.pl/ump_tiles/{0}/{1}/{2}.png";
    }
}
