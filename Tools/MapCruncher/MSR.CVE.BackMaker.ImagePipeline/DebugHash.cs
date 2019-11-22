using System.Drawing;
using System.Text;

namespace MSR.CVE.BackMaker.ImagePipeline
{
    public class DebugHash : IRobustHash
    {
        private StringBuilder sb = new StringBuilder();

        public void Accumulate(int input)
        {
            Accumulate(input.ToString());
        }

        public void Accumulate(long input)
        {
            Accumulate(input.ToString());
        }

        public void Accumulate(Size size)
        {
            Accumulate(size.ToString());
        }

        public void Accumulate(double value)
        {
            Accumulate(value.ToString());
        }

        public void Accumulate(bool value)
        {
            Accumulate(value.ToString());
        }

        public void Accumulate(string value)
        {
            sb.Append(value);
            sb.Append(",");
        }

        public override string ToString()
        {
            return sb.ToString();
        }
    }
}
