using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Jama;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class SourceMapViewManager : IViewManager
    {
        private SourceMap sourceMap;
        private ViewControlIfc viewControl;
        private bool mapsLocked;
        private MapTileSourceFactory mapTileSourceFactory;
        private DefaultReferenceView drv;

        public SourceMapViewManager(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory,
            ViewControlIfc viewControl, DefaultReferenceView drv)
        {
            this.sourceMap = sourceMap;
            this.mapTileSourceFactory = mapTileSourceFactory;
            this.viewControl = viewControl;
            this.drv = drv;
        }

        public void Activate()
        {
            try
            {
                UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
                ViewerControlIfc sMViewerControl = viewControl.GetSMViewerControl();
                bool flag = false;
                if (sourceMap.lastView is SourceMapRegistrationView)
                {
                    try
                    {
                        SourceMapRegistrationView sourceMapRegistrationView =
                            (SourceMapRegistrationView)sourceMap.lastView;
                        if (sourceMapRegistrationView.locked)
                        {
                            if (sourceMap.ReadyToLock())
                            {
                                SetupLockedView();
                                uIPositionManager.GetVEPos()
                                    .setPosition(sourceMapRegistrationView.GetReferenceMapView());
                                flag = true;
                            }
                        }
                        else
                        {
                            SetupUnlockedView();
                            uIPositionManager.GetSMPos().setPosition(sourceMapRegistrationView.GetSourceMapView());
                            uIPositionManager.GetVEPos().setPosition(sourceMapRegistrationView.GetReferenceMapView());
                            flag = true;
                        }

                        viewControl.SetVEMapStyle(sourceMapRegistrationView.GetReferenceMapView().style);
                    }
                    catch (CorrespondencesAreSingularException)
                    {
                    }
                    catch (InsufficientCorrespondencesException)
                    {
                    }
                }

                if (!flag)
                {
                    SetupUnlockedView();
                    uIPositionManager.GetSMPos().setPosition(new ContinuousCoordinateSystem().GetDefaultView());
                    uIPositionManager.GetVEPos().setPosition(DefaultReferenceMapPosition(drv));
                }

                uIPositionManager.SetPositionMemory(sourceMap);
                viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.SourceMapOptions);
                viewControl.GetSourceMapInfoPanel().Configure(sourceMap);
                viewControl.GetSourceMapInfoPanel().Enabled = true;
                viewControl.GetTransparencyPanel().Configure(sourceMap, sMViewerControl);
                viewControl.GetTransparencyPanel().Enabled = true;
                viewControl.GetSMViewerControl().SetSnapViewStore(new SourceSnapView(this));
                viewControl.GetVEViewerControl().SetSnapViewStore(new RefSnapView(this));
                uIPositionManager.PositionUpdated();
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            viewControl.GetCachePackage().ClearSchedulers();
            UIPositionManager uIPositionManager = viewControl.GetUIPositionManager();
            uIPositionManager.SetPositionMemory(null);
            uIPositionManager.GetSMPos().setPosition(new LatLonZoom(0.0, 0.0, 0));
            uIPositionManager.switchFree();
            viewControl.GetSMViewerControl().ClearLayers();
            viewControl.GetSMViewerControl().SetSnapViewStore(null);
            viewControl.GetVEViewerControl().SetSnapViewStore(null);
            viewControl.SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
            viewControl.GetSourceMapInfoPanel().Configure(null);
            viewControl.GetSourceMapInfoPanel().Enabled = false;
            viewControl.GetTransparencyPanel().Configure(null, null);
            viewControl.GetTransparencyPanel().Enabled = false;
            viewControl.setDisplayedRegistration(null);
            sourceMap = null;
        }

        public void LockMaps()
        {
            try
            {
                LockMapsInternal();
            }
            catch (CorrespondencesAreSingularException)
            {
                MessageBox.Show(
                    "Ambiguous correspondences.\r\nIf some of your pushpins overlap, remove the redundant pushpins.\r\nOtherwise, some pushpins are perfectly colinear. Add more correspondences to complete lock.",
                    "Invalid correspondences",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
            }
        }

        private void LockMapsInternal()
        {
            if (mapsLocked)
            {
                throw new Exception("uh oh.  trying to lock already-locked maps!");
            }

            SetupLockedView();
            sourceMap.AutoSelectMaxZoom(mapTileSourceFactory);
            viewControl.GetUIPositionManager().PositionUpdated();
        }

        public void UnlockMaps()
        {
            if (!mapsLocked)
            {
                throw new Exception("uh oh.  trying to unlock maps that are already unlocked!");
            }

            SetupUnlockedView();
            ViewerControlIfc sMViewerControl = viewControl.GetSMViewerControl();
            MapRectangle bounds = viewControl.GetVEViewerControl().GetBounds();
            WarpedMapTileSource warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(sourceMap);
            IPointTransformer destLatLonToSourceTransformer = warpedMapTileSource.GetDestLatLonToSourceTransformer();
            MapRectangle newBounds = bounds.Transform(destLatLonToSourceTransformer);
            LatLonZoom latLonZoom = sMViewerControl.GetCoordinateSystem()
                .GetBestViewContaining(newBounds, sMViewerControl.Size);
            latLonZoom = CoordinateSystemUtilities.ConstrainLLZ(ContinuousCoordinateSystem.theInstance, latLonZoom);
            viewControl.GetUIPositionManager().GetSMPos().setPosition(latLonZoom);
            viewControl.GetUIPositionManager().PositionUpdated();
        }

        private void SetupLockedView()
        {
            WarpedMapTileSource warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(sourceMap);
            viewControl.GetSMViewerControl().ClearLayers();
            viewControl.GetSMViewerControl().SetBaseLayer(warpedMapTileSource);
            viewControl.GetUIPositionManager().switchSlaved();
            viewControl.setDisplayedRegistration(
                new RegistrationControlRecord(warpedMapTileSource.ComputeWarpedRegistration(), sourceMap));
            mapsLocked = true;
        }

        private void SetupUnlockedView()
        {
            viewControl.GetSMViewerControl().ClearLayers();
            viewControl.GetSMViewerControl()
                .SetBaseLayer(mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap));
            viewControl.GetSMViewerControl().SetLatentRegionHolder(sourceMap.latentRegionHolder);
            viewControl.GetUIPositionManager().switchFree();
            viewControl.setDisplayedRegistration(new RegistrationControlRecord(sourceMap.registration,
                sourceMap));
            mapsLocked = false;
        }

        internal bool MapsLocked()
        {
            return mapsLocked;
        }

        internal SourceMap GetSourceMap()
        {
            return sourceMap;
        }

        internal LatLonZoom DefaultReferenceMapPosition(DefaultReferenceView drv)
        {
            return DefaultReferenceMapPosition(sourceMap, mapTileSourceFactory, viewControl, drv);
        }

        internal static LatLonZoom DefaultReferenceMapPosition(SourceMap sourceMap,
            MapTileSourceFactory mapTileSourceFactory, ViewControlIfc viewControl, DefaultReferenceView drv)
        {
            if (sourceMap.ReadyToLock())
            {
                try
                {
                    ViewerControlIfc sMViewerControl = viewControl.GetSMViewerControl();
                    MapRectangle bounds = sMViewerControl.GetBounds();
                    WarpedMapTileSource warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(sourceMap);
                    IPointTransformer sourceToDestLatLonTransformer =
                        warpedMapTileSource.GetSourceToDestLatLonTransformer();
                    MapRectangle mapRectangle = bounds.Transform(sourceToDestLatLonTransformer);
                    mapRectangle = mapRectangle.ClipTo(new MapRectangle(-180.0, -360.0, 180.0, 360.0));
                    return viewControl.GetVEViewerControl().GetCoordinateSystem()
                        .GetBestViewContaining(mapRectangle, sMViewerControl.Size);
                }
                catch (CorrespondencesAreSingularException)
                {
                }
                catch (InsufficientCorrespondencesException)
                {
                }
            }

            if (drv != null && drv.present)
            {
                return drv.llz;
            }

            return viewControl.GetVEViewerControl().GetCoordinateSystem().GetDefaultView();
        }

        internal void PreviewSourceMapZoom()
        {
            if (!mapsLocked && sourceMap.ReadyToLock())
            {
                try
                {
                    LockMapsInternal();
                }
                catch (Exception)
                {
                }
            }

            if (mapsLocked)
            {
                viewControl.GetUIPositionManager().GetSMPos()
                    .setZoom(sourceMap.sourceMapRenderOptions.maxZoom);
            }
        }

        public object GetViewedObject()
        {
            return sourceMap;
        }

        internal void UpdateOverviewWindow(ViewerControl viewerControl)
        {
            if (sourceMap == null)
            {
                return;
            }

            viewerControl.ClearLayers();
            viewerControl.SetBaseLayer(mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap));
            viewerControl.setPinList(new List<PositionAssociationView>());
        }

        public void RecordSource(LatLonZoom llz)
        {
            if (mapsLocked)
            {
                RecordRef(llz);
                return;
            }

            sourceMap.sourceSnap = llz;
        }

        public LatLonZoom RestoreSource()
        {
            if (mapsLocked)
            {
                return RestoreRef();
            }

            return sourceMap.sourceSnap;
        }

        public void RecordRef(LatLonZoom llz)
        {
            sourceMap.referenceSnap = llz;
        }

        public LatLonZoom RestoreRef()
        {
            return sourceMap.referenceSnap;
        }

        public void RecordSourceZoom(int zoom)
        {
            if (mapsLocked)
            {
                RecordRefZoom(zoom);
                return;
            }

            sourceMap.sourceSnapZoom = zoom;
        }

        public int RestoreSourceZoom()
        {
            if (mapsLocked)
            {
                return RestoreRefZoom();
            }

            return sourceMap.sourceSnapZoom;
        }

        public void RecordRefZoom(int zoom)
        {
            sourceMap.referenceSnapZoom = zoom;
        }

        public int RestoreRefZoom()
        {
            return sourceMap.referenceSnapZoom;
        }
    }
}
