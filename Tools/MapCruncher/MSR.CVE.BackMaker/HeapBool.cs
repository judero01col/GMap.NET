namespace MSR.CVE.BackMaker
{
    public class HeapBool
    {
        private bool b;

        public bool value
        {
            get
            {
                return b;
            }
            set
            {
                b = value;
            }
        }

        public HeapBool(bool initialValue)
        {
            b = initialValue;
        }
    }
}
