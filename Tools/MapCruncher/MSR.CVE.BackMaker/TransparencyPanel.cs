using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class TransparencyPanel : UserControl
    {
        private IContainer components;
        private Button addTransparencyButton;
        private DataGridView colorGrid;
        private Button removeTransparencyButton;
        private RadioButton normalTransparencyButton;
        private RadioButton invertedTransparencyButton;
        private RadioButton noTransparencyButton;
        private NumericUpDown fuzzSpinner;
        private NumericUpDown haloSpinner;
        private Label label1;
        private Label label2;
        private DataGridViewImageColumn color;
        private DataGridViewTextBoxColumn Epsilon;
        private DataGridViewTextBoxColumn HaloWidth;
        private CheckBox useDocumentTransparencyCheckbox;
        private SourceMap sourceMap;
        private TransparencyIfc transparencyIfc;
        private bool needUpdate;
        private bool disableSpinnerUpdate;
        private bool suspendDocUpdate;

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
            DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            addTransparencyButton = new Button();
            colorGrid = new DataGridView();
            color = new DataGridViewImageColumn();
            Epsilon = new DataGridViewTextBoxColumn();
            HaloWidth = new DataGridViewTextBoxColumn();
            removeTransparencyButton = new Button();
            normalTransparencyButton = new RadioButton();
            invertedTransparencyButton = new RadioButton();
            noTransparencyButton = new RadioButton();
            fuzzSpinner = new NumericUpDown();
            haloSpinner = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            useDocumentTransparencyCheckbox = new CheckBox();
            ((ISupportInitialize)colorGrid).BeginInit();
            ((ISupportInitialize)fuzzSpinner).BeginInit();
            ((ISupportInitialize)haloSpinner).BeginInit();
            SuspendLayout();
            addTransparencyButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addTransparencyButton.Location = new Point(3, 95);
            addTransparencyButton.Name = "addTransparencyButton";
            addTransparencyButton.Size = new Size(210, 23);
            addTransparencyButton.TabIndex = 0;
            addTransparencyButton.Text = "Add Color Under Crosshairs";
            addTransparencyButton.UseVisualStyleBackColor = true;
            addTransparencyButton.Click += addTransparencyButton_Click;
            colorGrid.AllowUserToAddRows = false;
            colorGrid.AllowUserToDeleteRows = false;
            colorGrid.AllowUserToResizeRows = false;
            colorGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle.BackColor = SystemColors.Control;
            dataGridViewCellStyle.Font =
                new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = DataGridViewTriState.True;
            colorGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
            colorGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            colorGrid.Columns.AddRange(new DataGridViewColumn[] {color, Epsilon, HaloWidth});
            colorGrid.Location = new Point(3, 124);
            colorGrid.MultiSelect = false;
            colorGrid.Name = "colorGrid";
            colorGrid.ReadOnly = true;
            colorGrid.RowHeadersVisible = false;
            colorGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            colorGrid.Size = new Size(213, 158);
            colorGrid.TabIndex = 1;
            colorGrid.SelectionChanged += pinList_SelectedIndexChanged;
            color.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            color.HeaderText = "Color";
            color.Name = "color";
            color.ReadOnly = true;
            color.Resizable = DataGridViewTriState.True;
            color.SortMode = DataGridViewColumnSortMode.Automatic;
            Epsilon.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            Epsilon.DefaultCellStyle = dataGridViewCellStyle2;
            Epsilon.HeaderText = "Fuzziness (+/--)";
            Epsilon.Name = "Epsilon";
            Epsilon.ReadOnly = true;
            HaloWidth.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleCenter;
            HaloWidth.DefaultCellStyle = dataGridViewCellStyle3;
            HaloWidth.HeaderText = "Halo";
            HaloWidth.Name = "HaloWidth";
            HaloWidth.ReadOnly = true;
            removeTransparencyButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            removeTransparencyButton.Location = new Point(3, 288);
            removeTransparencyButton.Name = "removeTransparencyButton";
            removeTransparencyButton.Size = new Size(213, 23);
            removeTransparencyButton.TabIndex = 2;
            removeTransparencyButton.Text = "Remove Selected Color";
            removeTransparencyButton.UseVisualStyleBackColor = true;
            removeTransparencyButton.Click += removeTransparencyButton_Click;
            normalTransparencyButton.AutoSize = true;
            normalTransparencyButton.Checked = true;
            normalTransparencyButton.Location = new Point(3, 31);
            normalTransparencyButton.Name = "normalTransparencyButton";
            normalTransparencyButton.Size = new Size(182, 17);
            normalTransparencyButton.TabIndex = 3;
            normalTransparencyButton.TabStop = true;
            normalTransparencyButton.Text = "Make selected colors transparent";
            normalTransparencyButton.UseVisualStyleBackColor = true;
            normalTransparencyButton.CheckedChanged +=
                normalTransparencyButton_CheckedChanged;
            invertedTransparencyButton.AutoSize = true;
            invertedTransparencyButton.Location = new Point(3, 55);
            invertedTransparencyButton.Name = "invertedTransparencyButton";
            invertedTransparencyButton.Size = new Size(155, 17);
            invertedTransparencyButton.TabIndex = 4;
            invertedTransparencyButton.TabStop = true;
            invertedTransparencyButton.Text = "Display only selected colors";
            invertedTransparencyButton.UseVisualStyleBackColor = true;
            invertedTransparencyButton.CheckedChanged +=
                invertedTransparencyButton_CheckedChanged;
            noTransparencyButton.AutoSize = true;
            noTransparencyButton.Location = new Point(3, 78);
            noTransparencyButton.Name = "noTransparencyButton";
            noTransparencyButton.Size = new Size(124, 17);
            noTransparencyButton.TabIndex = 5;
            noTransparencyButton.TabStop = true;
            noTransparencyButton.Text = "Disable transparency";
            noTransparencyButton.UseVisualStyleBackColor = true;
            noTransparencyButton.CheckedChanged += noTransparencyButton_CheckedChanged;
            fuzzSpinner.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            fuzzSpinner.Location = new Point(62, 321);
            fuzzSpinner.Name = "fuzzSpinner";
            fuzzSpinner.Size = new Size(42, 20);
            fuzzSpinner.TabIndex = 6;
            fuzzSpinner.ValueChanged += exactnessSpinner_ValueChanged;
            haloSpinner.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            haloSpinner.Location = new Point(165, 321);
            haloSpinner.Name = "haloSpinner";
            haloSpinner.Size = new Size(48, 20);
            haloSpinner.TabIndex = 7;
            haloSpinner.ValueChanged += haloSpinner_ValueChanged;
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(0, 323);
            label1.Name = "label1";
            label1.Size = new Size(53, 13);
            label1.TabIndex = 8;
            label1.Text = "Fuzziness";
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(129, 323);
            label2.Name = "label2";
            label2.Size = new Size(29, 13);
            label2.TabIndex = 9;
            label2.Text = "Halo";
            useDocumentTransparencyCheckbox.AutoSize = true;
            useDocumentTransparencyCheckbox.Location = new Point(3, 8);
            useDocumentTransparencyCheckbox.Name = "useDocumentTransparencyCheckbox";
            useDocumentTransparencyCheckbox.Size = new Size(159, 17);
            useDocumentTransparencyCheckbox.TabIndex = 10;
            useDocumentTransparencyCheckbox.Text = "Use document transparency";
            useDocumentTransparencyCheckbox.UseVisualStyleBackColor = true;
            useDocumentTransparencyCheckbox.CheckedChanged +=
                useDocumentTransparencyCheckbox_CheckedChanged;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(useDocumentTransparencyCheckbox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(haloSpinner);
            Controls.Add(fuzzSpinner);
            Controls.Add(noTransparencyButton);
            Controls.Add(invertedTransparencyButton);
            Controls.Add(normalTransparencyButton);
            Controls.Add(removeTransparencyButton);
            Controls.Add(colorGrid);
            Controls.Add(addTransparencyButton);
            Name = "TransparencyPanel";
            Size = new Size(216, 345);
            ((ISupportInitialize)colorGrid).EndInit();
            ((ISupportInitialize)fuzzSpinner).EndInit();
            ((ISupportInitialize)haloSpinner).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        public TransparencyPanel()
        {
            InitializeComponent();
            haloSpinner.Minimum = TransparencyOptions.HaloSizeRange.min;
            haloSpinner.Maximum = TransparencyOptions.HaloSizeRange.max;
            fuzzSpinner.Minimum = TransparencyOptions.FuzzRange.min;
            fuzzSpinner.Maximum = TransparencyOptions.FuzzRange.max;
            disableSpinnerUpdate = false;
        }

        public void Configure(SourceMap sourceMap, TransparencyIfc transparencyIfc)
        {
            this.transparencyIfc = transparencyIfc;
            if (this.sourceMap != null)
            {
                this.sourceMap.transparencyOptions.transparencyOptionsChangedEvent -=
                    TransparencyChangedHandler;
            }

            this.sourceMap = sourceMap;
            if (this.sourceMap != null)
            {
                this.sourceMap.transparencyOptions.transparencyOptionsChangedEvent +=
                    TransparencyChangedHandler;
            }

            update();
        }

        private void TransparencyChangedHandler()
        {
            needUpdate = true;
            Invalidate();
            if (transparencyIfc != null)
            {
                transparencyIfc.InvalidatePipeline();
            }
        }

        public void SetSelected(TransparencyColor tc)
        {
            try
            {
                if (tc != null)
                {
                    foreach (DataGridViewRow dataGridViewRow in colorGrid.Rows)
                    {
                        if (((TransparencyColor)dataGridViewRow.Tag).color == tc.color)
                        {
                            dataGridViewRow.Selected = true;
                            colorGrid.CurrentCell = dataGridViewRow.Cells[0];
                            pinList_SelectedIndexChanged(null, null);
                            return;
                        }
                    }

                    UnselectAll();
                }
            }
            catch (Exception ex)
            {
                D.Sayf(0, "the bad thing happened: {0}", new object[] {ex.Message});
            }
        }

        private void UnselectAll()
        {
            foreach (DataGridViewRow dataGridViewRow in colorGrid.SelectedRows)
            {
                dataGridViewRow.Selected = false;
            }

            pinList_SelectedIndexChanged(null, null);
        }

        public TransparencyColor GetSelected()
        {
            foreach (DataGridViewRow dataGridViewRow in colorGrid.Rows)
            {
                if (dataGridViewRow.Selected)
                {
                    return (TransparencyColor)dataGridViewRow.Tag;
                }
            }

            return null;
        }

        private void update()
        {
            if (sourceMap != null)
            {
                useDocumentTransparencyCheckbox.Checked =
                    sourceMap.transparencyOptions.useDocumentTransparency;
                TransparencyOptions.TransparencyMode mode = sourceMap.transparencyOptions.GetMode();
                suspendDocUpdate = true;
                if (mode == TransparencyOptions.TransparencyMode.Off)
                {
                    noTransparencyButton.Checked = true;
                }
                else
                {
                    if (mode == TransparencyOptions.TransparencyMode.Inverted)
                    {
                        invertedTransparencyButton.Checked = true;
                    }
                    else
                    {
                        if (mode == TransparencyOptions.TransparencyMode.Normal)
                        {
                            normalTransparencyButton.Checked = true;
                        }
                    }
                }

                suspendDocUpdate = false;
            }

            disableSpinnerUpdate = true;
            TransparencyColor selected = GetSelected();
            colorGrid.Rows.Clear();
            if (sourceMap != null)
            {
                foreach (TransparencyColor current in sourceMap.transparencyOptions.colorList)
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    Bitmap bitmap = new Bitmap(40, 12);
                    Graphics graphics = Graphics.FromImage(bitmap);
                    graphics.FillRectangle(new SolidBrush(current.color.ToColor()),
                        new Rectangle(new Point(0, 0), bitmap.Size));
                    dataGridViewRow.CreateCells(colorGrid,
                        new object[] {bitmap, current.fuzz.ToString(), current.halo.ToString()});
                    dataGridViewRow.Tag = current;
                    colorGrid.Rows.Add(dataGridViewRow);
                }
            }

            disableSpinnerUpdate = false;
            SetSelected(selected);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (D.CustomPaintDisabled())
            {
                return;
            }

            if (needUpdate)
            {
                needUpdate = false;
                update();
            }

            base.OnPaint(e);
        }

        private void addTransparencyButton_Click(object sender, EventArgs e)
        {
            if (sourceMap != null && transparencyIfc != null)
            {
                Pixel baseLayerCenterPixel = transparencyIfc.GetBaseLayerCenterPixel();
                if (baseLayerCenterPixel is UndefinedPixel)
                {
                    return;
                }

                foreach (TransparencyColor current in sourceMap.transparencyOptions.colorList)
                {
                    if (current.color == baseLayerCenterPixel)
                    {
                        SetSelected(current);
                        return;
                    }
                }

                TransparencyColor selected = sourceMap.transparencyOptions.AddColor(baseLayerCenterPixel);
                update();
                SetSelected(selected);
            }
        }

        private void removeTransparencyButton_Click(object sender, EventArgs e)
        {
            if (sourceMap != null && colorGrid.SelectedRows.Count >= 1)
            {
                sourceMap.transparencyOptions.RemoveColor((TransparencyColor)colorGrid.SelectedRows[0].Tag);
            }
        }

        private void normalTransparencyButton_CheckedChanged(object sender, EventArgs e)
        {
            if (suspendDocUpdate)
            {
                return;
            }

            if (sourceMap != null)
            {
                sourceMap.transparencyOptions.SetNormalTransparency();
            }
        }

        private void invertedTransparencyButton_CheckedChanged(object sender, EventArgs e)
        {
            if (suspendDocUpdate)
            {
                return;
            }

            if (sourceMap != null)
            {
                sourceMap.transparencyOptions.SetInvertedTransparency();
            }
        }

        private void noTransparencyButton_CheckedChanged(object sender, EventArgs e)
        {
            if (suspendDocUpdate)
            {
                return;
            }

            if (sourceMap != null)
            {
                sourceMap.transparencyOptions.SetDisabledTransparency();
            }
        }

        private void exactnessSpinner_ValueChanged(object sender, EventArgs e)
        {
            if (sourceMap != null && colorGrid.SelectedRows.Count >= 1)
            {
                sourceMap.transparencyOptions.SetFuzz((TransparencyColor)colorGrid.SelectedRows[0].Tag,
                    (int)fuzzSpinner.Value);
            }
        }

        private void haloSpinner_ValueChanged(object sender, EventArgs e)
        {
            if (sourceMap != null && colorGrid.SelectedRows.Count >= 1)
            {
                sourceMap.transparencyOptions.SetHalo((TransparencyColor)colorGrid.SelectedRows[0].Tag,
                    (int)haloSpinner.Value);
            }
        }

        private void pinList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (disableSpinnerUpdate)
            {
                return;
            }

            TransparencyColor selected = GetSelected();
            if (selected == null)
            {
                haloSpinner.Enabled = false;
                fuzzSpinner.Enabled = false;
                removeTransparencyButton.Enabled = false;
                return;
            }

            if (haloSpinner.Value != selected.halo)
            {
                haloSpinner.Value = selected.halo;
            }

            if (fuzzSpinner.Value != selected.fuzz)
            {
                fuzzSpinner.Value = selected.fuzz;
            }

            haloSpinner.Enabled = true;
            fuzzSpinner.Enabled = true;
            removeTransparencyButton.Enabled = true;
        }

        private void useDocumentTransparencyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            sourceMap.transparencyOptions.useDocumentTransparency = useDocumentTransparencyCheckbox.Checked;
        }
    }
}
