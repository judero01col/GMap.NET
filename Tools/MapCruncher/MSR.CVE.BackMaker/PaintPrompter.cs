using System.Threading;
using System.Windows.Forms;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    internal class PaintPrompter
    {
        private delegate void UpdateDelegate();

        private Control paintee;

        private EventWaitHandle prompt =
            new CountedEventWaitHandle(false, EventResetMode.AutoReset, "PaintPrompter.prompt");

        private bool exit;

        public PaintPrompter(Control paintee)
        {
            this.paintee = paintee;
            DebugThreadInterrupter.theInstance.AddThread("PaintPrompter",
                Run,
                ThreadPriority.Normal);
        }

        public void Dispose()
        {
            exit = true;
            Prompt();
        }

        public void Prompt()
        {
            prompt.Set();
        }

        private void Run()
        {
            while (true)
            {
                prompt.WaitOne();
                if (exit)
                {
                    break;
                }

                paintee.Invalidate();
                UpdateDelegate method = paintee.Update;
                paintee.Invoke(method);
            }
        }
    }
}
