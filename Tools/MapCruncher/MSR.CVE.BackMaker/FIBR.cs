using System.IO;

namespace MSR.CVE.BackMaker
{
    internal class FIBR
    {
        private StreamWriter sw;
        private static FIBR theFIBR = new FIBR();

        private FIBR()
        {
        }

        private void Dispose()
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
        }

        private void Write(string message)
        {
            if (sw != null)
            {
                sw.WriteLine(message);
                sw.Flush();
            }
        }

        public static void Announce(string methodName, params object[] paramList)
        {
        }
    }
}
