using System;
using System.Data.SqlClient;
using System.Diagnostics;
using GMap.NET.MapProviders;

namespace GMap.NET.CacheProviders
{
    /// <summary>
    ///     image cache for ms sql server
    ///     optimized by mmurfinsimmons@gmail.com
    /// </summary>
    public class MsSQLPureImageCache : PureImageCache, IDisposable
    {
        string _connectionString = string.Empty;

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;

                    if (Initialized)
                    {
                        Dispose();
                        Initialize();
                    }
                }
            }
        }

        SqlCommand _cmdInsert;
        SqlCommand _cmdFetch;
        SqlConnection _cnGet;
        SqlConnection _cnSet;

        bool _initialized;

        /// <summary>
        ///     is cache initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                lock (this)
                {
                    return _initialized;
                }
            }
            private set
            {
                lock (this)
                {
                    _initialized = value;
                }
            }
        }

        /// <summary>
        ///     inits connection to server
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            lock (this)
            {
                if (!Initialized)
                {
                    #region prepare mssql & cache table

                    try
                    {
                        // different connections so the multi-thread inserts and selects don't collide on open readers.
                        _cnGet = new SqlConnection(_connectionString);
                        _cnGet.Open();
                        _cnSet = new SqlConnection(_connectionString);
                        _cnSet.Open();

                        bool tableExists;
                        using (var cmd = new SqlCommand("select object_id('GMapNETcache')", _cnGet))
                        {
                            var objid = cmd.ExecuteScalar();
                            tableExists = objid != null && objid != DBNull.Value;
                        }

                        if (!tableExists)
                        {
                            using (var cmd = new SqlCommand(
                                "CREATE TABLE [GMapNETcache] ( \n"
                                + "   [Type] [int]   NOT NULL, \n"
                                + "   [Zoom] [int]   NOT NULL, \n"
                                + "   [X]    [int]   NOT NULL, \n"
                                + "   [Y]    [int]   NOT NULL, \n"
                                + "   [Tile] [image] NOT NULL, \n"
                                + "   CONSTRAINT [PK_GMapNETcache] PRIMARY KEY CLUSTERED (Type, Zoom, X, Y) \n"
                                + ")",
                                _cnGet))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        _cmdFetch =
                            new SqlCommand(
                                "SELECT [Tile] FROM [GMapNETcache] WITH (NOLOCK) WHERE [X]=@x AND [Y]=@y AND [Zoom]=@zoom AND [Type]=@type",
                                _cnGet);
                        _cmdFetch.Parameters.Add("@x", System.Data.SqlDbType.Int);
                        _cmdFetch.Parameters.Add("@y", System.Data.SqlDbType.Int);
                        _cmdFetch.Parameters.Add("@zoom", System.Data.SqlDbType.Int);
                        _cmdFetch.Parameters.Add("@type", System.Data.SqlDbType.Int);
                        _cmdFetch.Prepare();

                        _cmdInsert =
                            new SqlCommand(
                                "INSERT INTO [GMapNETcache] ( [X], [Y], [Zoom], [Type], [Tile] ) VALUES ( @x, @y, @zoom, @type, @tile )",
                                _cnSet);
                        _cmdInsert.Parameters.Add("@x", System.Data.SqlDbType.Int);
                        _cmdInsert.Parameters.Add("@y", System.Data.SqlDbType.Int);
                        _cmdInsert.Parameters.Add("@zoom", System.Data.SqlDbType.Int);
                        _cmdInsert.Parameters.Add("@type", System.Data.SqlDbType.Int);
                        _cmdInsert.Parameters.Add("@tile", System.Data.SqlDbType.Image); //, calcmaximgsize);
                        //can't prepare insert because of the IMAGE field having a variable size.  Could set it to some 'maximum' size?

                        Initialized = true;
                    }
                    catch (Exception ex)
                    {
                        _initialized = false;
                        Debug.WriteLine(ex.Message);
                    }

                    #endregion
                }

                return Initialized;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_cmdInsert)
            {
                if (_cmdInsert != null)
                {
                    _cmdInsert.Dispose();
                    _cmdInsert = null;
                }

                if (_cnSet != null)
                {
                    _cnSet.Dispose();
                    _cnSet = null;
                }
            }

            lock (_cmdFetch)
            {
                if (_cmdFetch != null)
                {
                    _cmdFetch.Dispose();
                    _cmdFetch = null;
                }

                if (_cnGet != null)
                {
                    _cnGet.Dispose();
                    _cnGet = null;
                }
            }

            Initialized = false;
        }

        #endregion

        #region PureImageCache Members

        public bool PutImageToCache(byte[] tile, int type, GPoint pos, int zoom)
        {
            bool ret = true;
            {
                if (Initialize())
                {
                    try
                    {
                        lock (_cmdInsert)
                        {
                            _cmdInsert.Parameters["@x"].Value = pos.X;
                            _cmdInsert.Parameters["@y"].Value = pos.Y;
                            _cmdInsert.Parameters["@zoom"].Value = zoom;
                            _cmdInsert.Parameters["@type"].Value = type;
                            _cmdInsert.Parameters["@tile"].Value = tile;
                            _cmdInsert.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        ret = false;
                        Dispose();
                    }
                }
            }
            return ret;
        }

        public PureImage GetImageFromCache(int type, GPoint pos, int zoom)
        {
            PureImage ret = null;
            {
                if (Initialize())
                {
                    try
                    {
                        object odata;
                        lock (_cmdFetch)
                        {
                            _cmdFetch.Parameters["@x"].Value = pos.X;
                            _cmdFetch.Parameters["@y"].Value = pos.Y;
                            _cmdFetch.Parameters["@zoom"].Value = zoom;
                            _cmdFetch.Parameters["@type"].Value = type;
                            odata = _cmdFetch.ExecuteScalar();
                        }

                        if (odata != null && odata != DBNull.Value)
                        {
                            var tile = (byte[])odata;
                            if (tile != null && tile.Length > 0)
                            {
                                if (GMapProvider.TileImageProxy != null)
                                {
                                    ret = GMapProvider.TileImageProxy.FromArray(tile);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        ret = null;
                        Dispose();
                    }
                }
            }
            return ret;
        }

        /// <summary>
        ///     NotImplemented
        /// </summary>
        /// <param name="date"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        int PureImageCache.DeleteOlderThan(DateTime date, int? type)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
