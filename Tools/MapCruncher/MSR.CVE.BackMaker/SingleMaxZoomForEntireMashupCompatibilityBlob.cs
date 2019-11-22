namespace MSR.CVE.BackMaker
{
    public class SingleMaxZoomForEntireMashupCompatibilityBlob
    {
        private int _minZoom = 1;
        private int _maxZoom;

        public int minZoom
        {
            get
            {
                return _minZoom;
            }
            set
            {
                if (value != _minZoom)
                {
                    _minZoom = value;
                }
            }
        }

        public int maxZoom
        {
            get
            {
                return _maxZoom;
            }
            set
            {
                if (value != _maxZoom)
                {
                    _maxZoom = value;
                }
            }
        }
    }
}
