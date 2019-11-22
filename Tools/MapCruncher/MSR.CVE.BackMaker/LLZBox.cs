using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class LLZBox : UserControl, InvalidatableViewIfc
    {
        public enum LabelStyle
        {
            LatLon,
            XY
        }

        private DegreesMinutesSeconds dms = new DegreesMinutesSeconds();
        private MapDrawingOption _ShowDMS;
        private LatLonEditIfc latLonEdit;
        private LatLonZoom lastValue;
        private IContainer components;
        private GroupBox groupBox;
        private Label zoomLabel;
        private Label zoomLabel_text;
        private Label lonLabel_text;
        private Label latLabel_text;
        private TextBox lonText;
        private TextBox latText;

        public MapDrawingOption ShowDMS
        {
            set
            {
                _ShowDMS = value;
                _ShowDMS.SetInvalidatableView(this);
            }
        }

        public LLZBox()
        {
            InitializeComponent();
            latText.LostFocus += latText_TextChanged;
            lonText.LostFocus += lonText_TextChanged;
        }

        private void latText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double newLat = dms.ParseLatLon(latText.Text);
                latLonEdit.latEdited(newLat);
            }
            catch
            {
                PositionChanged(lastValue);
            }
        }

        private void lonText_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double newLon = dms.ParseLatLon(lonText.Text);
                latLonEdit.lonEdited(newLon);
            }
            catch
            {
                PositionChanged(lastValue);
            }
        }

        public void configureEditable(LatLonEditIfc latLonEdit)
        {
            this.latLonEdit = latLonEdit;
            latText.ReadOnly = false;
            lonText.ReadOnly = false;
        }

        public void setName(string name)
        {
            groupBox.Text = name;
        }

        public void PositionChanged(LatLonZoom llz)
        {
            lastValue = llz;
            InvalidateView();
        }

        public void InvalidateView()
        {
            dms.outputMode = _ShowDMS.Enabled
                ? DegreesMinutesSeconds.OutputMode.DMS
                : DegreesMinutesSeconds.OutputMode.DecimalDegrees;
            latText.Text = dms.FormatLatLon(lastValue.lat);
            lonText.Text = dms.FormatLatLon(lastValue.lon);
            zoomLabel.Text = lastValue.zoom.ToString();
        }

        public void SetLabelStyle(LabelStyle labelStyle)
        {
            switch (labelStyle)
            {
                case LabelStyle.LatLon:
                    latLabel_text.Text = "Latitude";
                    lonLabel_text.Text = "Longitude";
                    return;
                case LabelStyle.XY:
                    latLabel_text.Text = "Y";
                    lonLabel_text.Text = "X";
                    return;
                default:
                    return;
            }
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
            groupBox = new GroupBox();
            zoomLabel = new Label();
            zoomLabel_text = new Label();
            lonLabel_text = new Label();
            latLabel_text = new Label();
            latText = new TextBox();
            lonText = new TextBox();
            groupBox.SuspendLayout();
            SuspendLayout();
            groupBox.Controls.Add(lonText);
            groupBox.Controls.Add(latText);
            groupBox.Controls.Add(zoomLabel);
            groupBox.Controls.Add(zoomLabel_text);
            groupBox.Controls.Add(lonLabel_text);
            groupBox.Controls.Add(latLabel_text);
            groupBox.Location = new Point(3, 3);
            groupBox.Name = "groupBox";
            groupBox.Size = new Size(175, 74);
            groupBox.TabIndex = 10;
            groupBox.TabStop = false;
            groupBox.Text = "Map Location";
            zoomLabel.AutoSize = true;
            zoomLabel.Location = new Point(76, 56);
            zoomLabel.Name = "zoomLabel";
            zoomLabel.Size = new Size(13, 13);
            zoomLabel.TabIndex = 5;
            zoomLabel.Text = "0";
            zoomLabel_text.AutoSize = true;
            zoomLabel_text.Location = new Point(7, 56);
            zoomLabel_text.Name = "zoomLabel_text";
            zoomLabel_text.Size = new Size(63, 13);
            zoomLabel_text.TabIndex = 2;
            zoomLabel_text.Text = "Zoom Level";
            lonLabel_text.AutoSize = true;
            lonLabel_text.Location = new Point(7, 33);
            lonLabel_text.Name = "lonLabel_text";
            lonLabel_text.Size = new Size(54, 13);
            lonLabel_text.TabIndex = 1;
            lonLabel_text.Text = "Longitude";
            latLabel_text.AutoSize = true;
            latLabel_text.Location = new Point(7, 16);
            latLabel_text.Name = "latLabel_text";
            latLabel_text.Size = new Size(45, 13);
            latLabel_text.TabIndex = 0;
            latLabel_text.Text = "Latitude";
            latText.Location = new Point(69, 13);
            latText.Name = "latText";
            latText.ReadOnly = true;
            latText.Size = new Size(100, 20);
            latText.TabIndex = 6;
            lonText.Location = new Point(69, 33);
            lonText.Name = "lonText";
            lonText.ReadOnly = true;
            lonText.Size = new Size(100, 20);
            lonText.TabIndex = 7;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox);
            Name = "LLZBox";
            Size = new Size(181, 80);
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ResumeLayout(false);
        }
    }
}
