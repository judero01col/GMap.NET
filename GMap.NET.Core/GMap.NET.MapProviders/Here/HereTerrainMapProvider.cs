
namespace GMap.NET.MapProviders
{
   using System;

   /// <summary>
   /// HereTerrainMap provider
   /// </summary>
   public class HereTerrainMapProvider : HereMapProviderBase
   {
      public static readonly HereTerrainMapProvider Instance;

      HereTerrainMapProvider()
      {
      }

      static HereTerrainMapProvider()
      {
         Instance = new HereTerrainMapProvider();
      }

      #region GMapProvider Members

      readonly Guid id = new Guid("7267338C-445E-4E61-B8B8-82D0B7AAACC5");
      public override Guid Id
      {
         get
         {
            return id;
         }
      }

      readonly string name = "HereTerrainMap";
      public override string Name
      {
         get
         {
            return name;
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
            return string.Format(UrlFormat, UrlServerLetters[GetServerNum(pos, 4)], zoom, pos.X, pos.Y, AppId, AppCode);
        }

        static readonly string UrlFormat = "http://{0}.traffic.maps.cit.api.here.com/maptile/2.1/traffictile/newest/terrain.day/{1}/{2}/{3}/256/png8?app_id={4}&app_code={5}";
    }
}