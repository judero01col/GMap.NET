using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class WPFOpenDocument : VerbPresent, Verb, Present, IDisposable, SizedObject
    {
        private string sourceFilename;
        private int frameNumber;
        private BitmapSource primarySource;
        private RectangleF actualBoundingBox;
        private RectangleF boundingBox;
        private int hackRectangleAdjust;

        public WPFOpenDocument(string sourceFilename, int frameNumber)
        {
            this.sourceFilename = sourceFilename;
            this.frameNumber = frameNumber;
            if (this.frameNumber != 0)
            {
                throw new Exception("Unable to open page numbers other than 0 for this type of image file.");
            }

            D.Assert(this.frameNumber == 0);
            Stream streamSource = new FileStream(sourceFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = streamSource;
            bitmapImage.CacheOption = BitmapCacheOption.None;
            bitmapImage.EndInit();
            primarySource = new FormatConvertedBitmap(bitmapImage, PixelFormats.Pbgra32, null, 0.0);
            primarySource.Freeze();
            actualBoundingBox = new RectangleF(0f,
                0f,
                primarySource.PixelWidth,
                primarySource.PixelHeight);
            boundingBox = SourceMapRendererTools.ToSquare(actualBoundingBox);
            if (sourceFilename.EndsWith("emf") || sourceFilename.EndsWith("wmf"))
            {
                hackRectangleAdjust = 1;
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("WPFOpenDocument");
            hash.Accumulate(sourceFilename);
            hash.Accumulate(frameNumber);
        }

        public Present Duplicate(string refCredit)
        {
            return this;
        }

        public void Dispose()
        {
            Monitor.Enter(this);
            try
            {
                primarySource = null;
            }
            finally
            {
                Monitor.Exit(this);
            }

            D.Sayf(0, "WPFOpenDocument Dispose({0})", new object[] {sourceFilename});
        }

        internal Present Render(MapRectangle mapRect, System.Drawing.Size size, bool useDocumentTransparency,
            bool exactColors)
        {
            Monitor.Enter(this);
            Present result;
            try
            {
                RectangleD rectangleD = new RectangleD(mapRect.lon0 * boundingBox.Width - 0.5,
                    -mapRect.lat1 * boundingBox.Height + actualBoundingBox.Height - 0.5,
                    (mapRect.lon1 - mapRect.lon0) * boundingBox.Width + hackRectangleAdjust,
                    (mapRect.lat1 - mapRect.lat0) * boundingBox.Height + hackRectangleAdjust);
                RectangleD rectangleD2 = rectangleD.Grow(2.0);
                RectangleD r = new RectangleD(actualBoundingBox.X,
                    actualBoundingBox.Y,
                    actualBoundingBox.Width,
                    actualBoundingBox.Height);
                RectangleD dest = new RectangleD(0.0, 0.0, size.Width, size.Height);
                ScaleAndTranslate scaleAndTranslate = new ScaleAndTranslate(rectangleD, dest);
                RectangleD rectangleD3 = rectangleD2.Intersect(r).Round();
                RectangleD rectangleD4 = scaleAndTranslate.Apply(rectangleD.Intersect(r));
                RectangleD rectangleD5 = scaleAndTranslate.Apply(rectangleD3);
                ScaleAndTranslate scaleAndTranslate2 = new ScaleAndTranslate(-rectangleD5.X, -rectangleD5.Y);
                RectangleD rectangleD6 = scaleAndTranslate2.Apply(rectangleD4);
                GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(size, "WPFVerb");
                GDIBigLockedImage image = gDIBigLockedImage;
                if (!rectangleD3.IntIsEmpty() && !rectangleD6.IntIsEmpty())
                {
                    try
                    {
                        GDIBigLockedImage obj;
                        Monitor.Enter(obj = gDIBigLockedImage);
                        try
                        {
                            Bitmap bitmap =
                                (Bitmap)gDIBigLockedImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                            BitmapSource source = new CroppedBitmap(primarySource, rectangleD3.ToInt32Rect());
                            BitmapSource source2 = new TransformedBitmap(source, scaleAndTranslate.ToScaleTransform());
                            BitmapSource bitmapSource = new CroppedBitmap(source2, rectangleD6.ToInt32Rect());
                            Int32Rect int32Rect = rectangleD4.ToInt32Rect();
                            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, size.Width, size.Height),
                                ImageLockMode.WriteOnly,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            try
                            {
                                IntPtr buffer = new IntPtr(bitmapData.Scan0.ToInt32() +
                                                           int32Rect.Y * bitmapData.Stride + int32Rect.X * 4);
                                bitmapSource.CopyPixels(
                                    new Int32Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight),
                                    buffer,
                                    bitmapData.Stride * bitmapData.Height,
                                    bitmapData.Stride);
                            }
                            finally
                            {
                                bitmap.UnlockBits(bitmapData);
                            }
                        }
                        finally
                        {
                            Monitor.Exit(obj);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (BigDebugKnob.theKnob.debugFeaturesEnabled)
                        {
                            LabelThisImage(gDIBigLockedImage, ex.ToString());
                        }
                    }

                    if (useDocumentTransparency)
                    {
                        image = gDIBigLockedImage;
                    }
                    else
                    {
                        GDIBigLockedImage gDIBigLockedImage2 = new GDIBigLockedImage(size, "WPFVerb-untransparent");
                        GDIBigLockedImage obj2;
                        Monitor.Enter(obj2 = gDIBigLockedImage2);
                        try
                        {
                            Graphics graphics =
                                gDIBigLockedImage2.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics();
                            graphics.FillRectangle(new SolidBrush(System.Drawing.Color.White),
                                0,
                                0,
                                size.Width,
                                size.Height);
                            graphics.DrawImage(gDIBigLockedImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage(),
                                0,
                                0);
                            graphics.Dispose();
                            image = gDIBigLockedImage2;
                            gDIBigLockedImage.Dispose();
                        }
                        finally
                        {
                            Monitor.Exit(obj2);
                        }
                    }
                }

                result = new ImageRef(new ImageRefCounted(image));
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        private void LabelThisImage(GDIBigLockedImage newImage, string message)
        {
            Graphics graphics = newImage.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheGraphics();
            graphics.DrawString(StringUtils.breakLines(message),
                new Font("Arial", 8f),
                new SolidBrush(System.Drawing.Color.DarkBlue),
                3f,
                15f);
            graphics.Dispose();
        }

        internal Present FetchBounds()
        {
            return new BoundsPresent(new RenderRegion(new MapRectangle(0.0,
                    0.0,
                    actualBoundingBox.Height / boundingBox.Height,
                    actualBoundingBox.Width / boundingBox.Width),
                new DirtyEvent()));
        }

        public long GetSize()
        {
            return 2097152L;
        }

        internal Present ImageDetail(System.Drawing.Size assumedDisplaySize)
        {
            Monitor.Enter(this);
            Present result;
            try
            {
                double num = Math.Max(primarySource.PixelWidth / (double)assumedDisplaySize.Width,
                    primarySource.PixelHeight / (double)assumedDisplaySize.Height);
                num = Math.Max(num, 1.0);
                int num2 = 1 + (int)Math.Ceiling(Math.Log(num) / Math.Log(2.0));
                D.Assert(num2 >= 0);
                result = new IntParameter(num2);
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        public Present Evaluate(Present[] paramList)
        {
            if (!(paramList[0] is IntParameter))
            {
                return PresentFailureCode.FailedCast(paramList[0], "FoxitOpenDocument.Evaluate");
            }

            switch (((IntParameter)paramList[0]).value)
            {
                case 0:
                {
                    D.Assert(paramList.Length == 5);
                    MapRectangleParameter mapRectangleParameter = (MapRectangleParameter)paramList[1];
                    SizeParameter sizeParameter = (SizeParameter)paramList[2];
                    BoolParameter boolParameter = (BoolParameter)paramList[3];
                    BoolParameter boolParameter2 = (BoolParameter)paramList[4];
                    return Render(mapRectangleParameter.value,
                        sizeParameter.value,
                        boolParameter.value,
                        boolParameter2.value);
                }
                case 1:
                    D.Assert(paramList.Length == 1);
                    return FetchBounds();
                case 2:
                {
                    D.Assert(paramList.Length == 2);
                    SizeParameter sizeParameter2 = (SizeParameter)paramList[1];
                    return ImageDetail(sizeParameter2.value);
                }
                case 3:
                    D.Assert(paramList.Length == 1);
                    return new StringParameter(null);
                default:
                    return new PresentFailureCode("Invalid AccessVerb");
            }
        }
    }
}
