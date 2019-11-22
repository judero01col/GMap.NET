using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderedMashupViewer : Form
    {
        private MapPosition mapPos;
        private CachePackage cachePackage;
        private PrintDocument printDoc;
        private IContainer components;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem vEBackgroundToolStripMenuItem;
        private ToolStripMenuItem VEroadView;
        private ToolStripMenuItem VEaerialView;
        private ToolStripMenuItem VEhybridView;
        private ToolStripMenuItem mashupLayersMenuItem;
        private ToolStripMenuItem addLayerToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ViewerControl viewer;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem printToolStripMenuItem;
        private ToolStripMenuItem pageSetupToolStripMenuItem;
        private ToolStripMenuItem printPreviewToolStripMenuItem;

        public RenderedMashupViewer(CachePackage cachePackage, ToolStripMenuItem dmsMenuItem)
        {
            InitializeComponent();
            this.cachePackage = cachePackage;
            mapPos = new MapPosition(viewer);
            viewer.Initialize(GetMapPos, "Map Location");
            viewer.ShowDMS = new MapDrawingOption(viewer, dmsMenuItem, false);
            SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
            mapPos.setPosition(viewer.GetCoordinateSystem().GetDefaultView());
            printDoc = new PrintDocument();
            printDoc.PrintPage += PrintPage;
        }

        private MapPosition GetMapPos()
        {
            return mapPos;
        }

        private void SetVEMapStyle(string s)
        {
            if (!VirtualEarthWebDownloader.StyleIsValid(s))
            {
                return;
            }

            VEroadView.Checked = s == VirtualEarthWebDownloader.RoadStyle;
            VEaerialView.Checked = s == VirtualEarthWebDownloader.AerialStyle;
            VEhybridView.Checked = s == VirtualEarthWebDownloader.HybridStyle;
            viewer.SetBaseLayer(new VETileSource(cachePackage, s));
        }

        private void VEroadView_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
        }

        private void VEaerialView_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.AerialStyle);
        }

        private void VEhybridView_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.HybridStyle);
        }

        private void addLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addLayers(RenderedLayerSelector.GetLayerSelector(viewer, cachePackage));
        }

        private void addLayers(RenderedLayerDisplayInfo displayInfo)
        {
            if (displayInfo != null)
            {
                foreach (ToolStripMenuItem current in displayInfo.tsmiList)
                {
                    mashupLayersMenuItem.DropDownItems.Add(current);
                }

                mapPos.setPosition(displayInfo.defaultView);
            }
        }

        internal void AddLayersFromUri(Uri uri)
        {
            addLayers(RenderedLayerSelector.GetLayerSelector(viewer, cachePackage, uri));
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (new PrintDialog {Document = printDoc}.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();
            pageSetupDialog.Document = printDoc;
            pageSetupDialog.AllowOrientation = true;
            pageSetupDialog.AllowMargins = true;
            pageSetupDialog.AllowPaper = true;
            DebugPrintSettings();
            pageSetupDialog.ShowDialog();
            DebugPrintSettings();
        }

        private void DebugPrintSettings()
        {
            D.Say(0,
                string.Format("Printer {0} Paper {1} Width {2} Landscape {3} Color {4}",
                    new object[]
                    {
                        printDoc.PrinterSettings.PrinterName, printDoc.DefaultPageSettings.PaperSize,
                        printDoc.DefaultPageSettings.PaperSize.Width,
                        printDoc.DefaultPageSettings.Landscape, printDoc.DefaultPageSettings.Color
                    }));
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.TranslateTransform(e.MarginBounds.X, e.MarginBounds.Y);
            Rectangle rectangle = new Rectangle(0, 0, e.MarginBounds.Width, e.MarginBounds.Height);
            e.Graphics.SetClip(rectangle);
            int num = 4;
            float num2 = 1 << num;
            e.Graphics.ScaleTransform(1f / num2, 1f / num2);
            rectangle.Width = (int)(rectangle.Width * num2);
            rectangle.Height = (int)(rectangle.Height * num2);
            GraphicsContainer container = e.Graphics.BeginContainer();
            PaintSpecification e2 = new PaintSpecification(e.Graphics, rectangle, rectangle.Size, true);
            viewer.PaintPrintWindow(e2, num);
            e.Graphics.EndContainer(container);
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new PrintPreviewDialog {Document = printDoc}.ShowDialog();
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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(RenderedMashupViewer));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            pageSetupToolStripMenuItem = new ToolStripMenuItem();
            printPreviewToolStripMenuItem = new ToolStripMenuItem();
            printToolStripMenuItem = new ToolStripMenuItem();
            vEBackgroundToolStripMenuItem = new ToolStripMenuItem();
            VEroadView = new ToolStripMenuItem();
            VEaerialView = new ToolStripMenuItem();
            VEhybridView = new ToolStripMenuItem();
            mashupLayersMenuItem = new ToolStripMenuItem();
            addLayerToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            viewer = new ViewerControl();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                fileToolStripMenuItem, vEBackgroundToolStripMenuItem, mashupLayersMenuItem
            });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(792, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                pageSetupToolStripMenuItem, printPreviewToolStripMenuItem, printToolStripMenuItem
            });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(35, 20);
            fileToolStripMenuItem.Text = "File";
            fileToolStripMenuItem.Visible = false;
            pageSetupToolStripMenuItem.Name = "pageSetupToolStripMenuItem";
            pageSetupToolStripMenuItem.Size = new Size(160, 22);
            pageSetupToolStripMenuItem.Text = "Page Setup...";
            pageSetupToolStripMenuItem.Click += pageSetupToolStripMenuItem_Click;
            printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            printPreviewToolStripMenuItem.Size = new Size(160, 22);
            printPreviewToolStripMenuItem.Text = "Print Preview...";
            printPreviewToolStripMenuItem.Click += printPreviewToolStripMenuItem_Click;
            printToolStripMenuItem.Name = "printToolStripMenuItem";
            printToolStripMenuItem.Size = new Size(160, 22);
            printToolStripMenuItem.Text = "Print...";
            printToolStripMenuItem.Click += printToolStripMenuItem_Click;
            vEBackgroundToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                VEroadView, VEaerialView, VEhybridView
            });
            vEBackgroundToolStripMenuItem.Name = "vEBackgroundToolStripMenuItem";
            vEBackgroundToolStripMenuItem.Size = new Size(90, 20);
            vEBackgroundToolStripMenuItem.Text = "VE Background";
            VEroadView.Name = "VEroadView";
            VEroadView.Size = new Size(148, 22);
            VEroadView.Text = "Roads";
            VEroadView.Click += VEroadView_Click;
            VEaerialView.Name = "VEaerialView";
            VEaerialView.Size = new Size(148, 22);
            VEaerialView.Text = "Aerial Photos";
            VEaerialView.Click += VEaerialView_Click;
            VEhybridView.Name = "VEhybridView";
            VEhybridView.Size = new Size(148, 22);
            VEhybridView.Text = "Hybrid";
            VEhybridView.Click += VEhybridView_Click;
            mashupLayersMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                addLayerToolStripMenuItem, toolStripSeparator1
            });
            mashupLayersMenuItem.Name = "mashupLayersMenuItem";
            mashupLayersMenuItem.Size = new Size(91, 20);
            mashupLayersMenuItem.Text = "Mashup Layers";
            addLayerToolStripMenuItem.Name = "addLayerToolStripMenuItem";
            addLayerToolStripMenuItem.Size = new Size(146, 22);
            addLayerToolStripMenuItem.Text = "Add Layer...";
            addLayerToolStripMenuItem.Click += addLayerToolStripMenuItem_Click;
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(143, 6);
            viewer.Dock = DockStyle.Fill;
            viewer.Location = new Point(0, 24);
            viewer.Name = "viewer";
            viewer.Size = new Size(792, 542);
            viewer.TabIndex = 1;
            AutoScaleDimensions = new SizeF(6f, 13f);
            //base.AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(792, 566);
            Controls.Add(viewer);
            Controls.Add(menuStrip1);
            //base.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "RenderedMashupViewer";
            Text = "Mashup Viewer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
