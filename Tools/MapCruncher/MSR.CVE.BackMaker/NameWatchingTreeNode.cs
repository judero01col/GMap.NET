using System.Threading;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    internal class NameWatchingTreeNode : TreeNode
    {
        public NameWatchingTreeNode(HasDisplayNameIfc tag)
        {
            Init(tag);
        }

        public NameWatchingTreeNode(HasDisplayNameIfc tag, TreeNode[] children) : base(null, children)
        {
            Init(tag);
        }

        private void Init(HasDisplayNameIfc tag)
        {
            Tag = tag;
            UpdateName();
            if (tag is SourceMap)
            {
                ((SourceMap)tag).readyToLockChangedEvent.Add(UpdateNameListener);
            }
        }

        public void Dispose()
        {
            if (Tag is SourceMap)
            {
                ((SourceMap)Tag).readyToLockChangedEvent.Remove(UpdateNameListener);
            }
        }

        private void UpdateNameListener()
        {
            Monitor.Enter(this);
            try
            {
                if (TreeView != null)
                {
                    TreeView.Invoke(new DirtyListener(UpdateName));
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void UpdateName()
        {
            string text = ((HasDisplayNameIfc)Tag).GetDisplayName();
            if (Tag is SourceMap && !((SourceMap)Tag).ReadyToLock())
            {
                text += " (!)";
            }

            Text = text;
        }
    }
}
