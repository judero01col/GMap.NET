namespace GMap.NET.Internals
{
    /// <summary>
    ///     cache queue item
    /// </summary>
    internal struct CacheQueueItem
    {
        public RawTile Tile;
        public byte[] Img;
        public CacheUsage CacheType;

        public CacheQueueItem(RawTile tile, byte[] img, CacheUsage cacheType)
        {
            Tile = tile;
            this.Img = img;
            CacheType = cacheType;
        }

        public override string ToString()
        {
            return Tile + ", CacheType:" + CacheType;
        }

        public void Clear()
        {
            Img = null;
        }
    }

    internal enum CacheUsage
    {
        First = 2,
        Second = 4,
        Both = First | Second
    }
}
