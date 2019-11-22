using System;
using System.Drawing;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    internal class WarpImageVerb : Verb
    {
        private class UnboundParameter : Parameter, IRobustlyHashable, Present, IDisposable
        {
            public void AccumulateRobustHash(IRobustHash hash)
            {
                hash.Accumulate("UnboundParameter");
            }

            public Present Duplicate(string refCredit)
            {
                return this;
            }

            public void Dispose()
            {
            }
        }

        private IImageTransformer imageTransformer;
        private IFuture warpedBoundsFuture;
        private IFuturePrototype sourceMapSupplier;

        public WarpImageVerb(IImageTransformer imageTransformer, IFuture warpedBoundsFuture,
            IFuturePrototype sourceMapSupplier)
        {
            this.imageTransformer = imageTransformer;
            this.warpedBoundsFuture = warpedBoundsFuture;
            this.sourceMapSupplier = sourceMapSupplier;
        }

        public Present Evaluate(Present[] paramList)
        {
            D.Assert(paramList.Length == 2);
            MapRectangle value = ((MapRectangleParameter)paramList[0]).value;
            Size value2 = ((SizeParameter)paramList[1]).value;
            MapRectangle mapRectangle = value.Transform(imageTransformer.getDestLatLonToSourceTransformer())
                .GrowFraction(0.05);
            Present present = warpedBoundsFuture.Realize("WarpImageVerb.Evaluate-bounds");
            if (present is BoundsPresent)
            {
                MapRectangle boundingBox = ((BoundsPresent)present).GetRenderRegion().GetBoundingBox();
                if (!boundingBox.intersects(value))
                {
                    return new BeyondImageBounds();
                }
            }

            Present present2 = sourceMapSupplier.Curry(new ParamDict(new object[]
            {
                TermName.ImageBounds, new MapRectangleParameter(mapRectangle)
            })).Realize("WarpImageVerb.Evaluate");
            if (present2 is PresentFailureCode)
            {
                return present2;
            }

            ImageRef imageRef = (ImageRef)present2;
            GDIBigLockedImage gDIBigLockedImage = new GDIBigLockedImage(value2, "WarpImageVerb");
            imageTransformer.doTransformImage(imageRef.image, mapRectangle, gDIBigLockedImage, value);
            imageRef.Dispose();
            return new ImageRef(new ImageRefCounted(gDIBigLockedImage));
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("WarpImageVerb(");
            imageTransformer.AccumulateRobustHash(hash);
            sourceMapSupplier.Curry(new ParamDict(new object[] {TermName.ImageBounds, new UnboundParameter()}))
                .AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }
    }
}
