using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class RenderOptionsPanel : UserControl
    {
        private IContainer components;
        private CheckBox publishSourcesCheckbox;
        private TextBox publishSourcesLabel;
        private ToolTip publishSourceMapsTip;
        private Panel panel1;
        private CheckBox permitCompositionCheckbox;
        private RadioButton renderToS3radio;
        private Label label1;
        private RadioButton renderToFileRadio;
        private bool needReload;
        private RenderOptions renderOptions;
        private Control renderToControl;

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
            components = new Container();
            publishSourcesCheckbox = new CheckBox();
            publishSourcesLabel = new TextBox();
            publishSourceMapsTip = new ToolTip(components);
            permitCompositionCheckbox = new CheckBox();
            panel1 = new Panel();
            renderToFileRadio = new RadioButton();
            label1 = new Label();
            renderToS3radio = new RadioButton();
            panel1.SuspendLayout();
            SuspendLayout();
            publishSourcesCheckbox.AutoSize = true;
            publishSourcesCheckbox.Location = new Point(3, 198);
            publishSourcesCheckbox.Name = "publishSourcesCheckbox";
            publishSourcesCheckbox.Size = new Size(15, 14);
            publishSourcesCheckbox.TabIndex = 15;
            publishSourceMapsTip.SetToolTip(publishSourcesCheckbox,
                "Provides site visitors with all of the data needed to re-render your crunchup.");
            publishSourcesCheckbox.UseVisualStyleBackColor = true;
            publishSourcesCheckbox.CheckedChanged += publishSourcesCheckbox_CheckedChanged;
            publishSourcesLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            publishSourcesLabel.BackColor = SystemColors.ControlLightLight;
            publishSourcesLabel.BorderStyle = BorderStyle.None;
            publishSourcesLabel.Location = new Point(24, 198);
            publishSourcesLabel.Multiline = true;
            publishSourcesLabel.Name = "publishSourcesLabel";
            publishSourcesLabel.ReadOnly = true;
            publishSourcesLabel.Size = new Size(305, 35);
            publishSourcesLabel.TabIndex = 16;
            publishSourcesLabel.Text = "Copy source maps and crunchup data to output folder";
            publishSourceMapsTip.SetToolTip(publishSourcesLabel,
                "Provides site visitors with all of the data needed to re-render your crunchup.");
            publishSourcesLabel.TextChanged += textBox1_TextChanged;
            publishSourceMapsTip.Popup += publishSourceMapsTip_Popup;
            permitCompositionCheckbox.AutoSize = true;
            permitCompositionCheckbox.Location = new Point(3, 239);
            permitCompositionCheckbox.Name = "permitCompositionCheckbox";
            permitCompositionCheckbox.Size = new Size(114, 17);
            permitCompositionCheckbox.TabIndex = 15;
            permitCompositionCheckbox.Text = "Permit composition";
            permitCompositionCheckbox.UseVisualStyleBackColor = true;
            permitCompositionCheckbox.CheckedChanged +=
                permitCompositionCheckbox_CheckedChanged;
            panel1.BackColor = SystemColors.ControlLightLight;
            panel1.Controls.Add(renderToS3radio);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(renderToFileRadio);
            panel1.Controls.Add(publishSourcesLabel);
            panel1.Controls.Add(permitCompositionCheckbox);
            panel1.Controls.Add(publishSourcesCheckbox);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(332, 362);
            panel1.TabIndex = 18;
            panel1.Paint += panel1_Paint;
            renderToFileRadio.AutoSize = true;
            renderToFileRadio.Location = new Point(19, 23);
            renderToFileRadio.Name = "renderToFileRadio";
            renderToFileRadio.Size = new Size(41, 17);
            renderToFileRadio.TabIndex = 18;
            renderToFileRadio.TabStop = true;
            renderToFileRadio.Text = "File";
            renderToFileRadio.UseVisualStyleBackColor = true;
            renderToFileRadio.CheckedChanged += renderToFileRadio_CheckedChanged;
            label1.AutoSize = true;
            label1.Location = new Point(3, 7);
            label1.Name = "label1";
            label1.Size = new Size(57, 13);
            label1.TabIndex = 19;
            label1.Text = "Render to:";
            label1.Click += label1_Click;
            renderToS3radio.AutoSize = true;
            renderToS3radio.Location = new Point(79, 23);
            renderToS3radio.Name = "renderToS3radio";
            renderToS3radio.Size = new Size(38, 17);
            renderToS3radio.TabIndex = 20;
            renderToS3radio.TabStop = true;
            renderToS3radio.Text = "S3";
            renderToS3radio.UseVisualStyleBackColor = true;
            renderToS3radio.CheckedChanged += renderToS3radio_CheckedChanged;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(panel1);
            Name = "RenderOptionsPanel";
            Size = new Size(332, 362);
            publishSourceMapsTip.SetToolTip(this,
                "Use the Render tab after the maps are locked to create your mashup.");
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        public RenderOptionsPanel()
        {
            InitializeComponent();
            publishSourceMapsTip.SetToolTip(permitCompositionCheckbox,
                "Places the " + CrunchedFile.CrunchedFilename +
                " file into the public domain so that it may be composed with other map applications.");
            string caption = "Provides site visitors with all of the data needed to re-render your crunchup.";
            publishSourceMapsTip.SetToolTip(publishSourcesCheckbox, caption);
            publishSourceMapsTip.SetToolTip(publishSourcesLabel, caption);
        }

        public void SetRenderOptions(RenderOptions renderOptions)
        {
            if (!BuildConfig.theConfig.enableS3)
            {
                renderToFileRadio.Visible = false;
                renderToS3radio.Visible = false;
            }

            if (this.renderOptions != null)
            {
                this.renderOptions.dirtyEvent.Remove(PromptReload);
            }

            this.renderOptions = renderOptions;
            if (this.renderOptions != null)
            {
                this.renderOptions.dirtyEvent.Add(PromptReload);
            }

            PromptReload();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (D.CustomPaintDisabled())
            {
                return;
            }

            if (needReload)
            {
                Reload();
            }

            base.OnPaint(e);
        }

        private void PromptReload()
        {
            needReload = true;
            Invalidate();
        }

        private void Reload()
        {
            needReload = false;
            Enabled = renderOptions != null;
            if (renderOptions != null)
            {
                publishSourcesCheckbox.Checked = renderOptions.publishSourceData;
                permitCompositionCheckbox.Checked = renderOptions.permitComposition;
                renderToFileRadio.Checked = renderOptions.renderToOptions is RenderToFileOptions;
                renderToS3radio.Checked = renderOptions.renderToOptions is RenderToS3Options;
                if (renderOptions.renderToOptions is RenderToFileOptions &&
                    !(renderToControl is RenderToFileControl))
                {
                    destroyRenderToControl();
                    RenderToFileControl renderToFileControl = new RenderToFileControl();
                    renderToFileControl.Configure((RenderToFileOptions)renderOptions.renderToOptions);
                    renderToFileControl.Location = new Point(0, 46);
                    renderToControl = renderToFileControl;
                }

                if (renderOptions.renderToOptions is RenderToS3Options &&
                    !(renderToControl is RenderToS3Control))
                {
                    destroyRenderToControl();
                    RenderToS3Control renderToS3Control = new RenderToS3Control();
                    renderToS3Control.Configure((RenderToS3Options)renderOptions.renderToOptions);
                    renderToS3Control.Location = new Point(0, 46);
                    renderToControl = renderToS3Control;
                }

                renderToControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                renderToControl.Size = new Size(panel1.Width, renderToControl.Height);
                panel1.Controls.Add(renderToControl);
            }
        }

        private void destroyRenderToControl()
        {
            if (renderToControl != null)
            {
                renderToControl.Dispose();
                renderToControl = null;
            }
        }

        private void publishSourcesCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            renderOptions.publishSourceData = ((CheckBox)sender).Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            publishSourcesCheckbox_CheckedChanged(sender, e);
        }

        private void permitCompositionCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            renderOptions.permitComposition = ((CheckBox)sender).Checked;
        }

        private void publishSourceMapsTip_Popup(object sender, PopupEventArgs e)
        {
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void renderToFileRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (renderToFileRadio.Checked && !(renderOptions.renderToOptions is RenderToFileOptions))
            {
                renderOptions.renderToOptions = new RenderToFileOptions(renderOptions.dirtyEvent);
            }
        }

        private void renderToS3radio_CheckedChanged(object sender, EventArgs e)
        {
            if (renderToS3radio.Checked && !(renderOptions.renderToOptions is RenderToS3Options))
            {
                renderOptions.renderToOptions = new RenderToS3Options(renderOptions.dirtyEvent);
            }
        }
    }
}
