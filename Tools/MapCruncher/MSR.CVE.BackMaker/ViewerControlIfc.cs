using System.Collections.Generic;
using System.Drawing;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public interface ViewerControlIfc : TransparencyIfc
    {
        Size Size
        {
            get;
        }

        void ClearLayers();
        void SetBaseLayer(IDisplayableSource layer);
        void AddLayer(IDisplayableSource layer);
        void setPinList(List<PositionAssociationView> newList);
        void SetLatentRegionHolder(LatentRegionHolder latentRegionHolder);
        void SetSnapViewStore(SnapViewStoreIfc snapViewStore);
        MapRectangle GetBounds();
        CoordinateSystemIfc GetCoordinateSystem();
    }
}
