using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class RenderWindow : Form
    {
        private IContainer components;
        private RenderOptionsPanel renderOptionsPanel;
        private RenderProgressPanel2 renderProgressPanel;
        private GroupBox groupBox1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            groupBox1 = new GroupBox();
            renderOptionsPanel = new RenderOptionsPanel();
            renderProgressPanel = new RenderProgressPanel2();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                | AnchorStyles.Left;
            groupBox1.BackColor = SystemColors.Control;
            groupBox1.Controls.Add(renderOptionsPanel);
            groupBox1.Location = new Point(2, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(272, 542);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Mashup Render Options";
            // 
            // renderOptionsPanel
            // 
            renderOptionsPanel.BackColor = SystemColors.Control;
            renderOptionsPanel.Dock = DockStyle.Fill;
            renderOptionsPanel.Location = new Point(3, 18);
            renderOptionsPanel.Margin = new Padding(4, 4, 4, 4);
            renderOptionsPanel.Name = "renderOptionsPanel";
            renderOptionsPanel.Size = new Size(266, 521);
            renderOptionsPanel.TabIndex = 0;
            // 
            // renderProgressPanel
            // 
            renderProgressPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                          | AnchorStyles.Left
                                                          | AnchorStyles.Right;
            renderProgressPanel.BackColor = SystemColors.Control;
            renderProgressPanel.Location = new Point(280, 12);
            renderProgressPanel.Margin = new Padding(4, 4, 4, 4);
            renderProgressPanel.Name = "renderProgressPanel";
            renderProgressPanel.Size = new Size(799, 533);
            renderProgressPanel.TabIndex = 1;
            // 
            // RenderWindow
            // 
            ClientSize = new Size(1081, 547);
            Controls.Add(groupBox1);
            Controls.Add(renderProgressPanel);
            Name = "RenderWindow";
            Text = "Render";
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        public RenderWindow()
        {
            InitializeComponent();
            FormClosed += RenderWindow_FormClosed;
        }

        private void RenderWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            UndoConstruction();
        }

        internal void UndoConstruction()
        {
            renderOptionsPanel.SetRenderOptions(null);
            renderProgressPanel.UndoConstruction();
            base.Dispose();
        }

        internal void Setup(RenderOptions renderOptions, Mashup currentMashup,
            MapTileSourceFactory mapTileSourceFactory,
            RenderProgressPanel2.LaunchRenderedBrowserDelegate LaunchRenderedBrowser,
            RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage)
        {
            renderOptionsPanel.SetRenderOptions(renderOptions);
            renderProgressPanel.Setup(currentMashup,
                mapTileSourceFactory,
                LaunchRenderedBrowser,
                flushRenderedTileCachePackage);
        }

        internal void StartRender(RenderProgressPanel2.RenderCompleteDelegate renderCompleteDelegate)
        {
            renderProgressPanel.StartRender(renderCompleteDelegate);
        }
    }
}
