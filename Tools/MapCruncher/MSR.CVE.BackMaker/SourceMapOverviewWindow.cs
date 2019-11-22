using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapOverviewWindow : Form
    {
        public delegate void ClosedDelegate();

        private IContainer components;
        public ViewerControl viewerControl;
        private MapPosition mapPos;
        private ClosedDelegate closedDelegate;

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
            viewerControl = new ViewerControl();
            SuspendLayout();
            viewerControl.Dock = DockStyle.Fill;
            viewerControl.Location = new Point(0, 0);
            viewerControl.Name = "viewerControl";
            viewerControl.Size = new Size(380, 351);
            viewerControl.TabIndex = 0;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(380, 351);
            Controls.Add(viewerControl);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Name = "SourceMapOverviewWindow";
            Text = "SourceMapOverviewWindow";
            TopMost = true;
            ResumeLayout(false);
        }

        public SourceMapOverviewWindow()
        {
            InitializeComponent();
            mapPos = new MapPosition(viewerControl);
            viewerControl.Initialize(GetMapPos, "Overview");
        }

        public void Initialize(ClosedDelegate closedDelegate, MapDrawingOption ShowDMS)
        {
            this.closedDelegate = closedDelegate;
            viewerControl.ShowDMS = ShowDMS;
            mapPos.setPosition(new ContinuousCoordinateSystem().GetDefaultView());
            Closed += SourceMapOverviewWindow_Closed;
        }

        private void SourceMapOverviewWindow_Closed(object sender, EventArgs e)
        {
            closedDelegate();
        }

        private MapPosition GetMapPos()
        {
            return mapPos;
        }

        private void SetDefaultView()
        {
            mapPos.setPosition(viewerControl.GetCoordinateSystem().GetDefaultView());
        }
    }
}
