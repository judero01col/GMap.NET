using System;

namespace MSR.CVE.BackMaker.MCDebug
{
    public class BigDebugKnob
    {
        public delegate void DebugKnobListener(bool enabled);

        private bool _debugFeaturesEnabled;
        public static BigDebugKnob theKnob = new BigDebugKnob();

        private event DebugKnobListener listeners;

        public bool debugFeaturesEnabled
        {
            get
            {
                return _debugFeaturesEnabled;
            }
            set
            {
                _debugFeaturesEnabled = value;
                listeners(_debugFeaturesEnabled);
            }
        }

        public void AddListener(DebugKnobListener listener)
        {
            listeners = (DebugKnobListener)Delegate.Combine(listeners, listener);
        }
    }
}
