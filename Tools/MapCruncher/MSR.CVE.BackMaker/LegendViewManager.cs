using System;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class LegendViewManager : IViewManager
    {
        private Legend legend;
        private ViewControlIfc viewControl;
        private MapTileSourceFactory mapTileSourceFactory;

        private SourceMap sourceMap
        {
            get
            {
                return legend.sourceMap;
            }
        }

        public LegendViewManager(Legend legend, MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl)
        {
            this.legend = legend;
            this.mapTileSourceFactory = mapTileSourceFactory;
            this.viewControl = viewControl;
        }

        public void Activate()
        {
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            bool flag = false;
            if (legend.GetLastView() != null)
            {
                LegendView lastView = legend.GetLastView();
                if (lastView.showingPreview)
                {
                    throw new Exception("unimpl");
                }

                SetupNonpreviewView();
                uIPositionManager.GetSMPos().setPosition(lastView.GetSourceMapView());
                uIPositionManager.GetVEPos().setPosition(lastView.GetReferenceMapView());
                flag = true;
                viewControl.SetVEMapStyle(lastView.GetReferenceMapView().style);
            }

            if (!flag)
            {
                SetupNonpreviewView();
                uIPositionManager.GetSMPos().setPosition(new ContinuousCoordinateSystem().GetDefaultView());
                uIPositionManager.GetVEPos().setPosition(DefaultReferenceMapPosition());
            }

            uIPositionManager.SetPositionMemory(legend);
            viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.LegendOptions);
            viewControl.GetLegendPanel().Configure(legend,
                mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap));
            uIPositionManager.PositionUpdated();
        }

        public void Dispose()
        {
            viewControl.GetCachePackage().ClearSchedulers();
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            uIPositionManager.SetPositionMemory(null);
            uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
            uIPositionManager.switchFree();
            viewControl.GetSMViewerControl().ClearLayers();
            viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
            viewControl.GetLegendPanel().Configure(null, null);
            viewControl.setDisplayedRegistration(null);
            legend = null;
        }

        private void SetupNonpreviewView()
        {
            viewControl.GetSMViewerControl().SetBaseLayer(new LegendDisplayableSourceWrapper(
                mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap),
                legend.latentRegionHolder));
            viewControl.GetSMViewerControl().SetLatentRegionHolder(legend.latentRegionHolder);
            viewControl.GetUIPositionManager().switchFree();
        }

        internal LatLonZoom DefaultReferenceMapPosition()
        {
            return SourceMapViewManager.DefaultReferenceMapPosition(sourceMap,
                mapTileSourceFactory,
                viewControl,
                null);
        }

        public object GetViewedObject()
        {
            return legend;
        }
    }
}
