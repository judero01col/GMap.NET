using System;
using System.Drawing;
using System.Threading;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class FoxitOpenDocument : VerbPresent, Verb, Present, IDisposable, SizedObject
    {
        private string sourceFilename;
        private int pageNumber;
        private IFoxitViewer foxitViewer;
        private RectangleF actualBoundingBox;
        private RectangleF boundingBox;

        public FoxitOpenDocument(string sourceFilename, int pageNumber)
        {
            this.sourceFilename = sourceFilename;
            this.pageNumber = pageNumber;
            foxitViewer = new RemoteFoxitStub(sourceFilename, pageNumber);
            actualBoundingBox = foxitViewer.GetPageSize();
            boundingBox = SourceMapRendererTools.ToSquare(actualBoundingBox);
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("FoxitOpenDocument");
            hash.Accumulate(sourceFilename);
            hash.Accumulate(pageNumber);
        }

        public Present Duplicate(string refCredit)
        {
            return this;
        }

        public void Dispose()
        {
            foxitViewer.Dispose();
        }

        internal Present Render(MapRectangle mapRect, Size size, bool useDocumentTransparency, bool exactColors)
        {
            RectangleF rectangleF = new RectangleF((float)mapRect.lon0,
                (float)mapRect.lat0,
                (float)(mapRect.lon1 - mapRect.lon0),
                (float)(mapRect.lat1 - mapRect.lat0));
            double num = actualBoundingBox.Width / (double)boundingBox.Width;
            double num2 = actualBoundingBox.Height / (double)boundingBox.Height;
            Size pagesize = new Size((int)Math.Round(size.Width / (double)rectangleF.Width * num),
                (int)Math.Round(size.Height / (double)rectangleF.Height * num2));
            Point topleft = new Point((int)Math.Round(-(double)pagesize.Width / num * mapRect.lon0),
                (int)Math.Round(-(double)pagesize.Height / num2 * (num2 - mapRect.lat1)));
            if (pagesize.Width > 262144 || pagesize.Width <= 0 || pagesize.Height > 262144 || pagesize.Height <= 0)
            {
                return new PresentFailureCode("Zoomed beyond FoxIt limits");
            }

            IFoxitViewer obj;
            Monitor.Enter(obj = foxitViewer);
            GDIBigLockedImage image;
            try
            {
                image = foxitViewer.Render(size, topleft, pagesize, useDocumentTransparency);
            }
            finally
            {
                Monitor.Exit(obj);
            }

            return new ImageRef(new ImageRefCounted(image));
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
            if (foxitViewer is RemoteFoxitStub)
            {
                return ((RemoteFoxitStub)foxitViewer).GetSize();
            }

            return 83886080L;
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
                    D.Assert(paramList.Length == 2);
                    return new IntParameter(5);
                case 3:
                    D.Assert(paramList.Length == 1);
                    if (BuildConfig.theConfig.suppressFoxitMessages)
                    {
                        return new StringParameter(null);
                    }

                    return new StringParameter("MapCruncher PDF rendering powered by Foxit Software Company");
                default:
                    return new PresentFailureCode("Invalid AccessVerb");
            }
        }
    }
}
