namespace MSR.CVE.BackMaker
{
    internal class RefSnapView : SnapViewStoreIfc
    {
        private SourceMapViewManager smvm;

        public RefSnapView(SourceMapViewManager smvm)
        {
            this.smvm = smvm;
        }

        public void Record(LatLonZoom llz)
        {
            smvm.RecordRef(llz);
        }

        public LatLonZoom Restore()
        {
            return smvm.RestoreRef();
        }

        public void RecordZoom(int zoom)
        {
            smvm.RecordRefZoom(zoom);
        }

        public int RestoreZoom()
        {
            return smvm.RestoreRefZoom();
        }
    }
}
