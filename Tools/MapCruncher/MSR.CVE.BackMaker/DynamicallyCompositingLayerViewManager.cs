using System.Drawing;
using Jama;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class DynamicallyCompositingLayerViewManager : IViewManager
    {
        private Layer layer;
        private MapTileSourceFactory mapTileSourceFactory;
        private ViewControlIfc viewControl;

        public DynamicallyCompositingLayerViewManager(Layer layer, MapTileSourceFactory mapTileSourceFactory,
            ViewControlIfc viewControl)
        {
            this.layer = layer;
            this.mapTileSourceFactory = mapTileSourceFactory;
            this.viewControl = viewControl;
        }

        public void Activate()
        {
            ViewerControlIfc sMViewerControl = viewControl.GetSMViewerControl();
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            foreach (SourceMap current in layer.GetBackToFront())
            {
                IDisplayableSource displayableSource = mapTileSourceFactory.CreateDisplayableWarpedSource(current);
                if (displayableSource != null)
                {
                    sMViewerControl.AddLayer(displayableSource);
                }
            }

            uIPositionManager.SetPositionMemory(layer);
            LayerView layerView = (LayerView)layer.lastView;
            viewControl.GetUIPositionManager().switchSlaved();
            if (layerView != null)
            {
                uIPositionManager.GetVEPos().setPosition(layerView.GetReferenceMapView());
                uIPositionManager.GetVEPos().setStyle(layerView.GetReferenceMapView().style);
                return;
            }

            MapRectangle mapRectangle = null;
            try
            {
                mapRectangle = layer.GetUserBoundingBox(mapTileSourceFactory);
            }
            catch (CorrespondencesAreSingularException)
            {
            }
            catch (InsufficientCorrespondencesException)
            {
            }

            LatLonZoom position;
            if (mapRectangle != null)
            {
                Size size = new Size(600, 600);
                position = viewControl.GetVEViewerControl().GetCoordinateSystem()
                    .GetBestViewContaining(mapRectangle, size);
            }
            else
            {
                position = viewControl.GetVEViewerControl().GetCoordinateSystem().GetDefaultView();
            }

            uIPositionManager.GetVEPos().setPosition(position);
        }

        public void Dispose()
        {
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            uIPositionManager.switchFree();
            uIPositionManager.SetPositionMemory(null);
            uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
            viewControl.GetSMViewerControl().ClearLayers();
            viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
            viewControl.GetSourceMapInfoPanel().Configure(null);
            viewControl.GetSourceMapInfoPanel().Enabled = false;
            viewControl.GetTransparencyPanel().Configure(null, null);
            viewControl.GetTransparencyPanel().Enabled = false;
            viewControl.setDisplayedRegistration(null);
            viewControl.GetCachePackage().ClearSchedulers();
        }

        public object GetViewedObject()
        {
            return layer;
        }
    }
}
