using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class S3CredentialsForm : Form
    {
        private IContainer components;
        private Label label2;
        private Label label5;
        private TextBox s3SecretAccessKey;
        private TextBox s3AccessKeyId;
        private Button saveButton;
        private Button cancelButton;

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
            label2 = new Label();
            label5 = new Label();
            s3SecretAccessKey = new TextBox();
            s3AccessKeyId = new TextBox();
            saveButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            label2.AutoSize = true;
            label2.Location = new Point(9, 55);
            label2.Name = "label2";
            label2.Size = new Size(97, 13);
            label2.TabIndex = 17;
            label2.Text = "Secret Access Key";
            label5.AutoSize = true;
            label5.Location = new Point(9, 6);
            label5.Name = "label5";
            label5.Size = new Size(77, 13);
            label5.TabIndex = 16;
            label5.Text = "Access Key ID";
            s3SecretAccessKey.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s3SecretAccessKey.Location = new Point(12, 71);
            s3SecretAccessKey.Name = "s3SecretAccessKey";
            s3SecretAccessKey.Size = new Size(355, 20);
            s3SecretAccessKey.TabIndex = 15;
            s3AccessKeyId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            s3AccessKeyId.Location = new Point(12, 22);
            s3AccessKeyId.Name = "s3AccessKeyId";
            s3AccessKeyId.Size = new Size(355, 20);
            s3AccessKeyId.TabIndex = 14;
            saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveButton.DialogResult = DialogResult.Yes;
            saveButton.Location = new Point(292, 123);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 23);
            saveButton.TabIndex = 18;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Location = new Point(211, 123);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 23);
            cancelButton.TabIndex = 18;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(379, 158);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(label2);
            Controls.Add(label5);
            Controls.Add(s3SecretAccessKey);
            Controls.Add(s3AccessKeyId);
            Name = "S3CredentialsForm";
            Text = "S3CredentialsForm";
            ResumeLayout(false);
            PerformLayout();
        }

        public S3CredentialsForm()
        {
            InitializeComponent();
        }

        internal void Initialize(S3Credentials s3c)
        {
            s3AccessKeyId.Text = s3c.accessKeyId;
            s3SecretAccessKey.Text = s3c.secretAccessKey;
        }

        internal void LoadResult(S3Credentials s3c)
        {
            s3c.accessKeyId = s3AccessKeyId.Text;
            s3c.secretAccessKey = s3SecretAccessKey.Text;
        }
    }
}
