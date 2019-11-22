using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class FoxitViewer : IFoxitViewer, IDisposable
    {
        private enum FoxitInvocationMode
        {
            UsingAppGraphicsContext,
            UsingFoxitBitmap,
            ReusingFoxitBitmap,
            UsingLocalGraphicsContext
        }

        private int docHandle;
        private int pageHandle;
        private int numPages;
        private FoxitLibWorker foxitLib = FoxitLibManager.theInstance.foxitLib;
        private string filename;
        internal static object FoxitMutex = new object();

        private static ResourceCounter foxitResourceCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("foxit", 5);

        public FoxitViewer(string filename, int pageNumber)
        {
            object foxitMutex;
            Monitor.Enter(foxitMutex = FoxitMutex);
            try
            {
                if (foxitLib == null)
                {
                    throw FoxitLibManager.theInstance.loadException;
                }

                this.filename = filename;
                foxitLib.UnlockDLL("SDKRDYX1645", "3A6DE5356500929A15F3E6517416F91635E336C0");
                if ((docHandle = foxitLib.LoadDocument(filename, null)) == 0)
                {
                    throw new Exception("Can't open " + filename);
                }

                numPages = foxitLib.GetPageCount(docHandle);
                if ((pageHandle = foxitLib.LoadPage(docHandle, pageNumber)) == 0)
                {
                    foxitLib.CloseDocument(docHandle);
                    throw new Exception("Can't open first page of " + filename);
                }

                FIBR.Announce("FoxitViewer.FoxitViewer",
                    new object[] {MakeObjectID.Maker.make(this), filename, pageNumber});
                foxitResourceCounter.crement(1);
            }
            finally
            {
                Monitor.Exit(foxitMutex);
            }
        }

        public RectangleF GetPageSize()
        {
            object foxitMutex;
            Monitor.Enter(foxitMutex = FoxitMutex);
            RectangleF result;
            try
            {
                RectangleF rectangleF = default(RectangleF);
                double num = foxitLib.GetPageWidth(pageHandle);
                rectangleF.Width = (float)num;
                num = foxitLib.GetPageHeight(pageHandle);
                rectangleF.Height = (float)num;
                FIBR.Announce("FoxitViewer.GetPageSize", new object[] {MakeObjectID.Maker.make(this)});
                result = rectangleF;
            }
            finally
            {
                Monitor.Exit(foxitMutex);
            }

            return result;
        }

        public GDIBigLockedImage Render(Size outSize, Point topleft, Size pagesize, bool transparentBackground)
        {
            int alpha = transparentBackground ? 0 : 255;
            object foxitMutex;
            Monitor.Enter(foxitMutex = FoxitMutex);
            GDIBigLockedImage result;
            try
            {
                int bitmap = foxitLib.Bitmap_Create(outSize.Width, outSize.Height, 1);
                foxitLib.Bitmap_FillRect(bitmap, 0, 0, outSize.Width, outSize.Height, 255, 255, 255, alpha);
                foxitLib.RenderPageBitmap(bitmap,
                    pageHandle,
                    topleft.X,
                    topleft.Y,
                    pagesize.Width,
                    pagesize.Height,
                    0,
                    0);
                IntPtr scan = foxitLib.Bitmap_GetBuffer(bitmap);
                Bitmap bitmap2 = new Bitmap(outSize.Width,
                    outSize.Height,
                    outSize.Width * 4,
                    PixelFormat.Format32bppArgb,
                    scan);
                foxitLib.Bitmap_Destroy(bitmap);
                GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(bitmap2);
                result = gDIBigLockedImage;
            }
            finally
            {
                Monitor.Exit(foxitMutex);
            }

            return result;
        }

        public RenderReply RenderBytes(Size outSize, Point topleft, Size pagesize, bool transparentBackground)
        {
            int alpha = transparentBackground ? 0 : 255;
            object foxitMutex;
            Monitor.Enter(foxitMutex = FoxitMutex);
            RenderReply result;
            try
            {
                int bitmap = foxitLib.Bitmap_Create(outSize.Width, outSize.Height, 1);
                foxitLib.Bitmap_FillRect(bitmap, 0, 0, outSize.Width, outSize.Height, 255, 255, 255, alpha);
                foxitLib.RenderPageBitmap(bitmap,
                    pageHandle,
                    topleft.X,
                    topleft.Y,
                    pagesize.Width,
                    pagesize.Height,
                    0,
                    0);
                IntPtr source = foxitLib.Bitmap_GetBuffer(bitmap);
                int num = outSize.Width * 4;
                byte[] array = new byte[outSize.Height * num];
                Marshal.Copy(source, array, 0, array.Length);
                foxitLib.Bitmap_Destroy(bitmap);
                result = new RenderReply(array, outSize.Width * 4);
            }
            finally
            {
                Monitor.Exit(foxitMutex);
            }

            return result;
        }

        public void Dispose()
        {
            object foxitMutex;
            Monitor.Enter(foxitMutex = FoxitMutex);
            try
            {
                foxitLib.ClosePage(pageHandle);
                foxitLib.CloseDocument(docHandle);
                FIBR.Announce("FoxitViewer.Dispose", new object[] {MakeObjectID.Maker.make(this)});
                foxitResourceCounter.crement(-1);
            }
            finally
            {
                Monitor.Exit(foxitMutex);
            }
        }
    }
}
