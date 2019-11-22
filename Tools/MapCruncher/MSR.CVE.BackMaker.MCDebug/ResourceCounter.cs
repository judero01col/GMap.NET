using System;

namespace MSR.CVE.BackMaker.MCDebug
{
    public class ResourceCounter
    {
        public delegate void NotifyDelegate(ResourceCounter resourceCounter);

        private string resourceName;
        private int period;
        private int _value;
        private int lastRoundedValue;
        private NotifyDelegate notifyDelegate;

        public int Value
        {
            get
            {
                return _value;
            }
        }

        public ResourceCounter(string resourceName, int period, NotifyDelegate notifyDelegate)
        {
            this.resourceName = resourceName;
            this.period = period;
            this.notifyDelegate = notifyDelegate;
        }

        public void crement(int crement)
        {
            _value += crement;
            if (period > 0)
            {
                int num = _value / period;
                if (Math.Abs(lastRoundedValue - num) > 1)
                {
                    D.Sayf(0, "Resource {0} value {1}", new object[] {resourceName, _value});
                    lastRoundedValue = num;
                }
            }

            if (notifyDelegate != null)
            {
                notifyDelegate(this);
            }
        }

        internal void SetValue(int newValue)
        {
            _value = newValue;
            if (notifyDelegate != null)
            {
                notifyDelegate(this);
            }
        }
    }
}
