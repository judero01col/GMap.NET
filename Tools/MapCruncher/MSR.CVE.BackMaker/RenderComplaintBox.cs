using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public class RenderComplaintBox
    {
        public delegate void AnnounceDelegate(string complaint);

        private AnnounceDelegate announce;

        private Dictionary<NonredundantRenderComplaint, bool> complaints =
            new Dictionary<NonredundantRenderComplaint, bool>();

        public RenderComplaintBox(AnnounceDelegate announce)
        {
            this.announce = announce;
        }

        public void Complain(NonredundantRenderComplaint complaint)
        {
            if (!complaints.ContainsKey(complaint))
            {
                complaints.Add(complaint, false);
                announce(complaint.ToString());
            }
        }
    }
}
