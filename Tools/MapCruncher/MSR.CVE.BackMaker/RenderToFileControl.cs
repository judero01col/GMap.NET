using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class RenderToFileControl : UserControl
    {
        private RenderToFileOptions renderToFileOptions;
        private IContainer components;
        private TextBox textBox9;
        private Button selectOutputFolderButton;
        private TextBox outputFolderDisplayBox;

        public RenderToFileControl()
        {
            InitializeComponent();
            outputFolderDisplayBox.LostFocus += outputFolderDisplayBox_LostFocus;
        }

        public void Configure(RenderToFileOptions renderToFileOptions)
        {
            this.renderToFileOptions = renderToFileOptions;
            Reload();
        }

        private void outputFolderDisplayBox_LostFocus(object sender, EventArgs e)
        {
            renderToFileOptions.outputFolder = outputFolderDisplayBox.Text;
        }

        private void selectOutputFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Select Output Folder";
            DialogResult dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                renderToFileOptions.outputFolder = folderBrowserDialog.SelectedPath;
                Reload();
            }
        }

        private void Reload()
        {
            if (renderToFileOptions != null)
            {
                outputFolderDisplayBox.Text = renderToFileOptions.outputFolder;
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
            textBox9 = new TextBox();
            selectOutputFolderButton = new Button();
            outputFolderDisplayBox = new TextBox();
            SuspendLayout();
            textBox9.BackColor = SystemColors.ControlLightLight;
            textBox9.BorderStyle = BorderStyle.None;
            textBox9.Location = new Point(3, 3);
            textBox9.Name = "textBox9";
            textBox9.Size = new Size(67, 13);
            textBox9.TabIndex = 12;
            textBox9.TabStop = false;
            textBox9.Text = "Output Folder";
            selectOutputFolderButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            selectOutputFolderButton.Font =
                new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            selectOutputFolderButton.Location = new Point(247, 3);
            selectOutputFolderButton.Name = "selectOutputFolderButton";
            selectOutputFolderButton.Size = new Size(38, 20);
            selectOutputFolderButton.TabIndex = 13;
            selectOutputFolderButton.TabStop = false;
            selectOutputFolderButton.Text = " ...";
            selectOutputFolderButton.TextAlign = ContentAlignment.TopCenter;
            selectOutputFolderButton.UseVisualStyleBackColor = true;
            selectOutputFolderButton.Click += selectOutputFolderButton_Click;
            outputFolderDisplayBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            outputFolderDisplayBox.Location = new Point(3, 29);
            outputFolderDisplayBox.Name = "outputFolderDisplayBox";
            outputFolderDisplayBox.Size = new Size(282, 20);
            outputFolderDisplayBox.TabIndex = 11;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            Controls.Add(textBox9);
            Controls.Add(selectOutputFolderButton);
            Controls.Add(outputFolderDisplayBox);
            Name = "RenderToFileControl";
            Size = new Size(288, 59);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
