using System;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LatentRegionHolder
    {
        private DirtyEvent dirtyEvent;
        private DirtyEvent boundsChangedEvent;
        public RenderRegion renderRegion;

        public LatentRegionHolder(DirtyEvent parentEvent, DirtyEvent parentBoundsAvailableEvent)
        {
            dirtyEvent = parentEvent;
            boundsChangedEvent = parentBoundsAvailableEvent;
        }

        public RenderRegion GetRenderRegionSynchronously(IFuture synchronousImageBoundsFuture)
        {
            Present present = synchronousImageBoundsFuture.Realize("LatentRegionHolder.GetRenderRegionSynchronously");
            StoreRenderRegion(present);
            if (renderRegion == null)
            {
                throw new Exception("Render region request failed, gasp: " + present.ToString());
            }

            return renderRegion;
        }

        public void RequestRenderRegion(IFuture asynchronousImageBoundsFuture)
        {
            if (renderRegion == null)
            {
                AsyncRef asyncRef =
                    (AsyncRef)asynchronousImageBoundsFuture.Realize("LatentRegionHolder.RequestRenderRegion");
                asyncRef.AddCallback(ImageBoundsAvailable);
                new PersistentInterest(asyncRef);
            }
        }

        private void ImageBoundsAvailable(AsyncRef asyncRef)
        {
            StoreRenderRegion(asyncRef.present);
        }

        private void StoreRenderRegion(Present present)
        {
            if (renderRegion == null && present is BoundsPresent)
            {
                BoundsPresent boundsPresent = (BoundsPresent)present;
                renderRegion = boundsPresent.GetRenderRegion().Copy(dirtyEvent);
                boundsChangedEvent.SetDirty();
            }
        }
    }
}
