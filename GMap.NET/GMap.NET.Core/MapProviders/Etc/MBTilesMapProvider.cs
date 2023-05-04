using System;
using System.Collections.Generic;
using System.IO;
using GMap.NET.Projections;

namespace GMap.NET.MapProviders
{
#if SQLite && !MONO

    /// <summary>Map provider for MBTiles files (https://github.com/mapbox/mbtiles-spec/blob/master/1.3/spec.md)</summary>
    /// <remarks>Sample files are available at https://ftp.gwdg.de/pub/misc/openstreetmap/openseamap/charts/mbtiles/.</remarks>
    public class MBTilesMapProvider : GMapProvider
    {
        public static readonly MBTilesMapProvider Instance;

        private class MBTiles
        {
            private System.Data.SQLite.SQLiteConnection db = null;
            public Dictionary<string, string> metadata = new Dictionary<string, string>();

            public MBTiles(string file)
            {
                db = new System.Data.SQLite.SQLiteConnection(string.Format("Data Source=\"{0}\";Pooling=True", file));
                db.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT * FROM metadata;", db))
                {
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read()) metadata[rd.GetString(0)] = rd.GetString(1);
                    }
                }
            }

            ~MBTiles()
            {
                if (db != null)
                {
                    db.Close();
                    db = null;
                }
            }

            public PureImage GetImage(GPoint pos, int zoom)
            {
                PureImage ret = null;
                using (var cmd = new System.Data.SQLite.SQLiteCommand(string.Format("SELECT tile_data FROM tiles WHERE zoom_level={2} AND tile_column={0} AND tile_row={1} LIMIT 1", pos.X, ((long)Math.Pow(2, zoom) - 1) - pos.Y, zoom), db))
                {
                    using (var rd = cmd.ExecuteReader(System.Data.CommandBehavior.SequentialAccess))
                    {
                        if (rd.Read())
                        {
                            long length = rd.GetBytes(0, 0, null, 0, 0);
                            byte[] tile = new byte[length];
                            rd.GetBytes(0, 0, tile, 0, tile.Length);
                            {
                                if (GMapProvider.TileImageProxy != null)
                                {
                                    ret = GMapProvider.TileImageProxy.FromArray(tile);
                                }
                            }
                        }
                    }
                }
                return ret;
            }

