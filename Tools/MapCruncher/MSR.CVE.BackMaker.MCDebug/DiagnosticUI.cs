using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker.MCDebug
{
    public class DiagnosticUI : Form, ListUIIfc
    {
        private delegate void CACDelegate();

        private delegate void UQLDelegate();

        private static DiagnosticUI _theDiagnostics;
        private Dictionary<string, ResourceCounter> resourceCountersByName = new Dictionary<string, ResourceCounter>();

        private Dictionary<ResourceCounter, DataGridViewRow> resourceCounterToGridRow =
            new Dictionary<ResourceCounter, DataGridViewRow>();

        private EventWaitHandle queueListChangedEvent =
            new EventWaitHandle(false, EventResetMode.AutoReset, "DiagnosticUI.queueListChangedEvent");

        private List<string> newResourceNames = new List<string>();
        private bool canInvoke;
        private List<object> queueList;
        private DateTime lastQueueDraw;
        private IContainer components;
        private DataGridView resourceCountersGridView;
        private DataGridViewTextBoxColumn resourceName;
        private DataGridViewTextBoxColumn Count;
        private ListBox renderQueueListBox;
        private SplitContainer splitContainer1;

        public static DiagnosticUI theDiagnostics
        {
            get
            {
                if (_theDiagnostics == null)
                {
                    _theDiagnostics = new DiagnosticUI();
                }

                return _theDiagnostics;
            }
        }

        public DiagnosticUI()
        {
            InitializeComponent();
            Shown += DiagnosticUI_Shown;
            Closing += DiagnosticUI_Closing;
            DebugThreadInterrupter.theInstance.AddThread("QueueListRedrawThread",
                UpdateQueueListThread,
                ThreadPriority.BelowNormal);
        }

        private void DiagnosticUI_Closing(object sender, CancelEventArgs e)
        {
            canInvoke = false;
        }

        private void DiagnosticUI_Shown(object sender, EventArgs e)
        {
            CreateAllCounters();
            canInvoke = true;
            queueListChangedEvent.Set();
        }

        public ResourceCounter fetchResourceCounter(string resourceName, int period)
        {
            Monitor.Enter(this);
            ResourceCounter result;
            try
            {
                if (!resourceCountersByName.ContainsKey(resourceName))
                {
                    resourceCountersByName[resourceName] = new ResourceCounter(resourceName,
                        period,
                        ResourceCounterCallback);
                    if (canInvoke)
                    {
                        DebugThreadInterrupter.theInstance.AddThread("DiagnosticUI.CreateAllCountersInvokeThread",
                            CreateAllCountersInvokeThread,
                            ThreadPriority.Normal);
                    }
                }

                result = resourceCountersByName[resourceName];
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        private void CreateAllCountersInvokeThread()
        {
            CACDelegate method = CreateAllCounters;
            Invoke(method);
        }

        private void CreateAllCounters()
        {
            foreach (string current in resourceCountersByName.Keys)
            {
                ResourceCounter resourceCounter = resourceCountersByName[current];
                if (!resourceCounterToGridRow.ContainsKey(resourceCounter))
                {
                    int index = resourceCountersGridView.Rows.Add();
                    DataGridViewRow dataGridViewRow = resourceCountersGridView.Rows[index];
                    dataGridViewRow.Cells[0].Value = current;
                    dataGridViewRow.Cells[1].Value = resourceCounter.Value;
                    resourceCounterToGridRow.Add(resourceCounter, dataGridViewRow);
                }
            }
        }

        private void ResourceCounterCallback(ResourceCounter resourceCounter)
        {
            if (canInvoke)
            {
                try
                {
                    resourceCounterToGridRow[resourceCounter].Cells[1].Value = resourceCounter.Value;
                }
                catch (KeyNotFoundException)
                {
                }
            }
        }

        public void listChanged(List<object> prefix)
        {
            queueList = prefix;
            queueListChangedEvent.Set();
        }

        private void updateQueueList()
        {
            List<object> list = queueList;
            renderQueueListBox.Items.Clear();
            foreach (object current in list)
            {
                renderQueueListBox.Items.Add(current);
            }

            renderQueueListBox.Refresh();
        }

        private void UpdateQueueListThread()
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                queueListChangedEvent.WaitOne();
                DateTime now = DateTime.Now;
                if (lastQueueDraw.AddMilliseconds(200.0) < now && canInvoke)
                {
                    UQLDelegate method = updateQueueList;
                    BeginInvoke(method);
                    lastQueueDraw = now;
                }
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
            resourceCountersGridView = new DataGridView();
            resourceName = new DataGridViewTextBoxColumn();
            Count = new DataGridViewTextBoxColumn();
            renderQueueListBox = new ListBox();
            splitContainer1 = new SplitContainer();
            ((ISupportInitialize)resourceCountersGridView).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            resourceCountersGridView.AllowUserToAddRows = false;
            resourceCountersGridView.AllowUserToDeleteRows = false;
            resourceCountersGridView.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resourceCountersGridView.Columns.AddRange(new DataGridViewColumn[] {resourceName, Count});
            resourceCountersGridView.Dock = DockStyle.Fill;
            resourceCountersGridView.Location = new Point(0, 0);
            resourceCountersGridView.Name = "resourceCountersGridView";
            resourceCountersGridView.ReadOnly = true;
            resourceCountersGridView.Size = new Size(283, 455);
            resourceCountersGridView.TabIndex = 0;
            resourceName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            resourceName.HeaderText = "Resource";
            resourceName.Name = "resourceName";
            resourceName.ReadOnly = true;
            Count.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Count.HeaderText = "Count";
            Count.Name = "Count";
            Count.ReadOnly = true;
            renderQueueListBox.Dock = DockStyle.Fill;
            renderQueueListBox.FormattingEnabled = true;
            renderQueueListBox.Location = new Point(0, 0);
            renderQueueListBox.Name = "renderQueueListBox";
            renderQueueListBox.Size = new Size(293, 446);
            renderQueueListBox.TabIndex = 1;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel1.Controls.Add(resourceCountersGridView);
            splitContainer1.Panel2.Controls.Add(renderQueueListBox);
            splitContainer1.Size = new Size(580, 455);
            splitContainer1.SplitterDistance = 283;
            splitContainer1.TabIndex = 2;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(580, 455);
            Controls.Add(splitContainer1);
            Name = "DiagnosticUI";
            Text = "DiagnosticUI";
            ((ISupportInitialize)resourceCountersGridView).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
