using System;
using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class TileAddress : IComparable, Parameter, IRobustlyHashable, Present, IDisposable
    {
        private const string TileXTag = "TileX";
        private const string TileYTag = "TileY";
        private const string ZoomTag = "Zoom";
        public int TileX;
        public int TileY;
        public int ZoomLevel;

        public TileAddress(int TileX, int TileY, int ZoomLevel)
        {
            this.TileX = TileX;
            this.TileY = TileY;
            this.ZoomLevel = ZoomLevel;
        }

        public TileAddress(TileAddress proto)
        {
            TileX = proto.TileX;
            TileY = proto.TileY;
            ZoomLevel = proto.ZoomLevel;
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return string.Format("tile_X{0}_Y{1}_Z{2}", TileX, TileY, ZoomLevel);
        }

        public override int GetHashCode()
        {
            return TileX.GetHashCode() ^ TileY.GetHashCode() ^ ZoomLevel.GetHashCode();
        }

        public override bool Equals(object o2)
        {
            TileAddress tileAddress = (TileAddress)o2;
            return tileAddress != null && TileX == tileAddress.TileX && TileY == tileAddress.TileY &&
                   ZoomLevel == tileAddress.ZoomLevel;
        }

        public int CompareTo(object obj)
        {
            TileAddress tileAddress = (TileAddress)obj;
            int num = ZoomLevel.CompareTo(tileAddress.ZoomLevel);
            if (num != 0)
            {
                return num;
            }

            int num2 = TileY.CompareTo(tileAddress.TileY);
            if (num2 != 0)
            {
                return num2;
            }

            int num3 = TileX.CompareTo(tileAddress.TileX);
            if (num3 != 0)
            {
                return num3;
            }

            return 0;
        }

        public void WriteXMLToAttributes(XmlTextWriter writer)
        {
            writer.WriteAttributeString("TileX", TileX.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("TileY", TileY.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Zoom", ZoomLevel.ToString(CultureInfo.InvariantCulture));
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(TileX);
            hash.Accumulate(TileY);
            hash.Accumulate(ZoomLevel);
        }

        public Present Duplicate(string refCredit)
        {
            return new TileAddress(this);
        }
    }
}
