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
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupBox1 = new GroupBox();
            this.renderOptionsPanel = new RenderOptionsPanel();
            this.renderProgressPanel = new RenderProgressPanel2();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Bottom
                                                                    | AnchorStyles.Left);
            this.groupBox1.BackColor = SystemColors.Control;
            this.groupBox1.Controls.Add(this.renderOptionsPanel);
            this.groupBox1.Location = new Point(2, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(272, 542);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mashup Render Options";
            // 
            // renderOptionsPanel
            // 
            this.renderOptionsPanel.BackColor = SystemColors.Control;
            this.renderOptionsPanel.Dock = DockStyle.Fill;
            this.renderOptionsPanel.Location = new Point(3, 18);
            this.renderOptionsPanel.Margin = new Padding(4, 4, 4, 4);
            this.renderOptionsPanel.Name = "renderOptionsPanel";
            this.renderOptionsPanel.Size = new Size(266, 521);
            this.renderOptionsPanel.TabIndex = 0;
            // 
            // renderProgressPanel
            // 
            this.renderProgressPanel.Anchor = (AnchorStyles)(AnchorStyles.Top | AnchorStyles.Bottom
                                                                              | AnchorStyles.Left
                                                                              | AnchorStyles.Right);
            this.renderProgressPanel.BackColor = SystemColors.Control;
            this.renderProgressPanel.Location = new Point(280, 12);
            this.renderProgressPanel.Margin = new Padding(4, 4, 4, 4);
            this.renderProgressPanel.Name = "renderProgressPanel";
            this.renderProgressPanel.Size = new Size(799, 533);
            this.renderProgressPanel.TabIndex = 1;
            // 
            // RenderWindow
            // 
            this.ClientSize = new Size(1081, 547);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.renderProgressPanel);
            this.Name = "RenderWindow";
            this.Text = "Render";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        public RenderWindow()
        {
            this.InitializeComponent();
            base.FormClosed += new FormClosedEventHandler(this.RenderWindow_FormClosed);
        }

        private void RenderWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.UndoConstruction();
        }

        internal void UndoConstruction()
        {
            this.renderOptionsPanel.SetRenderOptions(null);
            this.renderProgressPanel.UndoConstruction();
            base.Dispose();
        }

        internal void Setup(RenderOptions renderOptions, Mashup currentMashup,
            MapTileSourceFactory mapTileSourceFactory,
            RenderProgressPanel2.LaunchRenderedBrowserDelegate LaunchRenderedBrowser,
            RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage)
        {
            this.renderOptionsPanel.SetRenderOptions(renderOptions);
            this.renderProgressPanel.Setup(currentMashup,
                mapTileSourceFactory,
                LaunchRenderedBrowser,
                flushRenderedTileCachePackage);
        }

        internal void StartRender(RenderProgressPanel2.RenderCompleteDelegate renderCompleteDelegate)
        {
            this.renderProgressPanel.StartRender(renderCompleteDelegate);
        }
    }
}
