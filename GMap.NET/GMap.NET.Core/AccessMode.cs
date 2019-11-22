namespace GMap.NET
{
    /// <summary>
    ///     tile access mode
    /// </summary>
    public enum AccessMode
    {
        /// <summary>
        ///     access only server
        /// </summary>
        ServerOnly,

        /// <summary>
        ///     access first server and caches locally
        /// </summary>
        ServerAndCache,

        /// <summary>
        ///     access only cache
        /// </summary>
        CacheOnly,
    }
}
