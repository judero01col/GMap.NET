using System;

namespace MSR.CVE.BackMaker
{
    public class NonredundantRenderComplaint : Exception
    {
        public string message
        {
            get;
        }

        public NonredundantRenderComplaint(string message)
        {
            this.message = message;
        }

        public override int GetHashCode()
        {
            return message.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is NonredundantRenderComplaint &&
                   message.Equals(((NonredundantRenderComplaint)obj).message);
        }

        public override string ToString()
        {
            return message;
        }
    }
}
