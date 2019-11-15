namespace MSR.CVE.BackMaker
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.aboutContentsBrowser = new System.Windows.Forms.WebBrowser();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // aboutContentsBrowser
            // 
            this.aboutContentsBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.aboutContentsBrowser.Location = new System.Drawing.Point(8, 104);
            this.aboutContentsBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.aboutContentsBrowser.Name = "aboutContentsBrowser";
            this.aboutContentsBrowser.Size = new System.Drawing.Size(558, 439);
            this.aboutContentsBrowser.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Image = global::MapCruncher.Properties.Resources.image;
            this.label2.Location = new System.Drawing.Point(397, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(148, 139);
            this.label2.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Image = global::MapCruncher.Properties.Resources.label;
            this.label3.Location = new System.Drawing.Point(33, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(340, 92);
            this.label3.TabIndex = 5;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(555, 444);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.aboutContentsBrowser);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About MapCruncher";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser aboutContentsBrowser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}