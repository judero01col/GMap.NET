using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class RenderToS3Control : UserControl
    {
        private IContainer components;
        private Label label4;
        private Label label3;
        private Label label5;
        private TextBox s3PathPrefix;
        private TextBox s3Bucket;
        private TextBox s3CredentialsFilename;
        private Button credentialsBrowseButton;
        private Button editButton;
        private ToolTip toolTip1;
        private RenderToS3Options renderToS3Options;

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
            label4 = new Label();
            label3 = new Label();
            label5 = new Label();
            s3PathPrefix = new TextBox();
            s3Bucket = new TextBox();
            s3CredentialsFilename = new TextBox();
            credentialsBrowseButton = new Button();
            editButton = new Button();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            label4.AutoSize = true;
            label4.Location = new Point(3, 92);
            label4.Name = "label4";
            label4.Size = new Size(58, 13);
            label4.TabIndex = 15;
            label4.Text = "Path Prefix";
            label3.AutoSize = true;
            label3.Location = new Point(3, 62);
            label3.Name = "label3";
            label3.Size = new Size(41, 13);
            label3.TabIndex = 14;
            label3.Text = "Bucket";
            label5.AutoSize = true;
            label5.Location = new Point(3, 9);
            label5.Name = "label5";
            label5.Size = new Size(59, 13);
            label5.TabIndex = 12;
            label5.Text = "Credentials";
            s3PathPrefix.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s3PathPrefix.Location = new Point(68, 89);
            s3PathPrefix.Name = "s3PathPrefix";
            s3PathPrefix.Size = new Size(277, 20);
            s3PathPrefix.TabIndex = 11;
            s3Bucket.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s3Bucket.Location = new Point(68, 59);
            s3Bucket.Name = "s3Bucket";
            s3Bucket.Size = new Size(277, 20);
            s3Bucket.TabIndex = 10;
            s3CredentialsFilename.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s3CredentialsFilename.Location = new Point(68, 6);
            s3CredentialsFilename.Name = "s3CredentialsFilename";
            s3CredentialsFilename.Size = new Size(275, 20);
            s3CredentialsFilename.TabIndex = 8;
            toolTip1.SetToolTip(s3CredentialsFilename,
                "Path to file containing credentials for render upload");
            credentialsBrowseButton.Location = new Point(68, 32);
            credentialsBrowseButton.Name = "credentialsBrowseButton";
            credentialsBrowseButton.Size = new Size(66, 20);
            credentialsBrowseButton.TabIndex = 16;
            credentialsBrowseButton.Text = "Select...";
            toolTip1.SetToolTip(credentialsBrowseButton,
                "Select existing or create new credentials file from browser.");
            credentialsBrowseButton.UseVisualStyleBackColor = true;
            credentialsBrowseButton.Click += credentialsBrowseButton_Click;
            editButton.Location = new Point(140, 32);
            editButton.Name = "editButton";
            editButton.Size = new Size(66, 20);
            editButton.TabIndex = 17;
            editButton.Text = "Edit...";
            toolTip1.SetToolTip(editButton, "Edit this credentials file.");
            editButton.UseVisualStyleBackColor = true;
            editButton.Click += editButton_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            Controls.Add(editButton);
            Controls.Add(credentialsBrowseButton);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label5);
            Controls.Add(s3PathPrefix);
            Controls.Add(s3Bucket);
            Controls.Add(s3CredentialsFilename);
            Name = "RenderToS3Control";
            Size = new Size(346, 117);
            ResumeLayout(false);
            PerformLayout();
        }

        public RenderToS3Control()
        {
            InitializeComponent();
            s3CredentialsFilename.LostFocus += s3Credentials_LostFocus;
            UpdateButtons();
            s3Bucket.LostFocus += s3Bucket_LostFocus;
            s3PathPrefix.LostFocus += s3PathPrefix_LostFocus;
        }

        private void s3PathPrefix_LostFocus(object sender, EventArgs e)
        {
            renderToS3Options.s3pathPrefix = s3PathPrefix.Text;
        }

        private void s3Bucket_LostFocus(object sender, EventArgs e)
        {
            renderToS3Options.s3bucket = s3Bucket.Text;
        }

        private void s3Credentials_LostFocus(object sender, EventArgs e)
        {
            renderToS3Options.s3credentialsFilename = s3CredentialsFilename.Text;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            editButton.Enabled = FileIsReadable();
        }

        public void Configure(RenderToS3Options renderToS3Options)
        {
            this.renderToS3Options = renderToS3Options;
            Reload();
        }

        private void Reload()
        {
            if (renderToS3Options != null)
            {
                s3CredentialsFilename.Text = renderToS3Options.s3credentialsFilename;
                UpdateButtons();
                s3Bucket.Text = renderToS3Options.s3bucket;
                s3PathPrefix.Text = renderToS3Options.s3pathPrefix;
            }
        }

        private bool FileIsReadable()
        {
            bool result;
            try
            {
                new S3Credentials(renderToS3Options.s3credentialsFilename, false);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        private void credentialsBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.SupportMultiDottedExtensions = true;
            openFileDialog.DefaultExt = ".cred.xml";
            openFileDialog.Filter = "Credentials File (*.cred.xml)|*.cred.xml" + BuildConfig.theConfig.allFilesOption;
            openFileDialog.CheckFileExists = false;
            DialogResult dialogResult = openFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
            {
                return;
            }

            renderToS3Options.s3credentialsFilename = openFileDialog.FileName;
            if (!FileIsReadable())
            {
                EditFile();
                if (!FileIsReadable())
                {
                    renderToS3Options.s3credentialsFilename = "";
                }
            }

            Reload();
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            EditFile();
        }

        private void EditFile()
        {
            S3Credentials s3Credentials = new S3Credentials(renderToS3Options.s3credentialsFilename, true);
            S3CredentialsForm s3CredentialsForm = new S3CredentialsForm();
            s3CredentialsForm.Initialize(s3Credentials);
            DialogResult dialogResult = s3CredentialsForm.ShowDialog();
            if (dialogResult == DialogResult.Yes)
            {
                s3CredentialsForm.LoadResult(s3Credentials);
                s3Credentials.WriteXML();
            }
        }
    }
}
