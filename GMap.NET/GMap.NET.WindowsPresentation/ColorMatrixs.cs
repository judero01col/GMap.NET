﻿#if NETFRAMEWORK && !NET5_0_OR_GREATER
using System.Drawing.Imaging;
#endif

namespace GMap.NET.WindowsPresentation
{
#if NETFRAMEWORK && !NET5_0_OR_GREATER
    public static class ColorMatrixs
    {
        public static readonly ColorMatrix GrayScale = new ColorMatrix(new[]
        {
            new[] {.3f, .3f, .3f, 0, 0}, new[] {.59f, .59f, .59f, 0, 0},
            new[] {.11f, .11f, .11f, 0, 0}, new float[] {0, 0, 0, 1, 0}, new float[] {0, 0, 0, 0, 1}
        });

        public static readonly ColorMatrix Negative = new ColorMatrix(new[]
        {
            new float[] {-1, 0, 0, 0, 0}, new float[] {0, -1, 0, 0, 0}, new float[] {0, 0, -1, 0, 0},
            new float[] {0, 0, 0, 1, 0}, new float[] {1, 1, 1, 0, 1}
        });
    }
#endif
}
