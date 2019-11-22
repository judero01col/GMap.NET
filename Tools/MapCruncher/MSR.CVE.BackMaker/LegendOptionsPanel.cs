using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LegendOptionsPanel : UserControl
    {
        internal class CallbackIgnorinator
        {
            private LegendOptionsPanel lop;

            public CallbackIgnorinator(LegendOptionsPanel lop)
            {
                this.lop = lop;
            }

            public void Callback(AsyncRef asyncRef)
            {
                lop.AsyncReadyCallback(this);
            }
        }

        private IContainer components;
        private Label label1;
        private NumericUpDown renderedSizeSpinner;
        private Panel previewPanel;
        private Legend _legend;
        private IDisplayableSource displayableSource;
        private ImageRef previewImage;
        private IFuture previewFuture;
        private InterestList previewInterest;
        private CallbackIgnorinator waitingForCI;
        private bool needUpdate;

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
            renderedSizeSpinner = new NumericUpDown();
            previewPanel = new Panel();
            ((ISupportInitialize)renderedSizeSpinner).BeginInit();
            SuspendLayout();
            label1.AutoSize = true;
            label1.Location = new Point(7, 18);
            label1.Name = "label1";
            label1.Size = new Size(75, 13);
            label1.TabIndex = 0;
            label1.Text = "Rendered size";
            renderedSizeSpinner.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NumericUpDown arg_B6_0 = renderedSizeSpinner;
            int[] array = new int[4];
            array[0] = 50;
            arg_B6_0.Increment = new decimal(array);
            renderedSizeSpinner.Location = new Point(165, 16);
            NumericUpDown arg_ED_0 = renderedSizeSpinner;
            int[] array2 = new int[4];
            array2[0] = 1000;
            arg_ED_0.Maximum = new decimal(array2);
            NumericUpDown arg_10A_0 = renderedSizeSpinner;
            int[] array3 = new int[4];
            array3[0] = 50;
            arg_10A_0.Minimum = new decimal(array3);
            renderedSizeSpinner.Name = "renderedSizeSpinner";
            renderedSizeSpinner.Size = new Size(73, 20);
            renderedSizeSpinner.TabIndex = 1;
            renderedSizeSpinner.TextAlign = HorizontalAlignment.Right;
            NumericUpDown arg_163_0 = renderedSizeSpinner;
            int[] array4 = new int[4];
            array4[0] = 50;
            arg_163_0.Value = new decimal(array4);
            previewPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            previewPanel.Location = new Point(10, 50);
            previewPanel.Name = "previewPanel";
            previewPanel.Size = new Size(228, 235);
            previewPanel.TabIndex = 2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(previewPanel);
            Controls.Add(renderedSizeSpinner);
            Controls.Add(label1);
            Name = "LegendOptionsPanel";
            Size = new Size(250, 296);
            ((ISupportInitialize)renderedSizeSpinner).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        public LegendOptionsPanel()
        {
            InitializeComponent();
            renderedSizeSpinner.ValueChanged += renderedSizeSpinner_ValueChanged;
            previewPanel.Paint += previewPanel_Paint;
            renderedSizeSpinner.Minimum = Legend.renderedSizeRange.min;
            renderedSizeSpinner.Maximum = Legend.renderedSizeRange.max;
        }

        public void Configure(Legend legend, IDisplayableSource displayableSource)
        {
            Monitor.Enter(this);
            try
            {
                if (previewInterest != null)
                {
                    previewInterest.Dispose();
                    previewInterest = null;
                    waitingForCI = null;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (_legend == legend)
            {
                return;
            }

            if (_legend != null)
            {
                _legend.dirtyEvent.Remove(LegendChanged);
            }

            _legend = legend;
            this.displayableSource = displayableSource;
            if (_legend != null)
            {
                _legend.dirtyEvent.Add(LegendChanged);
                LegendChanged();
            }

            Enabled = _legend != null;
            UpdatePreviewImage(null);
            UpdatePreviewPanel();
        }

        private void UpdatePreviewImage(ImageRef imageRef)
        {
            Monitor.Enter(this);
            try
            {
                if (previewImage != null)
                {
                    previewImage.Dispose();
                    previewImage = null;
                }

                if (imageRef != null)
                {
                    previewImage = (ImageRef)imageRef.Duplicate("LegendOptionsPanel.UpdatePreviewImage");
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            Invalidate();
        }

        private void LegendChanged()
        {
            renderedSizeSpinner.Value = _legend.renderedSize;
            UpdatePreviewPanel();
        }

        private void renderedSizeSpinner_ValueChanged(object sender, EventArgs e)
        {
            _legend.renderedSize = (int)renderedSizeSpinner.Value;
        }

        private void UpdatePreviewPanel()
        {
            needUpdate = true;
            Invalidate();
        }

        private void HandleUpdate()
        {
            if (!needUpdate)
            {
                return;
            }

            needUpdate = false;
            Monitor.Enter(this);
            try
            {
                if (previewInterest != null)
                {
                    previewInterest.Dispose();
                    previewInterest = null;
                    waitingForCI = null;
                }

                if (_legend != null)
                {
                    try
                    {
                        IFuture renderedLegendFuture =
                            _legend.GetRenderedLegendFuture(displayableSource, (FutureFeatures)5);
                        if (previewFuture != renderedLegendFuture)
                        {
                            previewFuture = renderedLegendFuture;
                            AsyncRef asyncRef =
                                (AsyncRef)renderedLegendFuture.Realize("LegendOptionsPanel.UpdatePreviewPanel");
                            if (asyncRef.present == null)
                            {
                                waitingForCI = new CallbackIgnorinator(this);
                                asyncRef.AddCallback(waitingForCI.Callback);
                                asyncRef.SetInterest(524296);
                                previewInterest = new InterestList();
                                previewInterest.Add(asyncRef);
                                previewInterest.Activate();
                                UpdatePreviewImage(null);
                            }
                            else
                            {
                                if (asyncRef.present is ImageRef)
                                {
                                    UpdatePreviewImage((ImageRef)asyncRef.present);
                                }
                            }
                        }
                    }
                    catch (Legend.RenderFailedException)
                    {
                    }
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        internal void AsyncReadyCallback(CallbackIgnorinator ci)
        {
            if (ci == waitingForCI)
            {
                UpdatePreviewPanel();
            }
        }

        private void previewPanel_Paint(object sender, PaintEventArgs e)
        {
            HandleUpdate();
            ImageRef imageRef = null;
            Monitor.Enter(this);
            try
            {
                if (previewImage != null)
                {
                    imageRef = (ImageRef)previewImage.Duplicate("LegendOptionsPanel.previewPanel_Paint");
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (imageRef != null)
            {
                try
                {
                    GDIBigLockedImage image;
                    Monitor.Enter(image = imageRef.image);
                    try
                    {
                        Image image2 = imageRef.image.IPromiseIAmHoldingGDISLockSoPleaseGiveMeTheImage();
                        e.Graphics.DrawImage(image2,
                            new Rectangle(new Point(0, 0), previewPanel.Size),
                            new Rectangle(new Point(0, 0), previewPanel.Size),
                            GraphicsUnit.Pixel);
                    }
                    finally
                    {
                        Monitor.Exit(image);
                    }
                }
                catch (Exception)
                {
                    D.Say(0, "Absorbing that disturbing bug wherein the mostRecentTile image is corrupt.");
                }

                imageRef.Dispose();
                return;
            }

            e.Graphics.DrawRectangle(new Pen(Color.Black),
                0,
                0,
                previewPanel.Size.Width - 1,
                previewPanel.Height - 1);
        }
    }
}
