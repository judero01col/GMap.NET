using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class TileAddressEnumerator : IEnumerator<TileDisplayDescriptor>, IDisposable, IEnumerator
    {
        private TileDisplayDescriptorArray tad;
        private bool reset = true;
        private TileDisplayDescriptor current;
        private int screenTileCountX;
        private int screenTileCountY;

        public TileDisplayDescriptor Current
        {
            get
            {
                return current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public TileAddressEnumerator(TileDisplayDescriptorArray tad)
        {
            this.tad = tad;
        }

        public void Dispose()
        {
        }

        private void SetPaintLocation()
        {
            current.paintLocation = new Rectangle(
                tad.topLeftTileOffset.X + tad.tileSize.Width * screenTileCountX,
                tad.topLeftTileOffset.Y + tad.tileSize.Height * screenTileCountY,
                tad.tileSize.Width,
                tad.tileSize.Height);
        }

        public bool MoveNext()
        {
            bool result;
            if (tad.tileCountX <= 0 || tad.tileCountY <= 0)
            {
                result = false;
            }
            else
            {
                if (reset)
                {
                    current.tileAddress = new TileAddress(tad.topLeftTile);
                    reset = false;
                    screenTileCountX = 0;
                    screenTileCountY = 0;
                    SetPaintLocation();
                    result = true;
                }
                else
                {
                    if (screenTileCountX == tad.tileCountX - 1)
                    {
                        if (screenTileCountY == tad.tileCountY - 1)
                        {
                            result = false;
                        }
                        else
                        {
                            current.tileAddress.TileY =
                                tad.layout.YValueOneTileSouth(current.tileAddress);
                            screenTileCountY++;
                            current.tileAddress.TileX = tad.topLeftTile.TileX;
                            screenTileCountX = 0;
                            result = true;
                        }
                    }
                    else
                    {
                        current.tileAddress.TileX = tad.layout.XValueOneTileEast(current.tileAddress);
                        screenTileCountX++;
                        result = true;
                    }

                    SetPaintLocation();
                }
            }

            return result;
        }

        public void Reset()
        {
            reset = true;
        }
    }
}
