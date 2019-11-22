using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderProgressPanel2 : UserControl, RenderUIIfc
    {
        public delegate void LaunchRenderedBrowserDelegate(Uri path);

        public delegate void RenderCompleteDelegate(Exception failure);

        public delegate void NotifyDelegate();

        private Mashup mashup;
        private MapTileSourceFactory mapTileSourceFactory;
        private LaunchRenderedBrowserDelegate launchRenderedBrowser;
        private RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage;
        private RenderState renderState;
        private RenderCompleteDelegate renderCompleteDelegate;
        private bool updateRequired;
        private ImageRef previewImage;
        private IContainer components;
        private Panel tileDisplayPanel;
        private TextBox currentTileName;
        private TextBox estimatedOutputSizeBox;
        private TextBox textBox1;
        private TextBox renderErrors;
        private LinkLabel previewRenderedResultsLinkLabel;
        private LinkLabel viewInBrowserLinkLabel;
        private Panel panel1;
        private ProgressBar renderProgressBar;
        private Button renderControlButton;
        private TextBox tileDisplayLabel;

        public RenderProgressPanel2()
        {
            InitializeComponent();
            previewRenderedResultsLinkLabel.Click += previewRenderedResultsLinkLabel_Click;
            VisibleChanged += RenderProgressPanel2_VisibleChanged;
        }

        private void RenderProgressPanel2_VisibleChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        public void Setup(Mashup mashup, MapTileSourceFactory mapTileSourceFactory,
            LaunchRenderedBrowserDelegate launchRenderedBrowser,
            RenderState.FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage)
        {
            this.flushRenderedTileCachePackage = flushRenderedTileCachePackage;
            ReplacePreviewImage(null);
            if (this.mashup != null)
            {
                this.mashup.dirtyEvent.Remove(MashupChangedHandler);
            }

            this.mashup = mashup;
            this.mapTileSourceFactory = mapTileSourceFactory;
            this.launchRenderedBrowser = launchRenderedBrowser;
            if (this.mashup != null)
            {
                this.mashup.dirtyEvent.Add(MashupChangedHandler);
            }

            MashupChangedHandler();
        }

        private void CheckForUpdate()
        {
            if (updateRequired && renderState != null)
            {
                updateRequired = false;
                renderState.UI_UpdateRenderControlButtonLabel(renderControlButton);
                estimatedOutputSizeBox.Text = renderState.UI_GetStatusString();
                renderErrors.Lines = renderState.UI_GetPostedMessages().ToArray();
                renderErrors.SelectionStart = renderErrors.Text.Length;
                renderErrors.SelectionLength = 0;
                renderErrors.ScrollToCaret();
                renderState.UI_UpdateProgress(renderProgressBar);
                renderState.UI_UpdateLinks(previewRenderedResultsLinkLabel, viewInBrowserLinkLabel);
                ReplacePreviewImage(renderState.UI_GetLastRenderedImageRef());
                tileDisplayLabel.Lines = renderState.UI_GetTileDisplayLabel();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (D.CustomPaintDisabled())
            {
                return;
            }

            CheckForUpdate();
            base.OnPaint(e);
        }

        private void MashupChangedHandler()
        {
            RenderState renderState = new RenderState(mashup,
                this,
                flushRenderedTileCachePackage,
                mapTileSourceFactory);
            if (!renderState.Equals(this.renderState))
            {
                if (this.renderState != null)
                {
                    this.renderState.Dispose();
                    this.renderState = null;
                }

                renderErrors.Text = "";
                this.renderState = renderState;
                D.Sayf(0, "RenderProgressPanel2: renderState replaced.", new object[0]);
                uiChanged();
                return;
            }

            renderState.Dispose();
        }

        private void ReplacePreviewImage(ImageRef newImage)
        {
            if (previewImage != null)
            {
                previewImage.Dispose();
            }

            previewImage = newImage;
        }

        private void renderControlButton_Click(object sender, EventArgs e)
        {
            renderState.RenderClick();
        }

        public void StartRender(RenderCompleteDelegate renderCompleteDelegate)
        {
            this.renderCompleteDelegate = renderCompleteDelegate;
            renderState.StartRender();
        }

        private void tileDisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Monitor.Enter(this);
            try
            {
                if (previewImage != null)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.LightPink),
                        new Rectangle(new Point(0, 0), tileDisplayPanel.Size));
                    try
                    {
                        GDIBigLockedImage image;
                        Monitor.Enter(image = previewImage.image);
                        try
                        {
                            Image image2 = previewImage.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                            e.Graphics.DrawImage(image2,
                                new Rectangle(new Point(0, 0), tileDisplayPanel.Size),
                                new Rectangle(0, 0, image2.Width, image2.Height),
                                GraphicsUnit.Pixel);
                        }
                        finally
                        {
                            Monitor.Exit(image);
                        }

                        goto IL_EB;
                    }
                    catch (Exception)
                    {
                        D.Say(0, "Absorbing that disturbing bug wherein the mostRecentTile image is corrupt.");
                        goto IL_EB;
                    }
                }

                e.Graphics.DrawRectangle(new Pen(Color.Black),
                    0,
                    0,
                    tileDisplayPanel.Size.Width - 1,
                    tileDisplayPanel.Height - 1);
                IL_EB: ;
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        private void previewRenderedResultsLinkLabel_Click(object sender, EventArgs e)
        {
            launchRenderedBrowser(renderState.GetRenderedXMLDescriptor());
        }

        private void viewInBrowserLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(renderState.GetSampleHTMLUri().ToString())
            {
                WindowStyle = ProcessWindowStyle.Normal
            });
        }

        internal void UndoConstruction()
        {
            if (mashup != null)
            {
                mashup.dirtyEvent.Remove(MashupChangedHandler);
                mashup = null;
            }

            renderState.Dispose();
            renderState = null;
        }

        public void uiChanged()
        {
            updateRequired = true;
            Invalidate();
            tileDisplayPanel.Invalidate();
        }

        public void notifyRenderComplete(Exception failure)
        {
            if (renderCompleteDelegate != null)
            {
                renderCompleteDelegate(failure);
                return;
            }

            if (failure == null)
            {
                NotifyDelegate method = ModalNotifyRenderComplete;
                Invoke(method);
            }
        }

        private void ModalNotifyRenderComplete()
        {
            MessageBox.Show(this, "Render completed successfully.", "Render complete");
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
            tileDisplayPanel = new Panel();
            currentTileName = new TextBox();
            estimatedOutputSizeBox = new TextBox();
            textBox1 = new TextBox();
            renderErrors = new TextBox();
            previewRenderedResultsLinkLabel = new LinkLabel();
            viewInBrowserLinkLabel = new LinkLabel();
            panel1 = new Panel();
            tileDisplayLabel = new TextBox();
            renderProgressBar = new ProgressBar();
            renderControlButton = new Button();
            panel1.SuspendLayout();
            SuspendLayout();
            tileDisplayPanel.BorderStyle = BorderStyle.FixedSingle;
            tileDisplayPanel.Location = new Point(7, 77);
            tileDisplayPanel.Name = "tileDisplayPanel";
            tileDisplayPanel.Size = new Size(256, 256);
            tileDisplayPanel.TabIndex = 31;
            tileDisplayPanel.Paint += tileDisplayPanel_Paint;
            currentTileName.BackColor = SystemColors.Control;
            currentTileName.BorderStyle = BorderStyle.None;
            currentTileName.Location = new Point(2, 284);
            currentTileName.Name = "currentTileName";
            currentTileName.ReadOnly = true;
            currentTileName.Size = new Size(261, 13);
            currentTileName.TabIndex = 36;
            currentTileName.TabStop = false;
            estimatedOutputSizeBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            estimatedOutputSizeBox.BackColor = SystemColors.Control;
            estimatedOutputSizeBox.BorderStyle = BorderStyle.None;
            estimatedOutputSizeBox.Location = new Point(156, 49);
            estimatedOutputSizeBox.Name = "estimatedOutputSizeBox";
            estimatedOutputSizeBox.ReadOnly = true;
            estimatedOutputSizeBox.Size = new Size(566, 13);
            estimatedOutputSizeBox.TabIndex = 38;
            textBox1.BackColor = SystemColors.Control;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Location = new Point(7, 49);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(143, 13);
            textBox1.TabIndex = 39;
            textBox1.TabStop = false;
            textBox1.Text = "Status";
            renderErrors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            renderErrors.Location = new Point(269, 77);
            renderErrors.Multiline = true;
            renderErrors.Name = "renderErrors";
            renderErrors.ReadOnly = true;
            renderErrors.ScrollBars = ScrollBars.Vertical;
            renderErrors.Size = new Size(453, 293);
            renderErrors.TabIndex = 40;
            previewRenderedResultsLinkLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            previewRenderedResultsLinkLabel.AutoSize = true;
            previewRenderedResultsLinkLabel.Font =
                new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
            previewRenderedResultsLinkLabel.Location = new Point(266, 377);
            previewRenderedResultsLinkLabel.Name = "previewRenderedResultsLinkLabel";
            previewRenderedResultsLinkLabel.Size = new Size(170, 18);
            previewRenderedResultsLinkLabel.TabIndex = 41;
            previewRenderedResultsLinkLabel.TabStop = true;
            previewRenderedResultsLinkLabel.Text = "Preview rendered results";
            viewInBrowserLinkLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            viewInBrowserLinkLabel.AutoSize = true;
            viewInBrowserLinkLabel.Font =
                new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
            viewInBrowserLinkLabel.Location = new Point(442, 377);
            viewInBrowserLinkLabel.Name = "viewInBrowserLinkLabel";
            viewInBrowserLinkLabel.Size = new Size(160, 18);
            viewInBrowserLinkLabel.TabIndex = 41;
            viewInBrowserLinkLabel.TabStop = true;
            viewInBrowserLinkLabel.Text = "View results in browser";
            viewInBrowserLinkLabel.LinkClicked +=
                viewInBrowserLinkLabel_LinkClicked;
            panel1.Controls.Add(tileDisplayLabel);
            panel1.Controls.Add(renderErrors);
            panel1.Controls.Add(renderProgressBar);
            panel1.Controls.Add(viewInBrowserLinkLabel);
            panel1.Controls.Add(previewRenderedResultsLinkLabel);
            panel1.Controls.Add(estimatedOutputSizeBox);
            panel1.Controls.Add(tileDisplayPanel);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(renderControlButton);
            panel1.Controls.Add(currentTileName);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(725, 407);
            panel1.TabIndex = 42;
            tileDisplayLabel.BackColor = SystemColors.Control;
            tileDisplayLabel.BorderStyle = BorderStyle.None;
            tileDisplayLabel.Location = new Point(7, 361);
            tileDisplayLabel.Multiline = true;
            tileDisplayLabel.Name = "tileDisplayLabel";
            tileDisplayLabel.ReadOnly = true;
            tileDisplayLabel.Size = new Size(256, 66);
            tileDisplayLabel.TabIndex = 43;
            renderProgressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            renderProgressBar.Location = new Point(156, 20);
            renderProgressBar.Name = "estimateProgressBar";
            renderProgressBar.Size = new Size(566, 23);
            renderProgressBar.TabIndex = 42;
            renderControlButton.Location = new Point(6, 20);
            renderControlButton.Name = "estimateControlButton";
            renderControlButton.Size = new Size(144, 23);
            renderControlButton.TabIndex = 32;
            renderControlButton.Text = "Start";
            renderControlButton.UseVisualStyleBackColor = true;
            renderControlButton.Click += renderControlButton_Click;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(panel1);
            Name = "RenderProgressPanel2";
            Size = new Size(725, 407);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }
    }
}
