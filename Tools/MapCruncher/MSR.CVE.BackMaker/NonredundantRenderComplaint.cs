using System;

namespace MSR.CVE.BackMaker
{
    public class NonredundantRenderComplaint : Exception
    {
        private string _message;

        public string message
        {
            get
            {
                return _message;
            }
        }

        public NonredundantRenderComplaint(string message)
        {
            _message = message;
        }

        public override int GetHashCode()
        {
            return _message.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is NonredundantRenderComplaint &&
                   _message.Equals(((NonredundantRenderComplaint)obj)._message);
        }

        public override string ToString()
        {
            return _message;
        }
    }
}
