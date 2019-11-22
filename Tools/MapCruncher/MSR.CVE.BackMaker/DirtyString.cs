namespace MSR.CVE.BackMaker
{
    public class DirtyString
    {
        private string _myValue;
        public DirtyEvent dirtyEvent;

        public string myValue
        {
            get
            {
                if (_myValue == null)
                {
                    return "";
                }

                return _myValue;
            }
            set
            {
                if (value != _myValue)
                {
                    _myValue = value;
                    dirtyEvent.SetDirty();
                }
            }
        }

        public DirtyString(DirtyEvent parentDirtyEvent)
        {
            dirtyEvent = new DirtyEvent(parentDirtyEvent);
        }
    }
}
