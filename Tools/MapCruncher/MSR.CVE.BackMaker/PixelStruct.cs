using System.Drawing;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public struct PixelStruct
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;

        public static PixelStruct black()
        {
            PixelStruct result;
            result.b = 0;
            result.g = 0;
            result.r = 0;
            result.a = 0;
            return result;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(b);
            hash.Accumulate(g);
            hash.Accumulate(r);
            hash.Accumulate(a);
        }

        public Color ToColor()
        {
            return Color.FromArgb(a, r, g, b);
        }

        public static bool operator ==(PixelStruct p1, PixelStruct p2)
        {
            return p1.a == p2.a && p1.r == p2.r && p1.g == p2.g && p1.b == p2.b;
        }

        public static bool operator !=(PixelStruct p1, PixelStruct p2)
        {
            return !(p1 == p2);
        }

        public override bool Equals(object obj)
        {
            return obj is PixelStruct && this == (PixelStruct)obj;
        }

        public override int GetHashCode()
        {
            return a + 131 * (r + 131 * (g + 131 * b));
        }
    }
}
