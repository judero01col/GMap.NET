using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class RecoverAutoSavedFileDialog : Form
    {
        private IContainer components;
        private TextBox message;
        private Button openAsNewButton;
        private Button deleteBackupButton;
        private Button cancelButton;
        private NotifyIcon notifyIcon1;

        public RecoverAutoSavedFileDialog()
        {
            InitializeComponent();
        }

        public void Initialize(string filename)
        {
            List<string> list = new List<string>();
            list.Add(string.Format("An automatically-saved backup for {0} was found.", filename));
            list.Add("You may:");
            list.Add("    open it as a new mashup,");
            list.Add("    delete it and continue opening the original file, or");
            list.Add("    cancel the open operation.");
            message.Lines = list.ToArray();
            message.SelectionStart = 0;
            message.SelectionLength = 0;
        }

        private void openAsNewButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void deleteBackupButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Ignore;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
            components = new Container();
            message = new TextBox();
            openAsNewButton = new Button();
            deleteBackupButton = new Button();
            cancelButton = new Button();
            notifyIcon1 = new NotifyIcon(components);
            SuspendLayout();
            message.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            message.BorderStyle = BorderStyle.None;
            message.Font = new Font("Microsoft Sans Serif", 9f, FontStyle.Regular, GraphicsUnit.Point, 0);
            message.Location = new Point(8, 7);
            message.Multiline = true;
            message.Name = "message";
            message.ReadOnly = true;
            message.Size = new Size(376, 173);
            message.TabIndex = 0;
            message.Text = "Example text";
            openAsNewButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            openAsNewButton.Location = new Point(20, 218);
            openAsNewButton.Name = "openAsNewButton";
            openAsNewButton.Size = new Size(130, 29);
            openAsNewButton.TabIndex = 1;
            openAsNewButton.Text = "Open Backup as New";
            openAsNewButton.UseVisualStyleBackColor = true;
            openAsNewButton.Click += openAsNewButton_Click;
            deleteBackupButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            deleteBackupButton.Location = new Point(156, 218);
            deleteBackupButton.Name = "deleteBackupButton";
            deleteBackupButton.Size = new Size(107, 29);
            deleteBackupButton.TabIndex = 1;
            deleteBackupButton.Text = "Delete Backup";
            deleteBackupButton.UseVisualStyleBackColor = true;
            deleteBackupButton.Click += deleteBackupButton_Click;
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            cancelButton.Location = new Point(269, 218);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(107, 29);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            notifyIcon1.Text = "notifyIcon1";
            notifyIcon1.Visible = true;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(391, 266);
            Controls.Add(cancelButton);
            Controls.Add(deleteBackupButton);
            Controls.Add(openAsNewButton);
            Controls.Add(message);
            Name = "RecoverAutoSavedFileDialog";
            Text = "Recover automatically-saved backup file";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
