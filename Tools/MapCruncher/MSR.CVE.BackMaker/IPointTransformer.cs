using System.Xml;

namespace MSR.CVE.BackMaker
{
    public abstract class IPointTransformer
    {
        public virtual void doTransform(PointD p0, PointD p1, out bool isApproximate)
        {
            doTransform(p0, p1);
            isApproximate = false;
        }

        public PointD getTransformedPoint(PointD p0, out bool isApproximate)
        {
            PointD pointD = new PointD();
            doTransform(p0, pointD, out isApproximate);
            return pointD;
        }

        public virtual void doTransform(PointD p0, PointD p1)
        {
            bool flag;
            doTransform(p0, p1, out flag);
        }

        public PointD getTransformedPoint(PointD p0)
        {
            PointD pointD = new PointD();
            doTransform(p0, pointD);
            return pointD;
        }

        public abstract IPointTransformer getInversePointTransfomer();

        public virtual void writeToXml(XmlTextWriter writer)
        {
        }
    }
}
