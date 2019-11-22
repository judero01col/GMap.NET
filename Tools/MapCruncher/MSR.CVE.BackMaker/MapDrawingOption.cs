using System;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class MapDrawingOption
    {
        private bool enabled;
        private InvalidatableViewIfc map;
        private ToolStripMenuItem menuItem;

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                map.InvalidateView();
            }
        }

        public MapDrawingOption(InvalidatableViewIfc map, ToolStripMenuItem menuItem, bool default_value)
        {
            this.map = map;
            this.menuItem = menuItem;
            enabled = default_value;
            if (this.menuItem != null)
            {
                this.menuItem.Checked = enabled;
                this.menuItem.Click += menuItem_Click;
            }
        }

        public void SetInvalidatableView(InvalidatableViewIfc map)
        {
            this.map = map;
        }

        public static bool IsEnabled(MapDrawingOption mdo)
        {
            return mdo != null && mdo.Enabled;
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            enabled = menuItem.Checked = !enabled;
            map.InvalidateView();
        }
    }
}
