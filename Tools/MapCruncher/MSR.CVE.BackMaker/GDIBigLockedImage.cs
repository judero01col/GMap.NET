using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class GDIBigLockedImage : IDisposable
    {
        public enum Transparentness
        {
            EntirelyTransparent,
            SomeOfEach,
            EntirelyOpaque
        }

        private Image gdiImage;
        private bool sizeKnown;
        private Size size;
        private Graphics gdiGraphics;
        private string sourceLabel;

        private static ResourceCounter allImageCounter =
            DiagnosticUI.theDiagnostics.fetchResourceCounter("GDIBigLockedImage", 10);

        private static Dictionary<string, ResourceCounter> fineGrainedImageCounter =
            new Dictionary<string, ResourceCounter>();

        public int Height
        {
            get
            {
                return Size.Height;
            }
        }

        public int Width
        {
            get
            {
                return Size.Width;
            }
        }

        public ImageFormat RawFormat
        {
            get
            {
                Monitor.Enter(this);
                ImageFormat rawFormat;
                try
                {
                    rawFormat = gdiImage.RawFormat;
                }
                finally
                {
                    Monitor.Exit(this);
                }

                return rawFormat;
            }
        }

        public Size Size
        {
            get
            {
                if (!sizeKnown)
                {
                    Monitor.Enter(this);
                    try
                    {
                        size = gdiImage.Size;
                        sizeKnown = true;
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }

                return size;
            }
        }

        public GDIBigLockedImage(Size size, string sourceLabel)
        {
            gdiImage = new Bitmap(size.Width, size.Height);
            FIBR.Announce("GDIBigLockedImage.GDIBigLockedImage", new object[] {MakeObjectID.Maker.make(this), size});
            this.sourceLabel = sourceLabel;
            CrementCounter(1);
        }

        private void CrementCounter(int crement)
        {
            allImageCounter.crement(crement);
            D.Assert(sourceLabel != null);
            if (!fineGrainedImageCounter.ContainsKey(sourceLabel))
            {
                fineGrainedImageCounter[sourceLabel] =
                    DiagnosticUI.theDiagnostics.fetchResourceCounter("GDIBLI-" + sourceLabel, 10);
            }

            fineGrainedImageCounter[sourceLabel].crement(crement);
        }

        public GDIBigLockedImage(Bitmap bitmap)
        {
            gdiImage = bitmap;
            sourceLabel = "bitmapCtor";
            CrementCounter(1);
        }

        private GDIBigLockedImage(string sourceLabel)
        {
            this.sourceLabel = sourceLabel;
            CrementCounter(1);
        }

        public static GDIBigLockedImage FromStream(Stream instream)
        {
            GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage("FromStream");
            gDIBigLockedImage.gdiImage = Image.FromStream(instream);
            FIBR.Announce("GDIBigLockedImage.FromStream", new object[] {MakeObjectID.Maker.make(gDIBigLockedImage)});
            return gDIBigLockedImage;
        }

        public static GDIBigLockedImage FromFile(string filename)
        {
            GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage("FromFile");
            gDIBigLockedImage.gdiImage = Image.FromFile(filename);
            FIBR.Announce("GDIBigLockedImage.FromFile",
                new object[] {MakeObjectID.Maker.make(gDIBigLockedImage), filename});
            return gDIBigLockedImage;
        }

        internal void CopyPixels()
        {
            Monitor.Enter(this);
            try
            {
                DisposeGraphics();
                Image image = new Bitmap(Size.Width, Size.Height);
                Graphics graphics = Graphics.FromImage(image);
                graphics.DrawImage(gdiImage, 0, 0, Size.Width, Size.Height);
                graphics.Dispose();
                gdiImage.Dispose();
                gdiImage = image;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void Dispose()
        {
            Monitor.Enter(this);
            try
            {
                if (gdiGraphics != null)
                {
                    gdiGraphics.Dispose();
                    FIBR.Announce("GDIBigLockedImage.Dispose(graphics)", new object[] {MakeObjectID.Maker.make(this)});
                }

                FIBR.Announce("GDIBigLockedImage.Dispose(image)", new object[] {MakeObjectID.Maker.make(this)});
                gdiImage.Dispose();
                CrementCounter(-1);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void DrawImageOntoThis(GDIBigLockedImage gDISImage, RectangleF destRect, RectangleF srcRect)
        {
            Monitor.Enter(this);
            try
            {
                Graphics gDIGraphics = GetGDIGraphics();
                Monitor.Enter(gDISImage);
                try
                {
                    gDIGraphics.DrawImage(gDISImage.gdiImage, destRect, srcRect, GraphicsUnit.Pixel);
                }
                finally
                {
                    Monitor.Exit(gDISImage);
                }

                gdiGraphics.Dispose();
                gdiGraphics = null;
                FIBR.Announce("GDIBigLockedImage.DrawImageOntoThis",
                    new object[]
                    {
                        MakeObjectID.Maker.make(this), MakeObjectID.Maker.make(gDISImage), destRect, srcRect
                    });
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private Graphics GetGDIGraphics()
        {
            if (gdiGraphics == null)
            {
                Monitor.Enter(this);
                try
                {
                    gdiGraphics = Graphics.FromImage(gdiImage);
                    FIBR.Announce("GDIBigLockedImage.GetGDIGraphics", new object[] {MakeObjectID.Maker.make(this)});
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }

            return gdiGraphics;
        }

        public void Save(Stream outputStream, ImageFormat imageFormat)
        {
            Monitor.Enter(this);
            try
            {
                gdiImage.Save(outputStream, imageFormat);
                FIBR.Announce("GDIBigLockedImage.Save", new object[] {MakeObjectID.Maker.make(this), "stream"});
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void Save(string outputFilename)
        {
            Monitor.Enter(this);
            try
            {
                gdiImage.Save(outputFilename);
                FIBR.Announce("GDIBigLockedImage.Save", new object[] {MakeObjectID.Maker.make(this), outputFilename});
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public Graphics IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics()
        {
            return GetGDIGraphics();
        }

        public Image IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage()
        {
            return gdiImage;
        }

        public void SetInterpolationMode(InterpolationMode interpolationMode)
        {
            Monitor.Enter(this);
            try
            {
                GetGDIGraphics().InterpolationMode = interpolationMode;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetClip(Region clipRegion)
        {
            Monitor.Enter(this);
            try
            {
                GetGDIGraphics().Clip = clipRegion;
                FIBR.Announce("GDIBigLockedImage.SetClip", new object[] {MakeObjectID.Maker.make(this)});
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal void DisposeGraphics()
        {
            Monitor.Enter(this);
            try
            {
                if (gdiGraphics != null)
                {
                    gdiGraphics.Dispose();
                    gdiGraphics = null;
                    FIBR.Announce("GDIBigLockedImage.DisposeGraphics", new object[] {MakeObjectID.Maker.make(this)});
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public unsafe Transparentness GetTransparentness()
        {
            Transparentness entirelyTransparent;
            lock (this)
            {
                Bitmap bitmap = (Bitmap)IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);
                bool flag = false;
                bool condition = false;
                try
                {
                    PixelStruct* structPtr = (PixelStruct*)bitmapdata.Scan0;
                    for (int i = 0; i < bitmapdata.Height; i++)
                    {
                        for (int j = 0; j < bitmapdata.Width; j++)
                        {
                            PixelStruct* structPtr2 = structPtr + i * bitmapdata.Stride / sizeof(PixelStruct) + j;
                            if (structPtr2->a != 0)
                            {
                                flag = true;
                            }
                            else
                            {
                                condition = true;
                            }

                            if (flag && condition)
                            {
                                return Transparentness.SomeOfEach;
                            }
                        }
                    }

                    if (flag)
                    {
                        D.Assert(!condition);
                        return Transparentness.EntirelyOpaque;
                    }

                    D.Assert(condition);
                    entirelyTransparent = Transparentness.EntirelyTransparent;
                }
                finally
                {
                    bitmap.UnlockBits(bitmapdata);
                }
            }

            return entirelyTransparent;
        }
    }
}
