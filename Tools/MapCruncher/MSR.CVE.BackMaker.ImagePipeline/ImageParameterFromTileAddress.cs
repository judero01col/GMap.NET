using System.Drawing;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class ImageParameterFromTileAddress : ImageParameterTypeIfc
    {
        private IFuturePrototype boundsParameter;
        private IFuturePrototype sizeParameter;

        public ImageParameterFromTileAddress(CoordinateSystemIfc coordSys) : this(coordSys, coordSys.GetTileSize())
        {
        }

        public ImageParameterFromTileAddress(CoordinateSystemIfc coordSys, Size outputSize)
        {
            boundsParameter = new ApplyPrototype(new TileAddressToImageRegion(coordSys),
                new IFuturePrototype[] {new UnevaluatedTerm(TermName.TileAddress)});
            sizeParameter = new ConstantFuture(new SizeParameter(outputSize));
        }

        public IFuturePrototype GetBoundsParameter()
        {
            return boundsParameter;
        }

        public IFuturePrototype GetSizeParameter()
        {
            return sizeParameter;
        }

        public override bool Equals(object obj)
        {
            if (obj is ImageParameterFromTileAddress)
            {
                ImageParameterFromTileAddress imageParameterFromTileAddress = (ImageParameterFromTileAddress)obj;
                return boundsParameter.Equals(imageParameterFromTileAddress.boundsParameter) &&
                       sizeParameter.Equals(imageParameterFromTileAddress.sizeParameter);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return boundsParameter.GetHashCode() * 131 + sizeParameter.GetHashCode();
        }
    }
}
