using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class registrationControls : UserControl, InvalidatableViewIfc
    {
        private IContainer components;
        private Button removeAllPushpinsButton;
        private Button removePushPinButton;
        private Button unlockTransformButton;
        private Button lockTransformButton;
        private Button addPushPinButton;
        private TextBox lockStatusText;
        private DataGridView pinList;
        private Button updatePushPinButton;
        private TableLayoutPanel LockButtonTable;
        private TableLayoutPanel pinNameTable;
        private TextBox pinText;
        private CheckBox forceAffineCheckBox;
        private ToolTip toolTip;
        private GroupBox getStartedBox;
        private TextBox textBox;
        private Panel panel1;
        private DataGridViewTextBoxColumn pinIDcolumn;
        private DataGridViewTextBoxColumn pinNameColumn;
        private DataGridViewTextBoxColumn LocationColumn;
        private DataGridViewTextBoxColumn Error;
        private AssociationIfc associationIfc;
        private RegistrationControlRecord registrationControl;
        private DegreesMinutesSeconds dms = new DegreesMinutesSeconds();
        private MapDrawingOption _ShowDMS;

        public MapDrawingOption ShowDMS
        {
            set
            {
                _ShowDMS = value;
                _ShowDMS.SetInvalidatableView(this);
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
            components = new Container();
            getStartedBox = new GroupBox();
            textBox = new TextBox();
            pinNameTable = new TableLayoutPanel();
            pinText = new TextBox();
            addPushPinButton = new Button();
            updatePushPinButton = new Button();
            LockButtonTable = new TableLayoutPanel();
            removePushPinButton = new Button();
            removeAllPushpinsButton = new Button();
            unlockTransformButton = new Button();
            lockTransformButton = new Button();
            pinList = new DataGridView();
            pinIDcolumn = new DataGridViewTextBoxColumn();
            pinNameColumn = new DataGridViewTextBoxColumn();
            LocationColumn = new DataGridViewTextBoxColumn();
            Error = new DataGridViewTextBoxColumn();
            lockStatusText = new TextBox();
            forceAffineCheckBox = new CheckBox();
            toolTip = new ToolTip(components);
            panel1 = new Panel();
            getStartedBox.SuspendLayout();
            pinNameTable.SuspendLayout();
            LockButtonTable.SuspendLayout();
            ((ISupportInitialize)pinList).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            getStartedBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            getStartedBox.BackColor = SystemColors.ControlLightLight;
            getStartedBox.Controls.Add(textBox);
            getStartedBox.Location = new Point(9, 61);
            getStartedBox.Name = "getStartedBox";
            getStartedBox.Size = new Size(196, 95);
            getStartedBox.TabIndex = 8;
            getStartedBox.TabStop = false;
            textBox.BorderStyle = BorderStyle.None;
            textBox.Dock = DockStyle.Fill;
            textBox.Font = new Font("Microsoft Sans Serif", 11f, FontStyle.Bold, GraphicsUnit.Point, 0);
            textBox.ForeColor = Color.Red;
            textBox.Location = new Point(3, 16);
            textBox.Multiline = true;
            textBox.Name = "textBox";
            textBox.Size = new Size(190, 76);
            textBox.TabIndex = 0;
            textBox.Text = "Place corresponding points under crosshairs and click Add.";
            pinNameTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pinNameTable.ColumnCount = 4;
            pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pinNameTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            pinNameTable.Controls.Add(pinText, 0, 0);
            pinNameTable.Controls.Add(addPushPinButton, 2, 0);
            pinNameTable.Controls.Add(updatePushPinButton, 3, 0);
            pinNameTable.Location = new Point(2, 3);
            pinNameTable.Name = "pinNameTable";
            pinNameTable.RowCount = 1;
            pinNameTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            pinNameTable.Size = new Size(223, 24);
            pinNameTable.TabIndex = 10;
            pinText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pinNameTable.SetColumnSpan(pinText, 2);
            pinText.Location = new Point(0, 0);
            pinText.Margin = new Padding(0);
            pinText.Name = "pinText";
            pinText.Size = new Size(110, 20);
            pinText.TabIndex = 2;
            addPushPinButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addPushPinButton.Location = new Point(113, 0);
            addPushPinButton.Margin = new Padding(3, 0, 0, 0);
            addPushPinButton.Name = "addPushPinButton";
            addPushPinButton.Size = new Size(52, 23);
            addPushPinButton.TabIndex = 0;
            addPushPinButton.Text = "Add";
            toolTip.SetToolTip(addPushPinButton,
                "To create a registration point, position the crosshairs over corresponding points on both maps.  Then click Add.");
            addPushPinButton.Click += addPushPinButton_Click;
            updatePushPinButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            updatePushPinButton.Location = new Point(168, 0);
            updatePushPinButton.Margin = new Padding(3, 0, 0, 0);
            updatePushPinButton.Name = "updatePushPinButton";
            updatePushPinButton.Size = new Size(55, 23);
            updatePushPinButton.TabIndex = 8;
            updatePushPinButton.Text = "Update";
            toolTip.SetToolTip(updatePushPinButton,
                "To move an existing point, highlight it on the list below.  Then reposition the crosshairs and click update.");
            updatePushPinButton.Click += updatePushPinButton_Click;
            LockButtonTable.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            LockButtonTable.ColumnCount = 2;
            LockButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            LockButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            LockButtonTable.Controls.Add(removePushPinButton, 0, 0);
            LockButtonTable.Controls.Add(removeAllPushpinsButton, 1, 0);
            LockButtonTable.Controls.Add(unlockTransformButton, 0, 1);
            LockButtonTable.Controls.Add(lockTransformButton, 1, 1);
            LockButtonTable.Location = new Point(69, 293);
            LockButtonTable.Name = "LockButtonTable";
            LockButtonTable.RowCount = 2;
            LockButtonTable.RowStyles.Add(new RowStyle());
            LockButtonTable.RowStyles.Add(new RowStyle());
            LockButtonTable.Size = new Size(156, 59);
            LockButtonTable.TabIndex = 9;
            removePushPinButton.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            removePushPinButton.Enabled = false;
            removePushPinButton.Location = new Point(3, 3);
            removePushPinButton.Name = "removePushPinButton";
            removePushPinButton.Size = new Size(72, 23);
            removePushPinButton.TabIndex = 1;
            removePushPinButton.Text = "Remove";
            toolTip.SetToolTip(removePushPinButton, "Removes the highlighted correspondence point.");
            removePushPinButton.Click += removePushPinButton_Click;
            removeAllPushpinsButton.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            removeAllPushpinsButton.Enabled = false;
            removeAllPushpinsButton.Location = new Point(81, 3);
            removeAllPushpinsButton.Name = "removeAllPushpinsButton";
            removeAllPushpinsButton.Size = new Size(72, 23);
            removeAllPushpinsButton.TabIndex = 4;
            removeAllPushpinsButton.Text = "Remove All";
            toolTip.SetToolTip(removeAllPushpinsButton, "Removes all correspondence points.");
            removeAllPushpinsButton.Click += removeAllPushpinsButton_Click;
            unlockTransformButton.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            unlockTransformButton.Location = new Point(3, 32);
            unlockTransformButton.Name = "unlockTransformButton";
            unlockTransformButton.Size = new Size(72, 24);
            unlockTransformButton.TabIndex = 0;
            unlockTransformButton.Text = "Unlock";
            toolTip.SetToolTip(unlockTransformButton,
                "Unlocks the source map from Virtual Earth, allowing additional points to be added.");
            unlockTransformButton.Click += unlockTransformButton_Click;
            lockTransformButton.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lockTransformButton.Location = new Point(81, 32);
            lockTransformButton.Name = "lockTransformButton";
            lockTransformButton.Size = new Size(72, 24);
            lockTransformButton.TabIndex = 0;
            lockTransformButton.Text = "Lock";
            toolTip.SetToolTip(lockTransformButton,
                "Warps the source map to fit Virtual Earth using the existing correspondence points.");
            lockTransformButton.Click += lockTransformButton_Click;
            pinList.AllowUserToAddRows = false;
            pinList.AllowUserToDeleteRows = false;
            pinList.AllowUserToOrderColumns = true;
            pinList.AllowUserToResizeRows = false;
            pinList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pinList.BackgroundColor = SystemColors.ButtonHighlight;
            pinList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            pinList.Columns.AddRange(new DataGridViewColumn[]
            {
                pinIDcolumn, pinNameColumn, LocationColumn, Error
            });
            pinList.GridColor = SystemColors.ActiveCaptionText;
            pinList.Location = new Point(2, 32);
            pinList.Margin = new Padding(2);
            pinList.MultiSelect = false;
            pinList.Name = "pinList";
            pinList.RowHeadersVisible = false;
            pinList.RowTemplate.Height = 24;
            pinList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            pinList.Size = new Size(224, 202);
            pinList.TabIndex = 7;
            pinList.DoubleClick += pinList_ItemActivate;
            pinList.SelectionChanged += pinList_SelectedIndexChanged;
            pinIDcolumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            pinIDcolumn.HeaderText = "ID";
            pinIDcolumn.MinimumWidth = 15;
            pinIDcolumn.Name = "pinIDcolumn";
            pinIDcolumn.ReadOnly = true;
            pinNameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            pinNameColumn.HeaderText = "Name";
            pinNameColumn.Name = "pinNameColumn";
            pinNameColumn.ReadOnly = true;
            LocationColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            LocationColumn.HeaderText = "Location";
            LocationColumn.Name = "LocationColumn";
            Error.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Error.HeaderText = "Error";
            Error.Name = "Error";
            lockStatusText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lockStatusText.BackColor = SystemColors.ControlLightLight;
            lockStatusText.BorderStyle = BorderStyle.None;
            lockStatusText.Location = new Point(3, 239);
            lockStatusText.Multiline = true;
            lockStatusText.Name = "lockStatusText";
            lockStatusText.ReadOnly = true;
            lockStatusText.Size = new Size(223, 48);
            lockStatusText.TabIndex = 6;
            lockStatusText.TabStop = false;
            forceAffineCheckBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            forceAffineCheckBox.AutoSize = true;
            forceAffineCheckBox.Location = new Point(9, 361);
            forceAffineCheckBox.Name = "forceAffineCheckBox";
            forceAffineCheckBox.Size = new Size(83, 17);
            forceAffineCheckBox.TabIndex = 11;
            forceAffineCheckBox.Text = "Force Affine";
            toolTip.SetToolTip(forceAffineCheckBox,
                "Selecting \"Affine\" forces MapCruncher to preserve straight lines in your map.  This reduces position accuracy.");
            forceAffineCheckBox.UseMnemonic = false;
            forceAffineCheckBox.UseVisualStyleBackColor = true;
            forceAffineCheckBox.CheckedChanged += checkBox1_CheckedChanged;
            panel1.Controls.Add(getStartedBox);
            panel1.Controls.Add(lockStatusText);
            panel1.Controls.Add(pinNameTable);
            panel1.Controls.Add(forceAffineCheckBox);
            panel1.Controls.Add(LockButtonTable);
            panel1.Controls.Add(pinList);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(228, 388);
            panel1.TabIndex = 12;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panel1);
            Name = "registrationControls";
            Size = new Size(228, 388);
            getStartedBox.ResumeLayout(false);
            getStartedBox.PerformLayout();
            pinNameTable.ResumeLayout(false);
            pinNameTable.PerformLayout();
            LockButtonTable.ResumeLayout(false);
            ((ISupportInitialize)pinList).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        public registrationControls()
        {
            InitializeComponent();
            unlockTransformButton.Enabled = false;
            lockTransformButton.Enabled = false;
            forceAffineCheckBox.Visible = false;
        }

        private void WarpStyleClick(object sender, EventArgs e)
        {
            if (registrationControl != null)
            {
                registrationControl.model.warpStyle = (TransformationStyle)((RadioButton)sender).Tag;
                updateWarpStyle();
            }
        }

        public void setAssociationIfc(AssociationIfc ai)
        {
            associationIfc = ai;
        }

        public void SetSelected(PositionAssociation pa)
        {
            foreach (DataGridViewRow dataGridViewRow in pinList.Rows)
            {
                if (dataGridViewRow.Tag == pa)
                {
                    dataGridViewRow.Selected = true;
                    pinList.CurrentCell = dataGridViewRow.Cells[0];
                    return;
                }
            }

            UnselectAll();
        }

        private void UnselectAll()
        {
            foreach (DataGridViewRow dataGridViewRow in pinList.SelectedRows)
            {
                dataGridViewRow.Selected = false;
            }
        }

        public PositionAssociation GetSelected()
        {
            foreach (DataGridViewRow dataGridViewRow in pinList.Rows)
            {
                if (dataGridViewRow.Selected)
                {
                    return (PositionAssociation)dataGridViewRow.Tag;
                }
            }

            return null;
        }

        public void DisplayModel(RegistrationControlRecord registrationControl)
        {
            this.registrationControl = registrationControl;
            forceAffineCheckBox.Visible = BuildConfig.theConfig.forceAffineControlVisible;
            pinList.Rows.Clear();
            if (registrationControl != null)
            {
                foreach (PositionAssociation current in registrationControl.model.GetAssociationList())
                {
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    string text = string.Format("{0}, {1}",
                        dms.FormatLatLon(current.globalPosition.pinPosition.lat),
                        dms.FormatLatLon(current.globalPosition.pinPosition.lon));
                    dataGridViewRow.CreateCells(pinList,
                        new object[] {current.pinId, current.associationName, text, current.qualityMessage});
                    dataGridViewRow.Tag = current;
                    pinList.Rows.Add(dataGridViewRow);
                }

                if (registrationControl.model.isLocked)
                {
                    pinText.Enabled = addPushPinButton.Enabled = updatePushPinButton.Enabled = false;
                    removePushPinButton.Enabled = false;
                    removeAllPushpinsButton.Enabled = false;
                    getStartedBox.Visible = false;
                }
                else
                {
                    bool flag = registrationControl.model.GetAssociationList().Count > 0;
                    pinText.Enabled = addPushPinButton.Enabled = updatePushPinButton.Enabled = true;
                    removeAllPushpinsButton.Enabled = flag;
                    getStartedBox.Visible = !flag;
                }
            }
            else
            {
                removePushPinButton.Enabled = false;
                removeAllPushpinsButton.Enabled = false;
                pinText.Enabled = addPushPinButton.Enabled = updatePushPinButton.Enabled = false;
            }

            updateWarpStyle();
        }

        private void updateWarpStyle()
        {
            if (registrationControl == null)
            {
                lockTransformButton.Enabled = false;
                unlockTransformButton.Enabled = false;
                forceAffineCheckBox.Enabled = false;
                lockStatusText.Text = "";
                return;
            }

            forceAffineCheckBox.Checked = registrationControl.model.warpStyle !=
                                               TransformationStyleFactory.getDefaultTransformationStyle();
            if (registrationControl.model.isLocked)
            {
                lockTransformButton.Enabled = false;
                lockStatusText.Text = "Explore the map. Select Render tab when done, or Unlock to improve.";
                unlockTransformButton.Enabled = true;
                forceAffineCheckBox.Enabled = false;
                return;
            }

            bool enabled = registrationControl.readyToLock.ReadyToLock();
            lockTransformButton.Enabled = enabled;
            unlockTransformButton.Enabled = false;
            lockStatusText.Lines = registrationControl.model.GetLockStatusText();
            forceAffineCheckBox.Enabled = true;
        }

        private void addPushPinButton_Click(object sender, EventArgs e)
        {
            try
            {
                associationIfc.AddNewAssociation(pinText.Text);
            }
            catch (DuplicatePushpinException ex)
            {
                DisplayDuplicatePushpinMessage("Cannot add pushpin.", ex);
            }

            pinText.Text = "";
            UnselectAll();
        }

        private void updatePushPinButton_Click(object sender, EventArgs e)
        {
            if (pinList.SelectedRows.Count == 0)
            {
                return;
            }

            int firstDisplayedScrollingRowIndex = pinList.FirstDisplayedScrollingRowIndex;
            PositionAssociation positionAssociation = (PositionAssociation)pinList.SelectedRows[0].Tag;
            try
            {
                associationIfc.UpdateAssociation(positionAssociation, pinText.Text);
            }
            catch (DuplicatePushpinException ex)
            {
                DisplayDuplicatePushpinMessage("Cannot update pushpin.", ex);
            }

            pinText.Text = "";
            SetSelected(positionAssociation);
            pinList.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
        }

        private void DisplayDuplicatePushpinMessage(string action, DuplicatePushpinException ex)
        {
            MessageBox.Show(string.Format("{0} {1}", action, ex.ToString()), "Duplicate Pushpin");
        }

        private void removePushPinButton_Click(object sender, EventArgs e)
        {
            int firstDisplayedScrollingRowIndex = pinList.FirstDisplayedScrollingRowIndex;
            DataGridViewRow dataGridViewRow = pinList.SelectedRows[0];
            int index = dataGridViewRow.Index;
            associationIfc.RemoveAssociation((PositionAssociation)dataGridViewRow.Tag);
            if (pinList.Rows.Count > 0)
            {
                pinList.FirstDisplayedScrollingRowIndex = firstDisplayedScrollingRowIndex;
            }

            if (pinList.Rows.Count > index)
            {
                pinList.Rows[index].Selected = true;
                return;
            }

            if (pinList.Rows.Count > 0)
            {
                pinList.Rows[pinList.Rows.Count - 1].Selected = true;
            }
        }

        private void removeAllPushpinsButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Remove all pushpins?",
                    "Are you sure?",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Exclamation) != DialogResult.OK)
            {
                return;
            }

            while (pinList.Rows.Count > 0)
            {
                associationIfc.RemoveAssociation((PositionAssociation)pinList.Rows[0].Tag);
            }
        }

        private void pinList_SelectedIndexChanged(object sender, EventArgs e)
        {
            removePushPinButton.Enabled = pinList.SelectedRows.Count != 0 &&
                                               registrationControl != null &&
                                               !registrationControl.model.isLocked;
            updatePushPinButton.Enabled = removePushPinButton.Enabled;
        }

        private void pinList_ItemActivate(object sender, EventArgs e)
        {
            if (pinList.SelectedRows.Count > 0)
            {
                associationIfc.ViewAssociation((PositionAssociation)pinList.SelectedRows[0].Tag);
            }
        }

        private void lockTransformButton_Click(object sender, EventArgs e)
        {
            associationIfc.LockMaps();
        }

        private void unlockTransformButton_Click(object sender, EventArgs e)
        {
            associationIfc.UnlockMaps();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (registrationControl != null)
            {
                registrationControl.model.warpStyle = ((CheckBox)sender).Checked
                    ? TransformationStyleFactory.getTransformationStyle(1)
                    : TransformationStyleFactory.getDefaultTransformationStyle();
                updateWarpStyle();
            }
        }

        public void InvalidateView()
        {
            dms.outputMode = _ShowDMS.Enabled
                ? DegreesMinutesSeconds.OutputMode.DMS
                : DegreesMinutesSeconds.OutputMode.DecimalDegrees;
            DisplayModel(registrationControl);
        }
    }
}
