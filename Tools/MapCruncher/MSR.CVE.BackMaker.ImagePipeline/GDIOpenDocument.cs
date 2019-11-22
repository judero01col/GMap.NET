using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class GDIOpenDocument : VerbPresent, Verb, Present, IDisposable, SizedObject
    {
        private string sourceFilename;
        private GDIBigLockedImage loadedImage;
        private RectangleF actualBoundingBox;
        private RectangleF boundingBox;
        private int hackRectangleAdjust;

        public GDIOpenDocument(string sourceFilename)
        {
            this.sourceFilename = sourceFilename;
            loadedImage = GDIBigLockedImage.FromFile(sourceFilename);
            D.Sayf(0, "GDIOpenDocument Image.FromFile({0})", new object[] {sourceFilename});
            actualBoundingBox = new RectangleF(default(PointF), loadedImage.Size);
            boundingBox = SourceMapRendererTools.ToSquare(actualBoundingBox);
            if (sourceFilename.EndsWith("emf") || sourceFilename.EndsWith("wmf"))
            {
                hackRectangleAdjust = 1;
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("GDIOpenDocument");
            hash.Accumulate(sourceFilename);
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
                loadedImage.Dispose();
                loadedImage = null;
            }
            finally
            {
                Monitor.Exit(this);
            }

            D.Sayf(0, "GDIOpenDocument Dispose({0})", new object[] {sourceFilename});
        }

        internal Present Render(MapRectangle mapRect, Size size, bool useDocumentTransparency, bool exactColors)
        {
            Monitor.Enter(this);
            Present result;
            try
            {
                RectangleD rectangleD = new RectangleD(mapRect.lon0 * boundingBox.Width - 0.5,
                    -mapRect.lat1 * boundingBox.Height + actualBoundingBox.Height - 0.5,
                    (mapRect.lon1 - mapRect.lon0) * boundingBox.Width + hackRectangleAdjust,
                    (mapRect.lat1 - mapRect.lat0) * boundingBox.Height + hackRectangleAdjust);
                RectangleD rectangleD2 = new RectangleD(0.0, 0.0, size.Width, size.Height);
                Reclip(actualBoundingBox, ref rectangleD, ref rectangleD2);
                D.Say(10, string.Format("Rendering {0} from {1}", mapRect, rectangleD));
                GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(size, "GDIVerb");
                if (exactColors)
                {
                    gDIBigLockedImage.SetInterpolationMode(InterpolationMode.NearestNeighbor);
                }
                else
                {
                    gDIBigLockedImage.SetInterpolationMode(InterpolationMode.HighQualityBicubic);
                }

                gDIBigLockedImage.DrawImageOntoThis(loadedImage,
                    rectangleD2.ToRectangleF(),
                    rectangleD.ToRectangleF());
                result = new ImageRef(new ImageRefCounted(gDIBigLockedImage));
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        private void Reclip(RectangleF sourceBounds, ref RectangleD sourceRect, ref RectangleD outRect)
        {
            double num = sourceRect.X;
            double num2 = sourceRect.Right;
            double num3 = sourceRect.Y;
            double num4 = sourceRect.Bottom;
            double num5 = outRect.X;
            double num6 = outRect.Right;
            double num7 = outRect.Y;
            double num8 = outRect.Bottom;
            if (sourceRect.Right < sourceBounds.Left || sourceRect.Right > sourceBounds.Right)
            {
                if (sourceRect.Right < sourceBounds.Left)
                {
                    num2 = sourceBounds.Left;
                }
                else
                {
                    if (sourceRect.Right > sourceBounds.Right)
                    {
                        num2 = sourceBounds.Right;
                    }
                }

                num6 = outRect.Left + outRect.Width * (num2 - sourceRect.Left) / sourceRect.Width;
            }

            if (sourceRect.Left < sourceBounds.Left || sourceRect.Left > sourceBounds.Right)
            {
                if (sourceRect.Left < sourceBounds.Left)
                {
                    num = sourceBounds.Left;
                }
                else
                {
                    if (sourceRect.Left > sourceBounds.Right)
                    {
                        num = sourceBounds.Right;
                    }
                }

                num5 = outRect.Left + outRect.Width * (num - sourceRect.Left) / sourceRect.Width;
            }

            if (sourceRect.Top < sourceBounds.Top || sourceRect.Top > sourceBounds.Bottom)
            {
                if (sourceRect.Top < sourceBounds.Top)
                {
                    num3 = sourceBounds.Top;
                }
                else
                {
                    if (sourceRect.Top > sourceBounds.Bottom)
                    {
                        num3 = sourceBounds.Bottom;
                    }
                }

                num7 = outRect.Top + outRect.Height * (num3 - sourceRect.Top) / sourceRect.Height;
            }

            if (sourceRect.Bottom < sourceBounds.Top || sourceRect.Bottom > sourceBounds.Bottom)
            {
                if (sourceRect.Bottom < sourceBounds.Top)
                {
                    num4 = sourceBounds.Top;
                }
                else
                {
                    if (sourceRect.Bottom > sourceBounds.Bottom)
                    {
                        num4 = sourceBounds.Bottom;
                    }
                }

                num8 = outRect.Top + outRect.Height * (num4 - sourceRect.Top) / sourceRect.Height;
            }

            RectangleD rectangleD = new RectangleD(num, num3, num2 - num, num4 - num3);
            RectangleD rectangleD2 = new RectangleD(num5, num7, num6 - num5, num8 - num7);
            sourceRect = rectangleD;
            outRect = rectangleD2;
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
            return (long)(actualBoundingBox.Width * actualBoundingBox.Height * 4f);
        }

        internal Present ImageDetail(Size assumedDisplaySize)
        {
            double num = Math.Max(loadedImage.Width / (double)assumedDisplaySize.Width,
                loadedImage.Height / (double)assumedDisplaySize.Height);
            num = Math.Max(num, 1.0);
            int num2 = 1 + (int)Math.Ceiling(Math.Log(num) / Math.Log(2.0));
            D.Assert(num2 >= 0);
            return new IntParameter(num2);
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
