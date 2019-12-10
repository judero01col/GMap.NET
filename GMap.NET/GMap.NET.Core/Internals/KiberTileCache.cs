using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GMap.NET.Internals
{
    /// <summary>
    ///     kiber speed memory cache for tiles with history support ;}
    /// </summary>
    internal class KiberTileCache : Dictionary<RawTile, byte[]>
    {
        public KiberTileCache() : base(new RawTileComparer())
        {
        }

        readonly Queue<RawTile> _queue = new Queue<RawTile>();

        /// <summary>
        ///     the amount of tiles in MB to keep in memory, default: 22MB, if each ~100Kb it's ~222 tiles
        /// </summary>
        public int MemoryCacheCapacity = 22;

        long _memoryCacheSize;

        /// <summary>
        ///     current memory cache size in MB
        /// </summary>
        public double MemoryCacheSize
        {
            get
            {
                return _memoryCacheSize / 1048576.0;
            }
        }

        public new void Add(RawTile key, byte[] value)
        {
            _queue.Enqueue(key);
            base.Add(key, value);

            _memoryCacheSize += value.Length;
        }

        // do not allow directly removal of elements
        private new void Remove(RawTile key)
        {
        }

        public new void Clear()
        {
            _queue.Clear();
            base.Clear();
            _memoryCacheSize = 0;
        }

        internal void RemoveMemoryOverload()
        {
            while (MemoryCacheSize > MemoryCacheCapacity)
            {
                if (Keys.Count > 0 && _queue.Count > 0)
                {
                    var first = _queue.Dequeue();
                    try
                    {
                        var m = base[first];
                        {
                            base.Remove(first);
                            _memoryCacheSize -= m.Length;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("RemoveMemoryOverload: " + ex);
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
