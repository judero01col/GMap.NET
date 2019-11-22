namespace MSR.CVE.BackMaker
{
    public interface IViewManager
    {
        void Activate();
        object GetViewedObject();
        void Dispose();
    }
}
