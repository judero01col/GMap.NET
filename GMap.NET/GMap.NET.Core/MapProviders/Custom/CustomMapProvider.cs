using System;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders
{
    public class CustomMapProvider : GMapProvider
    {
        public static readonly CustomMapProvider Instance;

        CustomMapProvider()
        {
            RefererUrl = CustomServerUrl;
        }

        static CustomMapProvider()
        {
            Instance = new CustomMapProvider();
        }

        #region GMapProvider Members

        readonly Guid id = new Guid("BEAB409B-6ED0-443F-B8E3-E6CC6F019F66");

        public override Guid Id
        {
            get
            {
                return id;
            }
        }

        readonly string name = "CustomMapProvider";

        public override string Name
        {
            get
            {
                return name;
            }
        }

        GMapProvider[] overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] {this};
                }

                return overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, string.Empty);

            return GetTileImageUsingHttp(url);
        }

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }

        public string CustomServerUrl = string.Empty;

        public string CustomServerLetters = string.Empty;

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            string url = CustomServerUrl;

            url = url.Replace("{l}", "{0}");
            url = url.Replace("{z}", "{1}");
            url = url.Replace("{x}", "{2}");
            url = url.Replace("{y}", "{3}");

            string letter = string.IsNullOrEmpty(CustomServerLetters)
                ? ""
                : CustomServerLetters[GetServerNum(pos, 3)].ToString();
            return string.Format(url, letter, zoom, pos.X, pos.Y);
        }
    }
}
