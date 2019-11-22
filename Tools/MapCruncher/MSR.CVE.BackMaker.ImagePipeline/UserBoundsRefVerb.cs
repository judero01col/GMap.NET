namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class UserBoundsRefVerb : Verb
    {
        private RenderRegion userRegion;
        private IFuture delayedStaticBoundsFuture;

        public UserBoundsRefVerb(LatentRegionHolder latentRegionHolder, IFuture delayedStaticBoundsFuture)
        {
            RenderRegion renderRegion = latentRegionHolder.renderRegion;
            if (renderRegion == null)
            {
                userRegion = null;
            }
            else
            {
                userRegion = renderRegion.Copy(new DirtyEvent());
            }

            this.delayedStaticBoundsFuture = delayedStaticBoundsFuture;
        }

        public Present Evaluate(Present[] paramList)
        {
            D.Assert(paramList.Length == 0);
            if (userRegion != null)
            {
                return new BoundsPresent(userRegion);
            }

            Present present = delayedStaticBoundsFuture.Realize("UserBoundsRefVerb.Evaluate");
            if (present is BoundsPresent)
            {
                return present;
            }

            if (present is PresentFailureCode)
            {
                return new PresentFailureCode((PresentFailureCode)present, "BoundsPresent.Evaluate");
            }

            return new PresentFailureCode(string.Format("Unrecognized Present type {0} in BoundsPresent.Evaluate",
                present.GetType()));
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("UserBoundsRefVerb(");
            if (userRegion != null)
            {
                userRegion.AccumulateRobustHash(hash);
            }
            else
            {
                hash.Accumulate("null");
            }

            delayedStaticBoundsFuture.AccumulateRobustHash(hash);
            hash.Accumulate(")");
        }
    }
}
