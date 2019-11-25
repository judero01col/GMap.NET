using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public class LayerControls : UserControl
    {
        private delegate void ReloadDelegate();

        private IContainer components;
        private GroupBox groupBox1;
        private Label getStartedLabel1;
        private Label getStartedLabel2;
        private GroupBox gettingStartedLabel;
        private Mashup _mashup;
        private LayerControlIfc layerControl;
        private MenuItem renameMenuItem;
        private MenuItem addLayerItem;
        private MenuItem removeItem;
        private MenuItem addSourceMapItem;
        private MenuItem addLegendItem;
        private TreeNode clickedNode;
        private Dictionary<object, TreeNode> tagToTreeNodeDict = new Dictionary<object, TreeNode>();

        private static string dataTypeName =
            typeof(NameWatchingTreeNode).Namespace + "." + typeof(NameWatchingTreeNode).Name;

        private TreeNode currentDropHighlight;

        public TreeView layerTreeView
        {
            get;
            private set;
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
            groupBox1 = new GroupBox();
            gettingStartedLabel = new GroupBox();
            getStartedLabel1 = new Label();
            getStartedLabel2 = new Label();
            layerTreeView = new TreeView();
            groupBox1.SuspendLayout();
            gettingStartedLabel.SuspendLayout();
            SuspendLayout();
            groupBox1.Controls.Add(gettingStartedLabel);
            groupBox1.Controls.Add(layerTreeView);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(221, 237);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Map Layers";
            gettingStartedLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gettingStartedLabel.BackColor = SystemColors.ControlLightLight;
            gettingStartedLabel.Controls.Add(getStartedLabel1);
            gettingStartedLabel.Controls.Add(getStartedLabel2);
            gettingStartedLabel.Location = new Point(13, 23);
            gettingStartedLabel.Name = "gettingStartedLabel";
            gettingStartedLabel.Size = new Size(196, 70);
            gettingStartedLabel.TabIndex = 3;
            gettingStartedLabel.TabStop = false;
            getStartedLabel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            getStartedLabel1.AutoSize = true;
            getStartedLabel1.BackColor = SystemColors.ControlLightLight;
            getStartedLabel1.Font =
                new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
            getStartedLabel1.ForeColor = Color.Red;
            getStartedLabel1.Location = new Point(7, 11);
            getStartedLabel1.Name = "getStartedLabel1";
            getStartedLabel1.Size = new Size(179, 22);
            getStartedLabel1.TabIndex = 1;
            getStartedLabel1.Text = "To get started, select";
            getStartedLabel1.TextAlign = ContentAlignment.MiddleCenter;
            getStartedLabel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            getStartedLabel2.AutoSize = true;
            getStartedLabel2.BackColor = SystemColors.ControlLightLight;
            getStartedLabel2.Font =
                new Font("Microsoft Sans Serif", 11f, FontStyle.Regular, GraphicsUnit.Point, 0);
            getStartedLabel2.ForeColor = Color.Red;
            getStartedLabel2.Location = new Point(5, 36);
            getStartedLabel2.Name = "getStartedLabel2";
            getStartedLabel2.Size = new Size(187, 22);
            getStartedLabel2.TabIndex = 2;
            getStartedLabel2.Text = "File | Add Source Map";
            getStartedLabel2.TextAlign = ContentAlignment.MiddleCenter;
            layerTreeView.AllowDrop = true;
            layerTreeView.Dock = DockStyle.Fill;
            layerTreeView.HideSelection = false;
            layerTreeView.LabelEdit = true;
            layerTreeView.Location = new Point(3, 16);
            layerTreeView.Name = "layerTreeView";
            layerTreeView.Size = new Size(215, 218);
            layerTreeView.TabIndex = 0;
            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox1);
            Name = "LayerControls";
            Size = new Size(221, 237);
            groupBox1.ResumeLayout(false);
            gettingStartedLabel.ResumeLayout(false);
            gettingStartedLabel.PerformLayout();
            ResumeLayout(false);
        }

        public LayerControls()
        {
            InitializeComponent();
            layerTreeView.Tag = this;
            layerTreeView.AfterSelect += NodeSelectedHandler;
            layerTreeView.AfterExpand += NodeExpandedHandler;
            layerTreeView.AfterCollapse += NodeExpandedHandler;
            layerTreeView.AfterLabelEdit += NodeLabelEditHandler;
            layerTreeView.MouseDown += layerTreeView_MouseDown;
            layerTreeView.ItemDrag += layerTreeView_ItemDrag;
            layerTreeView.DragEnter += layerTreeView_DragEnter;
            layerTreeView.DragOver += layerTreeView_DragOver;
            layerTreeView.DragDrop += layerTreeView_DragDrop;
            layerTreeView.DragLeave += layerTreeView_DragLeave;
            ContextMenu = new ContextMenu();
            ContextMenu.Popup += PopupHandler;
            addLayerItem = new MenuItem("Add Layer", AddLayerHandler);
            ContextMenu.MenuItems.Add(addLayerItem);
            addSourceMapItem = new MenuItem("Add Source Map", AddSourceMapHandler);
            ContextMenu.MenuItems.Add(addSourceMapItem);
            addLegendItem = new MenuItem("Add Legend", AddLegendHandler);
            ContextMenu.MenuItems.Add(addLegendItem);
            renameMenuItem = new MenuItem("Rename", RenameHandler);
            ContextMenu.MenuItems.Add(renameMenuItem);
            removeItem = new MenuItem("Remove", RemoveHandler);
            ContextMenu.MenuItems.Add(removeItem);
        }

        private void layerTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void layerTreeView_DragEnter(object sender, DragEventArgs e)
        {
            D.Sayf(0, "e.Data = {0}", new object[] {e.Data.ToString()});
            if (e.Data.GetDataPresent(dataTypeName, true))
            {
                e.Effect = DragDropEffects.Move;
                return;
            }

            e.Effect = DragDropEffects.None;
        }

        private void layerTreeView_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(dataTypeName, true))
            {
                return;
            }

            TreeNode treeNode = (TreeNode)e.Data.GetData(dataTypeName);
            TreeView treeView = (TreeView)sender;
            Point pt = treeView.PointToClient(new Point(e.X, e.Y));
            TreeNode nodeAt = treeView.GetNodeAt(pt);
            if (nodeAt == null || treeNode.Tag is Layer && !(nodeAt.Tag is Layer))
            {
                ((LayerControls)treeView.Tag).SetDropHighlight(null);
                e.Effect = DragDropEffects.None;
                return;
            }

            ((LayerControls)treeView.Tag).SetDropHighlight(nodeAt);
            e.Effect = DragDropEffects.Move;
        }

        private void layerTreeView_DragDrop(object sender, DragEventArgs e)
        {
            TreeView treeView = (TreeView)sender;
            try
            {
                if (e.Data.GetDataPresent(dataTypeName, true))
                {
                    TreeNode treeNode = (TreeNode)e.Data.GetData(dataTypeName);
                    TreeNode treeNode2 = ((LayerControls)treeView.Tag).currentDropHighlight;
                    if (treeNode2 != null)
                    {
                        D.Assert(treeView.Tag == this,
                            "Not currently designed to support drags from one layer control to another.");
                        if (treeNode2.Tag == treeNode.Tag)
                        {
                            D.Sayf(0, "Ignoring drop onto self", new object[0]);
                        }
                        else
                        {
                            if (treeNode.Tag is SourceMap)
                            {
                                SourceMap sourceMap = (SourceMap)treeNode.Tag;
                                Layer layer = (Layer)treeNode.Parent.Tag;
                                if (treeNode2.Tag is Layer)
                                {
                                    Layer layer2 = (Layer)treeNode2.Tag;
                                    layer.Remove(sourceMap);
                                    layer2.AddAt(sourceMap, 0);
                                }
                                else
                                {
                                    if (treeNode2.Tag is SourceMap)
                                    {
                                        SourceMap targetSourceMap = (SourceMap)treeNode2.Tag;
                                        Layer layer3 = (Layer)treeNode2.Parent.Tag;
                                        layer.Remove(sourceMap);
                                        int index = layer3.GetIndexOfSourceMap(targetSourceMap) + 1;
                                        layer3.AddAt(sourceMap, index);
                                    }
                                    else
                                    {
                                        D.Assert(false, "unknown case");
                                    }
                                }

                                object tag = layerTreeView.SelectedNode.Tag;
                                Reload();
                                IEnumerator enumerator = layerTreeView.Nodes.GetEnumerator();
                                try
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        TreeNode treeNode3 = (TreeNode)enumerator.Current;
                                        if (treeNode3.Tag == tag)
                                        {
                                            layerTreeView.SelectedNode = treeNode3;
                                            break;
                                        }
                                    }

                                    goto IL_226;
                                }
                                finally
                                {
                                    IDisposable disposable = enumerator as IDisposable;
                                    if (disposable != null)
                                    {
                                        disposable.Dispose();
                                    }
                                }
                            }

                            if (treeNode.Tag is Layer)
                            {
                                Layer layer4 = (Layer)treeNode.Tag;
                                if (!(treeNode2.Tag is Layer))
                                {
                                    return;
                                }

                                Layer layer5 = (Layer)treeNode2.Tag;
                                if (layer5 == layer4)
                                {
                                    return;
                                }

                                _mashup.layerList.Remove(layer4);
                                _mashup.layerList.AddAfter(layer4, layer5);
                                Reload();
                            }
                            else
                            {
                                D.Assert(false, "didn't think tree could contain anything else");
                            }
                        }

                        IL_226:
                        treeNode2.EnsureVisible();
                    }
                }
            }
            finally
            {
                ((LayerControls)treeView.Tag).SetDropHighlight(null);
            }
        }

        private void layerTreeView_DragLeave(object sender, EventArgs e)
        {
            SetDropHighlight(null);
        }

        private void SetDropHighlight(TreeNode targetNode)
        {
            if (currentDropHighlight == targetNode)
            {
                return;
            }

            if (currentDropHighlight != null)
            {
                currentDropHighlight.BackColor = layerTreeView.BackColor;
            }

            currentDropHighlight = targetNode;
            if (currentDropHighlight != null)
            {
                currentDropHighlight.BackColor = Color.ForestGreen;
            }
        }

        public void SetLayerControl(LayerControlIfc layerControl)
        {
            this.layerControl = layerControl;
        }

        public void SetMashup(Mashup mashup)
        {
            _mashup = mashup;
            Reload();
        }

        private void Reload()
        {
            layerTreeView.Nodes.Clear();
            if (_mashup != null)
            {
                foreach (Layer current in _mashup.layerList)
                {
                    List<TreeNode> list = new List<TreeNode>();
                    foreach (SourceMap current2 in current)
                    {
                        List<TreeNode> list2 = new List<TreeNode>();
                        foreach (Legend current3 in current2.legendList)
                        {
                            TreeNode treeNode = new NameWatchingTreeNode(current3);
                            tagToTreeNodeDict[current3] = treeNode;
                            list2.Add(treeNode);
                        }

                        TreeNode treeNode2 = new NameWatchingTreeNode(current2, list2.ToArray());
                        tagToTreeNodeDict[current2] = treeNode2;
                        if (current2.expanded)
                        {
                            treeNode2.Expand();
                        }
                        else
                        {
                            treeNode2.Collapse();
                        }

                        list.Add(treeNode2);
                    }

                    TreeNode treeNode3 = new NameWatchingTreeNode(current, list.ToArray());
                    tagToTreeNodeDict[current] = treeNode3;
                    if (current.expanded)
                    {
                        treeNode3.Expand();
                    }
                    else
                    {
                        treeNode3.Collapse();
                    }

                    layerTreeView.Nodes.Add(treeNode3);
                }
            }

            if (_mashup != null)
            {
                if (_mashup.lastView is SourceMapRegistrationView)
                {
                    SelectObject(((SourceMapRegistrationView)_mashup.lastView).sourceMap);
                }
                else
                {
                    if (_mashup.lastView is LayerView)
                    {
                        SelectObject(((LayerView)_mashup.lastView).layer);
                    }
                    else
                    {
                        if (_mashup.lastView is LegendView)
                        {
                            SelectObject(((LegendView)_mashup.lastView).legend);
                        }
                        else
                        {
                            if (_mashup.lastView is NoView)
                            {
                                SelectObject(null);
                            }
                            else
                            {
                                SelectObject(null);
                            }
                        }
                    }
                }
            }

            gettingStartedLabel.Visible = _mashup == null || _mashup.layerList.Count <= 0;
            Invalidate();
        }

        public void SelectObject(object obj)
        {
            if (obj == null)
            {
                layerTreeView.SelectedNode = null;
                return;
            }

            layerTreeView.SelectedNode = tagToTreeNodeDict[obj];
        }

        public Layer AddSourceMap(SourceMap sourceMap)
        {
            TreeNode selectedNode = layerTreeView.SelectedNode;
            Layer layer;
            if (selectedNode != null)
            {
                if (selectedNode.Tag is Layer)
                {
                    layer = (Layer)selectedNode.Tag;
                }
                else
                {
                    layer = (Layer)selectedNode.Parent.Tag;
                }
            }
            else
            {
                if (_mashup.layerList.Count == 0)
                {
                    _mashup.layerList.AddNewLayer();
                }

                layer = _mashup.layerList.First;
            }

            layer.Add(sourceMap);
            Reload();
            return layer;
        }

        public void CancelSourceMap(Layer layer, SourceMap sourceMap)
        {
            ReloadDelegate method = Reload;
            Invoke(method);
        }

        private void NodeSelectedHandler(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Node.Tag is SourceMap)
                {
                    SourceMap sourceMap = (SourceMap)e.Node.Tag;
                    layerControl.OpenSourceMap(sourceMap);
                }
                else
                {
                    if (e.Node.Tag is Layer)
                    {
                        Layer layer = (Layer)e.Node.Tag;
                        layerControl.OpenLayer(layer);
                    }
                    else
                    {
                        if (e.Node.Tag is Legend)
                        {
                            Legend legend = (Legend)e.Node.Tag;
                            layerControl.OpenLegend(legend);
                        }
                    }
                }
            }
            catch (UnknownImageTypeException)
            {
            }
        }

        private void NodeExpandedHandler(object sender, TreeViewEventArgs e)
        {
            ((ExpandedMemoryIfc)e.Node.Tag).expanded = e.Node.IsExpanded;
        }

        private void NodeLabelEditHandler(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                e.CancelEdit = true;
                return;
            }

            D.Say(0, string.Format("From {0} to {1}", e.Node.Text, e.Label));
            ((HasDisplayNameIfc)e.Node.Tag).SetDisplayName(e.Label);
            e.Node.Text = e.Label;
        }

        private void PopupHandler(object sender, EventArgs e)
        {
            layerTreeView.SelectedNode = clickedNode;
            addLayerItem.Enabled = true;
            addSourceMapItem.Enabled = true;
            removeItem.Enabled = false;
            removeItem.Text = "Remove";
            removeItem.Enabled = false;
            renameMenuItem.Enabled = false;
            addLegendItem.Enabled = false;
            if (layerTreeView.SelectedNode != null)
            {
                object tag = layerTreeView.SelectedNode.Tag;
                addLayerItem.Enabled = tag is Layer;
                addSourceMapItem.Enabled = tag is Layer;
                addLegendItem.Enabled = tag is SourceMap;
                renameMenuItem.Enabled = true;
                if (tag is Layer)
                {
                    if (_mashup.layerList.Count < 2)
                    {
                        removeItem.Enabled = false;
                        removeItem.Text = "Remove Layer (no other layers)";
                        return;
                    }

                    if (((Layer)tag).Count > 0)
                    {
                        removeItem.Enabled = false;
                        removeItem.Text = "Remove Layer (layer not empty)";
                        return;
                    }

                    removeItem.Enabled = true;
                    removeItem.Text = "Remove Layer";
                    return;
                }
                else
                {
                    if (tag is SourceMap)
                    {
                        removeItem.Enabled = true;
                        removeItem.Text = "Remove Source Map";
                        return;
                    }

                    if (tag is Legend)
                    {
                        removeItem.Enabled = true;
                        removeItem.Text = "Remove Legend";
                    }
                }
            }
        }

        private void RenameHandler(object sender, EventArgs e)
        {
            layerTreeView.SelectedNode.BeginEdit();
        }

        private void AddLayerHandler(object sender, EventArgs e)
        {
            _mashup.layerList.AddNewLayer();
            Reload();
        }

        private void RemoveHandler(object sender, EventArgs e)
        {
            object tag = layerTreeView.SelectedNode.Tag;
            if (tag is Layer)
            {
                _mashup.layerList.Remove((Layer)tag);
            }
            else
            {
                if (tag is SourceMap)
                {
                    SourceMap sourceMap = (SourceMap)tag;
                    DialogResult dialogResult =
                        MessageBox.Show(
                            string.Format("Are you sure you want to remove source map {0}?", sourceMap.displayName),
                            "Remove source map?",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Question);
                    if (dialogResult != DialogResult.OK)
                    {
                        return;
                    }

                    layerControl.RemoveSourceMap(sourceMap);
                }
                else
                {
                    if (tag is Legend)
                    {
                        Legend legend = (Legend)tag;
                        legend.sourceMap.legendList.RemoveLegend(legend);
                    }
                    else
                    {
                        D.Assert(false);
                    }
                }
            }

            object obj = null;
            if (layerTreeView.SelectedNode.Parent != null)
            {
                obj = layerTreeView.SelectedNode.Parent.Tag;
            }

            Reload();
            if (obj != null)
            {
                layerTreeView.SelectedNode = tagToTreeNodeDict[obj];
                return;
            }

            if (_mashup.layerList.Count > 0)
            {
                layerTreeView.SelectedNode = tagToTreeNodeDict[_mashup.layerList.First];
            }
        }

        private void AddSourceMapHandler(object sender, EventArgs e)
        {
            layerControl.AddSourceMap();
        }

        private void AddLegendHandler(object sender, EventArgs e)
        {
            SourceMap sourceMap = (SourceMap)layerTreeView.SelectedNode.Tag;
            Legend key = sourceMap.legendList.AddNewLegend();
            Reload();
            layerTreeView.SelectedNode = tagToTreeNodeDict[key];
        }

        private void layerTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            clickedNode = layerTreeView.GetNodeAt(e.X, e.Y);
        }
    }
}
