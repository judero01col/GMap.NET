using System;
using System.Drawing;
using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class SingleSourceUnit : RenderWorkUnit
    {
        public delegate bool NeedThisTileDelegate();

        private OneLayerBoundApplier applier;
        private TileAddress address;
        private int stage;
        private NeedThisTileDelegate needThisTile;
        private static int debug_lastZoom = -1;

        internal SingleSourceUnit(OneLayerBoundApplier applier, TileAddress address, int stage,
            NeedThisTileDelegate needThisTile)
        {
            this.applier = applier;
            this.address = address;
            this.stage = stage;
            this.needThisTile = needThisTile;
        }

        public override bool DoWork(ITileWorkFeedback feedback)
        {
            if (!needThisTile())
            {
                return false;
            }

            D.Sayf(10,
                "SingleSourcing {0} {1}",
                new object[] {applier.source.GetSourceMapDisplayName(), address});
            Present present = FetchClippedImage();
            if (present is ImageRef)
            {
                ImageRef image = (ImageRef)present;
                feedback.PostImageResult(image,
                    applier.layer,
                    applier.source.GetSourceMapDisplayName(),
                    address);
            }

            present.Dispose();
            return true;
        }

        public void CompositeImageInto(GDIBigLockedImage baseImage)
        {
            Present present = FetchClippedImage();
            if (present is ImageRef)
            {
                ImageRef imageRef = (ImageRef)present;
                baseImage.DrawImageOntoThis(imageRef.image,
                    new Rectangle(0, 0, baseImage.Width, baseImage.Height),
                    new Rectangle(0, 0, imageRef.image.Width, imageRef.image.Height));
            }
            else
            {
                if (present is PresentFailureCode)
                {
                    throw new NonredundantRenderComplaint(string.Format("{0}: {1}",
                        applier.DescribeSourceForComplaint(),
                        ((PresentFailureCode)present).exception.Message));
                }
            }

            present.Dispose();
        }

        private Present FetchClippedImage()
        {
            if (debug_lastZoom != address.ZoomLevel)
            {
                debug_lastZoom = address.ZoomLevel;
                D.Sayf(0, "{0} start zoom level {1}", new object[] {Clocker.theClock.stamp(), address.ZoomLevel});
            }

            IFuture future = applier.clippedImageFuture.Curry(new ParamDict(new object[]
            {
                TermName.TileAddress, address
            }));
            return future.Realize("ImageRef.FetchClippedImage");
        }

        public override RenderWorkUnitComparinator GetWorkUnitComparinator()
        {
            return new RenderWorkUnitComparinator(new IComparable[]
            {
                address.ZoomLevel, applier.layer.displayName, stage, 0, applier.source,
                address
            });
        }

        public override string ToString()
        {
            return string.Format("SSU layer {0} sm {1} address {2} stage {3}",
                new object[]
                {
                    applier.layer.displayName, applier.source.GetSourceMapDisplayName(), address,
                    stage
                });
        }
    }
}
