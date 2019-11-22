using System;

namespace MSR.CVE.BackMaker.MCDebug
{
    internal class Clocker
    {
        private bool started;
        private DateTime startTime;
        public static Clocker theClock = new Clocker();

        public int stamp()
        {
            start();
            return (int)DateTime.Now.Subtract(startTime).TotalSeconds;
        }

        private void start()
        {
            if (started)
            {
                return;
            }

            startTime = DateTime.Now;
            started = true;
        }
    }
}
