namespace MSR.CVE.BackMaker
{
    public struct IntPixel
    {
        public int b;
        public int g;
        public int r;
        public int a;

        public static IntPixel BlackPixel()
        {
            IntPixel result;
            result.b = 0;
            result.g = 0;
            result.r = 0;
            result.a = 0;
            return result;
        }

        public void addWeighted(double weight, PixelStruct pix)
        {
            r += (int)(weight * pix.r);
            g += (int)(weight * pix.g);
            b += (int)(weight * pix.b);
            a += (int)(weight * pix.a);
        }

        public PixelStruct AsPixel()
        {
            PixelStruct struct2;
            struct2.r = r > 0xff ? (byte)0xff : (byte)r;
            struct2.g = g > 0xff ? (byte)0xff : (byte)g;
            struct2.b = b > 0xff ? (byte)0xff : (byte)b;
            struct2.a = a > 0xff ? (byte)0xff : (byte)a;
            return struct2;
        }
    }
}
