namespace MSR.CVE.BackMaker
{
    internal class SourceSnapView : SnapViewStoreIfc
    {
        private SourceMapViewManager smvm;

        public SourceSnapView(SourceMapViewManager smvm)
        {
            this.smvm = smvm;
        }

        public void Record(LatLonZoom llz)
        {
            smvm.RecordSource(llz);
        }

        public LatLonZoom Restore()
        {
            return smvm.RestoreSource();
        }

        public void RecordZoom(int zoom)
        {
            smvm.RecordSourceZoom(zoom);
        }

        public int RestoreZoom()
        {
            return smvm.RestoreSourceZoom();
        }
    }
}