            public int GetZoomMin()
            {
                try
                {
                    using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT MIN(zoom_level) AS zoom FROM tiles;", db))
                    {
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read()) return rd.GetInt32(0);
                        }
                    }
                }
                catch { }
                return -1;
            }

            public int GetZoomMax()
            {
                try
                {
                    using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT MAX(zoom_level) AS zoom FROM tiles;", db))
                    {
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read()) return rd.GetInt32(0);
                        }
                    }
                }
                catch { }
                return -1;
            }
        }

        private MBTiles source = null;

        MBTilesMapProvider()
        {
        }

        static MBTilesMapProvider()
        {
            Instance = new MBTilesMapProvider();
        }

        #region Properties
        /// <summary>
        /// The human-readable name of the tileset.
        /// </summary>
        public string DataName { get; private set; }

        /// <summary>
        /// The file format of the tile data: pbf, jpg, png, webp, or an IETF media type for other formats.
        /// </summary>
        /// <remarks>pbf as a format refers to gzip-compressed vector tile data in Mapbox Vector Tile format.</remarks>
        public string Format { get; private set; }

        /// <summary>
        /// The maximum extent of the rendered map area. Bounds must define an area covered by all zoom levels. The bounds are represented as WGS 84 latitude and longitude values, in the OpenLayers Bounds format (left, bottom, right, top). For example, the bounds of the full Earth, minus the poles, would be: -180.0,-85,180,85.
        /// </summary>
        public PointLatLng[] Bounds { get; private set; }

        /// <summary>
        /// The longitude, latitude, and zoom level of the default view of the map. Example: -122.1906,37.7599,11
        /// </summary>
        public PointLatLng CenterLocation { get; private set; }

        /// <summary>
        /// The longitude, latitude, and zoom level of the default view of the map. Example: -122.1906,37.7599,11
        /// </summary>
        public int CenterZoom { get; private set; }

        /// <summary>
        /// The lowest zoom level for which the tileset provides data
        /// </summary>
        public int MinZoom { get; private set; }

        /// <summary>
        /// The highest zoom level for which the tileset provides data
        /// </summary>
        public int MaxZoom { get; private set; }

        /// <summary>
        /// An attribution string, which explains the sources of data and/or style for the map.
        /// </summary>
        public string Attribution { get; private set; }

        /// <summary>
        /// A description of the tileset's content.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// overlay or baselayer
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The version of the tileset. This refers to a revision of the tileset itself, not of the MBTiles specification.
        /// </summary>
        public long Version { get; private set; }

        /// <summary>
        /// The metadata table MAY contain additional rows for tilesets that implement UTFGrid-based interaction or for other purposes.
        /// </summary>
        public Dictionary<string, string> Metadata
        {
            get
            {
                return source != null ? source.metadata : new Dictionary<string, string>();
            }
        }
        #endregion

        #region GMapProvider Members

        public override Guid Id
        {
            get;
        } = new Guid("CD2A114E-188C-423F-BBCC-FB7849333AE4");

        public override string Name
        {
            get;
        } = "MBTilesMapProvider";

        GMapProvider[] _overlays;

        public override GMapProvider[] Overlays
        {
            get
            {
                if (_overlays == null)
                {
                    _overlays = new GMapProvider[] { this };
                }

                return _overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            if (source == null) return null;
            if (zoom < MinZoom || zoom > MaxZoom) return null;
            return source.GetImage(pos, zoom);
        }

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }
        #endregion

        public bool Open(string MBTilesFilePath)
        {
            if (!File.Exists(MBTilesFilePath)) return false;
            try
            {
                source = new MBTiles(MBTilesFilePath);
                DataName = String.Empty;
                Format = String.Empty;
                Bounds = null;
                CenterLocation = PointLatLng.Empty;
                CenterZoom = -1;
                MinZoom = -1;
                MaxZoom = -1;
                Attribution = String.Empty;
                Description = String.Empty;
                Type = String.Empty;
                Version = 0;
                foreach (var kvp in source.metadata)
                {
                    switch (kvp.Key.ToLower())
                    {
                        case "name": DataName = kvp.Value; break;
                        case "format": Format = kvp.Value; break;
                        case "bounds":
                            string[] tmp1 = kvp.Value.Split(',');
                            if (tmp1.Length == 4)
                            {
                                Bounds = new PointLatLng[2]
                                {
                                    new PointLatLng(double.Parse(tmp1[3], System.Globalization.CultureInfo.InvariantCulture),double.Parse(tmp1[0], System.Globalization.CultureInfo.InvariantCulture)),
                                    new PointLatLng(double.Parse(tmp1[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(tmp1[2], System.Globalization.CultureInfo.InvariantCulture))
                                };
                            }
                            break;
                        case "center":
                            string[] tmp2 = kvp.Value.Split(',');
                            if (tmp2.Length == 3)
                            {
                                CenterLocation = new PointLatLng(double.Parse(tmp2[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(tmp2[0], System.Globalization.CultureInfo.InvariantCulture));
                                CenterZoom = int.Parse(tmp2[2]);
                            }
                            break;
                        case "minzoom": MinZoom = int.Parse(kvp.Value); break;
                        case "maxzoom": MaxZoom = int.Parse(kvp.Value); break;
                        case "attribution": Attribution = kvp.Value; break;
                        case "description": Description = kvp.Value; break;
                        case "type": Type = kvp.Value; break;
                        case "version": Version = long.Parse(kvp.Value); break;
                        default: break;
                    }

                }
                if (string.IsNullOrEmpty(DataName) || string.IsNullOrEmpty(Format))
                {
                    source = null;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("[MBTilesMapProvider] Metafields 'name' and 'format' are required!");
#endif
                    return false;
                }
                if (Format.ToLower() == "pbf" && !Metadata.ContainsKey("json"))
                {
                    source = null;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("[MBTilesMapProvider] If the format is pbf, the metadata table MUST contain 'json' row!");
#endif
                    return false;
                }
                if (MinZoom < 0) MinZoom = source.GetZoomMin();
                if (MaxZoom < 0) MaxZoom = source.GetZoomMax();
                if (Bounds != null && CenterLocation == PointLatLng.Empty) CenterLocation = new PointLatLng(Bounds[0].Lat - 0.5 * (Bounds[1].Lat - Bounds[0].Lat), Bounds[0].Lng + 0.5 * (Bounds[1].Lng - Bounds[0].Lng));
                if (CenterZoom < 0) CenterZoom = MinZoom + (MaxZoom - MinZoom) / 2;

                return true;
            }
            catch { return false; }
        }
    }

#endif
}
