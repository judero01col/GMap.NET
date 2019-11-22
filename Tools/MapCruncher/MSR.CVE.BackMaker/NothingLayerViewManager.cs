using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class NothingLayerViewManager : IViewManager
    {
        private ViewControlIfc viewControl;

        public NothingLayerViewManager(ViewControlIfc viewControl)
        {
            this.viewControl = viewControl;
        }

        public void Activate()
        {
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            uIPositionManager.switchFree();
            uIPositionManager.GetSMPos().setPosition(ContinuousCoordinateSystem.theInstance.GetDefaultView());
            uIPositionManager.GetVEPos().setPosition(MercatorCoordinateSystem.theInstance.GetDefaultView());
        }

        public void Dispose()
        {
        }

        public object GetViewedObject()
        {
            return null;
        }
    }
}
