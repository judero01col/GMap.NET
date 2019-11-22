using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MSR.CVE.BackMaker
{
    internal class FPDFBitmapCache
    {
        private bool created;
        private Size allocatedSize;
        private bool locked;
        private int bitmapHandle;

        [DllImport("fpdfview.dll")]
        private static extern int FPDFBitmap_Create(int width, int height, int alpha);

        [DllImport("fpdfview.dll")]
        private static extern void FPDF_RenderPageBitmap(int bitmap, int page, int start_x, int start_y, int size_x,
            int size_y, int rotate, int flags);

        [DllImport("fpdfview.dll")]
        private static extern void FPDFBitmap_FillRect(int bitmap, int left, int top, int width, int height, int red,
            int green, int blue, int alpha);

        [DllImport("fpdfview.dll")]
        private static extern IntPtr FPDFBitmap_GetBuffer(int bitmap);

        [DllImport("fpdfview.dll")]
        private static extern int FPDFBitmap_GetWidth(int bitmap);

        [DllImport("fpdfview.dll")]
        private static extern int FPDFBitmap_GetHeight(int bitmap);

        [DllImport("fpdfview.dll")]
        private static extern void FPDFBitmap_Destroy(int bitmap);

        public int Get(int width, int height)
        {
            D.Assert(!locked);
            if (!created || width != allocatedSize.Width || height != allocatedSize.Height)
            {
                dispose();
                allocatedSize = new Size(width, height);
                create();
            }

            D.Assert(created && width == allocatedSize.Width && height == allocatedSize.Height);
            locked = true;
            FPDFBitmap_FillRect(bitmapHandle,
                0,
                0,
                allocatedSize.Width,
                allocatedSize.Height,
                255,
                255,
                255,
                255);
            return bitmapHandle;
        }

        private void dispose()
        {
            D.Assert(!locked);
            if (created)
            {
                FPDFBitmap_Destroy(bitmapHandle);
                created = false;
            }
        }

        private void create()
        {
            D.Assert(!created);
            D.Assert(!locked);
            bitmapHandle = FPDFBitmap_Create(allocatedSize.Width, allocatedSize.Height, 1);
            created = true;
        }

        public void Release(int bitmapHandle)
        {
            D.Assert(locked);
            locked = false;
        }
    }
}
