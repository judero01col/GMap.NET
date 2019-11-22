using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class HTMLMessageBox : Form
    {
        private IContainer components;
        private WebBrowser webBrowser1;
        private Button OKButton;

        public HTMLMessageBox() : this("Preview of <i>HTML</i> <b>message box</b>.", "Caption Argument")
        {
            InitializeComponent();
        }

        public HTMLMessageBox(string htmlContent, string caption)
        {
            InitializeComponent();
            webBrowser1.DocumentText = htmlContent;
            Text = caption;
        }

        public static DialogResult Show(string htmlContent, string caption)
        {
            HTMLMessageBox hTMLMessageBox = new HTMLMessageBox(htmlContent, caption);
            return hTMLMessageBox.ShowDialog();
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
            webBrowser1 = new WebBrowser();
            OKButton = new Button();
            SuspendLayout();
            webBrowser1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webBrowser1.Location = new Point(0, 0);
            webBrowser1.MinimumSize = new Size(20, 20);
            webBrowser1.Name = "webBrowser1";
            webBrowser1.Size = new Size(339, 254);
            webBrowser1.TabIndex = 0;
            OKButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            OKButton.DialogResult = DialogResult.OK;
            OKButton.Location = new Point(241, 265);
            OKButton.Name = "OKButton";
            OKButton.Size = new Size(79, 35);
            OKButton.TabIndex = 1;
            OKButton.Text = "OK";
            OKButton.UseVisualStyleBackColor = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(339, 311);
            Controls.Add(OKButton);
            Controls.Add(webBrowser1);
            Name = "HTMLMessageBox";
            Text = "HTMLMessageBox";
            ResumeLayout(false);
        }
    }
}
