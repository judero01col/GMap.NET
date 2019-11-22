using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapInfoPanel : UserControl
    {
        public delegate void PreviewSourceMapZoomDelegate(SourceMap sourceMap);

        private SourceMap sourceMap;
        private bool needUpdate;
        private PreviewSourceMapZoomDelegate previewSourceMapZoom;
        private IContainer components;
        private Label label1;
        private TextBox mapFileURLTextBox;
        private Label label2;
        private TextBox mapHomePageTextBox;
        private TextBox mapDescriptionTextBox;
        private Panel panel1;
        private Label label3;
        private TextBox textBox7;
        private NumericUpDown closestZoomUpDown;

        public SourceMapInfoPanel()
        {
            InitializeComponent();
            mapFileURLTextBox.LostFocus += mapFileURLTextBox_LostFocus;
            mapHomePageTextBox.LostFocus += mapHomePageTextBox_LostFocus;
            mapDescriptionTextBox.LostFocus += descriptionTextBox_LostFocus;
            MercatorCoordinateSystem mercatorCoordinateSystem = new MercatorCoordinateSystem();
            closestZoomUpDown.Minimum = mercatorCoordinateSystem.GetZoomRange().min;
            closestZoomUpDown.Maximum = mercatorCoordinateSystem.GetZoomRange().max;
        }

        public void Initialize(PreviewSourceMapZoomDelegate previewSourceMapZoom)
        {
            this.previewSourceMapZoom = previewSourceMapZoom;
        }

        private void descriptionTextBox_LostFocus(object sender, EventArgs e)
        {
            if (sourceMap != null)
            {
                sourceMap.sourceMapInfo.mapDescription = ((TextBox)sender).Text;
            }
        }

        private void mapHomePageTextBox_LostFocus(object sender, EventArgs e)
        {
            if (sourceMap != null)
            {
                sourceMap.sourceMapInfo.mapHomePage = ((TextBox)sender).Text;
            }
        }

        private void mapFileURLTextBox_LostFocus(object sender, EventArgs e)
        {
            if (sourceMap != null)
            {
                sourceMap.sourceMapInfo.mapFileURL = ((TextBox)sender).Text;
            }
        }

        public void Configure(SourceMap sourceMap)
        {
            if (this.sourceMap != null)
            {
                this.sourceMap.sourceMapRenderOptions.dirtyEvent.Remove(ZoomChangedHandler);
            }

            this.sourceMap = sourceMap;
            if (this.sourceMap != null)
            {
                this.sourceMap.sourceMapRenderOptions.dirtyEvent.Add(ZoomChangedHandler);
            }

            update();
        }

        private void update()
        {
            if (sourceMap != null)
            {
                mapFileURLTextBox.Text = sourceMap.sourceMapInfo.mapFileURL;
                mapHomePageTextBox.Text = sourceMap.sourceMapInfo.mapHomePage;
                mapDescriptionTextBox.Text = sourceMap.sourceMapInfo.mapDescription;
            }
            else
            {
                mapFileURLTextBox.Text = "";
                mapHomePageTextBox.Text = "";
                mapDescriptionTextBox.Text = "";
            }

            if (sourceMap == null || sourceMap.sourceMapRenderOptions.maxZoom == -1)
            {
                closestZoomUpDown.Value = closestZoomUpDown.Minimum;
                closestZoomUpDown.Enabled = false;
                return;
            }

            closestZoomUpDown.Value = sourceMap.sourceMapRenderOptions.maxZoom;
            closestZoomUpDown.Enabled = true;
        }

        private void ZoomChangedHandler()
        {
            needUpdate = true;
            Invalidate();
        }

        private void closestZoomUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (sourceMap != null)
            {
                sourceMap.sourceMapRenderOptions.maxZoom = Convert.ToInt32(closestZoomUpDown.Value);
                previewSourceMapZoom(sourceMap);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (D.CustomPaintDisabled())
            {
                return;
            }

            if (needUpdate)
            {
                update();
                needUpdate = false;
            }

            base.OnPaint(e);
        }

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
            label1 = new Label();
            mapFileURLTextBox = new TextBox();
            label2 = new Label();
            mapHomePageTextBox = new TextBox();
            mapDescriptionTextBox = new TextBox();
            panel1 = new Panel();
            textBox7 = new TextBox();
            closestZoomUpDown = new NumericUpDown();
            label3 = new Label();
            panel1.SuspendLayout();
            ((ISupportInitialize)closestZoomUpDown).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(75, 13);
            label1.TabIndex = 0;
            label1.Text = "Map File URL:";
            mapFileURLTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mapFileURLTextBox.Location = new Point(6, 20);
            mapFileURLTextBox.Name = "mapFileURLTextBox";
            mapFileURLTextBox.Size = new Size(300, 20);
            mapFileURLTextBox.TabIndex = 1;
            label2.AutoSize = true;
            label2.Location = new Point(3, 96);
            label2.Name = "label2";
            label2.Size = new Size(157, 13);
            label2.TabIndex = 0;
            label2.Text = "Map description and comments:";
            mapHomePageTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mapHomePageTextBox.Location = new Point(6, 68);
            mapHomePageTextBox.Name = "mapHomePageTextBox";
            mapHomePageTextBox.Size = new Size(300, 20);
            mapHomePageTextBox.TabIndex = 1;
            mapDescriptionTextBox.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mapDescriptionTextBox.Location = new Point(6, 116);
            mapDescriptionTextBox.Multiline = true;
            mapDescriptionTextBox.Name = "mapDescriptionTextBox";
            mapDescriptionTextBox.Size = new Size(300, 198);
            mapDescriptionTextBox.TabIndex = 1;
            panel1.Controls.Add(textBox7);
            panel1.Controls.Add(closestZoomUpDown);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(mapDescriptionTextBox);
            panel1.Controls.Add(mapFileURLTextBox);
            panel1.Controls.Add(mapHomePageTextBox);
            panel1.Controls.Add(label2);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(309, 365);
            panel1.TabIndex = 2;
            textBox7.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textBox7.BackColor = SystemColors.ControlLight;
            textBox7.BorderStyle = BorderStyle.None;
            textBox7.Location = new Point(6, 345);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(132, 13);
            textBox7.TabIndex = 9;
            textBox7.TabStop = false;
            textBox7.Text = "Maximum (Closest) Zoom";
            closestZoomUpDown.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            closestZoomUpDown.Location = new Point(260, 342);
            closestZoomUpDown.Name = "closestZoomUpDown";
            closestZoomUpDown.Size = new Size(46, 20);
            closestZoomUpDown.TabIndex = 8;
            closestZoomUpDown.ValueChanged += closestZoomUpDown_ValueChanged;
            label3.AutoSize = true;
            label3.Location = new Point(3, 48);
            label3.Name = "label3";
            label3.Size = new Size(87, 13);
            label3.TabIndex = 2;
            label3.Text = "Map home page:";
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Name = "SourceMapOptions";
            Size = new Size(309, 365);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((ISupportInitialize)closestZoomUpDown).EndInit();
            ResumeLayout(false);
        }
    }
}
