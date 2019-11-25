using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BackMaker;
using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class MainAppForm : Form, AssociationIfc, LayerControlIfc, ViewControlIfc, DocumentMutabilityControlIfc
    {
        private delegate void DKCUI(bool enabled);

        private delegate void ExitDelegate(int rc);

        private delegate void ReadyToLockChangedDelegate();

        private class Opening
        {
            public bool opening;
        }

        private class UndoAddSourceMap
        {
            private delegate void CloseViewDelegate();

            private string filename;
            private SourceMap newSourceMap;
            private Layer addedToLayer;
            private LayerControls layerControls;
            private MainAppForm mainAppForm;

            public UndoAddSourceMap(string filename, SourceMap newSourceMap, Layer addedToLayer,
                LayerControls layerControls, MainAppForm mainAppForm)
            {
                this.filename = filename;
                this.newSourceMap = newSourceMap;
                this.addedToLayer = addedToLayer;
                this.layerControls = layerControls;
                this.mainAppForm = mainAppForm;
            }

            public void Undo(string message)
            {
                MessageBox.Show(string.Format("Can't open {0}:\n{1}", filename, message),
                    "Can't Open Source Map",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                if (addedToLayer != null && newSourceMap != null && layerControls != null)
                {
                    addedToLayer.Remove(newSourceMap);
                    layerControls.CancelSourceMap(addedToLayer, newSourceMap);
                }

                var method = new CloseViewDelegate(mainAppForm.CloseView);
                mainAppForm.Invoke(method);
            }
        }

        private const string regSourceMapFileName = "last_open_source_map";
        private const string regWindowWidth = "gui_window_width";
        private const string regWindowHeight = "gui_window_height";
        private const string regWindowX = "gui_window_x";
        private const string regWindowY = "gui_window_y";
        private const string regControlSplitterPos = "control_splitter_pos";
        private const string regMapSplitterPos = "gui_splitter_pos";
        public const string DocumentExtension = "yum";
        private const int WM_SETREDRAW = 11;
        private string programName;
        private UIPositionManager uiPosition;
        private CachePackage cachePackage;
        private CachePackage renderedTileCachePackage;
        private MapTileSourceFactory mapTileSourceFactory;
        private BackMakerRegistry backMakerRegistry = new BackMakerRegistry();
        private Mashup currentMashup;
        private RegistrationControlRecord displayedRegistration;
        private IViewManager currentView;
        private bool documentIsMutable;
        private bool alreadyExiting;
        private bool undone;
        private RenderWindow renderWindow;
        private SourceMapOverviewWindow sourceMapOverviewWindow;
        private string startDocumentPath;
        private bool renderOnLaunch;
        private Opening opening = new Opening();
        private int paintFrozen;
        private IContainer components;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutMSRBackMakerToolStripMenuItem;
        private ToolStripMenuItem openMashupMenuItem;
        private ToolStripMenuItem closeMashupMenuItem;
        private ToolStripMenuItem mapOptionsToolStripMenuItem2;
        private ToolStripMenuItem showCrosshairsMenuItem;
        private ToolStripMenuItem showPushPinsMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem newMashupMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem addSourceMapFromUriMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem saveMashupMenuItem;
        private ToolStripMenuItem saveMashupAsMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem viewRenderedMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem VEroadView;
        private ToolStripMenuItem VEaerialView;
        private ToolStripMenuItem VEhybridView;
        private ToolStripMenuItem AddRegLayerMenuItem;
        private ToolStripSeparator snapFeaturesToolStripSeparator;
        private SplitContainer mapSplitContainer;
        private ViewerControl smViewerControl;
        private ViewerControl veViewerControl;
        private SplitContainer controlsSplitContainer;
        private LayerControls layerControls;
        private TabControl synergyExplorer;
        private TabPage correspondencesTab;
        private registrationControls registrationControls;
        private TabPage sourceInfoTab;
        private SourceMapInfoPanel sourceMapInfoPanel;
        private SplitContainer controlSplitContainer;
        private ToolStripMenuItem viewMapCruncherTutorialToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private Button RenderLaunchButton;
        private Panel panel1;
        private ToolStripMenuItem addSourceMapMenuItem;
        private TabPage transparencyTab;
        private TransparencyPanel transparencyPanel;
        private TabPage legendTabPage;
        private LegendOptionsPanel legendOptionsPanel1;
        private ToolStripMenuItem showDMSMenuItem;
        private ToolStripMenuItem showSourceMapOverviewMenuItem;
        private ToolStripMenuItem restoreSnapViewMenuItem;
        private ToolStripMenuItem recordSnapViewMenuItem;
        private ToolStripMenuItem recordSnapZoomMenuItem;
        private ToolStripMenuItem restoreSnapZoomMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem showTileNamesMenuItem;
        private ToolStripMenuItem showSourceCropToolStripMenuItem;
        private ToolStripMenuItem showTileBoundariesMenuItem;
        private ToolStripMenuItem showDiagnosticsUIToolStripMenuItem;
        private ToolStripSeparator debugModeToolStripSeparator;
        private ToolStripMenuItem enableDebugModeToolStripMenuItem;

        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegate6;
        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegate7;
        //[CompilerGenerated]
        //private static Converter<string, string> <>9__CachedAnonymousMethodDelegate9;
        //[CompilerGenerated]
        //private static Converter<PositionAssociation, PositionAssociationView> <>9__CachedAnonymousMethodDelegateb;

        private bool FreezePainting
        {
            get
            {
                return paintFrozen > 0;
            }
            set
            {
                if (value && IsHandleCreated && Visible && paintFrozen++ == 0)
                {
                    SendMessage(Handle, 11, 0, 0);
                }

                if (!value)
                {
                    if (paintFrozen == 0)
                    {
                        return;
                    }

                    if (--paintFrozen == 0)
                    {
                        SendMessage(Handle, 11, 1, 0);
                        Invalidate(true);
                    }
                }
            }
        }

        public MainAppForm(string startDocumentPath, bool renderOnLaunch)
        {
            this.startDocumentPath = startDocumentPath;
            this.renderOnLaunch = renderOnLaunch;
        }

        public void StartUpApplication()
        {
            try
            {
                D.SetDebugLevel(BuildConfig.theConfig.debugLevel);
                cachePackage = new CachePackage();
                renderedTileCachePackage = cachePackage.DeriveCache("renderedTile");
                InitializeComponent();
                SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
                mapTileSourceFactory = new MapTileSourceFactory(cachePackage);
            }
            catch (ConfigurationException)
            {
                UndoConstruction();
                throw;
            }

            layerControls.SetLayerControl(this);
            RestoreWindowParameters();
            SetInterfaceNoMashupOpen();
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 10000;
            timer.Tick += saveBackupTimer_Tick;
            timer.Start();
            registrationControls.ShowDMS =
                new MapDrawingOption(registrationControls, showDMSMenuItem, false);
            smViewerControl.ShowCrosshairs =
                new MapDrawingOption(smViewerControl, showCrosshairsMenuItem, true);
            smViewerControl.ShowTileBoundaries =
                new MapDrawingOption(smViewerControl, showTileBoundariesMenuItem, true);
            smViewerControl.ShowPushPins =
                new MapDrawingOption(smViewerControl, showPushPinsMenuItem, true);
            smViewerControl.ShowTileNames =
                new MapDrawingOption(smViewerControl, showTileNamesMenuItem, false);
            smViewerControl.ShowSourceCrop =
                new MapDrawingOption(smViewerControl, showSourceCropToolStripMenuItem, true);
            smViewerControl.ShowDMS = new MapDrawingOption(smViewerControl, showDMSMenuItem, false);
            smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.XY);
            SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
            veViewerControl.ShowCrosshairs =
                new MapDrawingOption(veViewerControl, showCrosshairsMenuItem, true);
            veViewerControl.ShowTileBoundaries =
                new MapDrawingOption(veViewerControl, showTileBoundariesMenuItem, false);
            veViewerControl.ShowPushPins =
                new MapDrawingOption(veViewerControl, showPushPinsMenuItem, true);
            veViewerControl.ShowTileNames =
                new MapDrawingOption(veViewerControl, showTileNamesMenuItem, false);
            veViewerControl.ShowDMS = new MapDrawingOption(veViewerControl, showDMSMenuItem, false);
            veViewerControl.configureLLZBoxEditable();
            uiPosition = new UIPositionManager(smViewerControl, veViewerControl);
            uiPosition.GetVEPos().setPosition(veViewerControl.GetCoordinateSystem().GetDefaultView());
            recordSnapViewMenuItem.Click += recordSnapViewMenuItem_Click;
            restoreSnapViewMenuItem.Click += restoreSnapViewMenuItem_Click;
            recordSnapZoomMenuItem.Click += recordSnapZoomMenuItem_Click;
            restoreSnapZoomMenuItem.Click += restoreSnapZoomMenuItem_Click;
            registrationControls.setAssociationIfc(this);
            setDisplayedRegistration(null);
            sourceMapInfoPanel.Initialize(
                PreviewSourceMapZoom);
            BigDebugKnob.theKnob.AddListener(debugKnobChanged);
            BigDebugKnob.theKnob.debugFeaturesEnabled = false;
            enableDebugModeToolStripMenuItem.Visible = BuildConfig.theConfig.debugModeEnabled;
            debugModeToolStripSeparator.Visible = BuildConfig.theConfig.debugModeEnabled;
            if (startDocumentPath != null)
            {
                LoadMashup(Path.GetFullPath(startDocumentPath));
            }
            else
            {
                NewMashup();
            }

            if (renderOnLaunch)
            {
                currentMashup.AutoSelectMaxZooms(mapTileSourceFactory);
                LaunchRenderWindow();
                renderWindow.StartRender(
                    LaunchedRenderComplete);
                Shown += MainAppForm_Shown_BringRenderWindowToFront;
            }
        }

        private void debugKnobChanged(bool enabled)
        {
            try
            {
                var method = new DKCUI(debugKnobChanged_UI);
                Invoke(method, new object[] {enabled});
            }
            catch (InvalidOperationException)
            {
                debugKnobChanged_UI(enabled);
            }
        }

        private void debugKnobChanged_UI(bool enabled)
        {
            debugToolStripMenuItem.Visible = enabled;
            enableDebugModeToolStripMenuItem.Checked = enabled;
        }

        private void MainAppForm_Shown_BringRenderWindowToFront(object sender, EventArgs e)
        {
            renderWindow.BringToFront();
        }

        private void LaunchedRenderComplete(Exception failure)
        {
            if (!alreadyExiting)
            {
                var method = new ExitDelegate(LaunchedRenderComplete_ExitApplication);
                int num = failure == null ? 0 : 255;
                Invoke(method, new object[] {num});
            }
        }

        private void LaunchedRenderComplete_ExitApplication(int rc)
        {
            ProgramInstance.SetApplicationResultCode(rc);
            TeardownApplication();
            Application.Exit();
        }

        private void saveBackupTimer_Tick(object sender, EventArgs e)
        {
            if (currentMashup != null)
            {
                currentMashup.AutoSaveBackup();
            }
        }

        private int RobustGetFromRegistry(int defaultValue, string registryEntryName)
        {
            string value = backMakerRegistry.GetValue(registryEntryName);
            if (value != null)
            {
                return int.Parse(value);
            }

            return defaultValue;
        }

        private void RestoreWindowParameters()
        {
            try
            {
                programName = Text;
                if (backMakerRegistry.GetValue("gui_window_width") != null)
                {
                    var location = new Point(int.Parse(backMakerRegistry.GetValue("gui_window_x")),
                        int.Parse(backMakerRegistry.GetValue("gui_window_y")));
                    if (location.X > 0 && location.Y > 0)
                    {
                        Location = location;
                        Width = RobustGetFromRegistry(Width, "gui_window_width");
                        Height = RobustGetFromRegistry(Height, "gui_window_height");
                        controlSplitContainer.SplitterDistance =
                            RobustGetFromRegistry(controlSplitContainer.SplitterDistance,
                                "control_splitter_pos");
                        mapSplitContainer.SplitterDistance =
                            RobustGetFromRegistry(mapSplitContainer.SplitterDistance, "gui_splitter_pos");
                    }
                }
            }
            catch (Exception)
            {
            }

            recordSnapViewMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            recordSnapViewMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            restoreSnapViewMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            restoreSnapViewMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            recordSnapZoomMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            recordSnapZoomMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            restoreSnapZoomMenuItem.Visible = BuildConfig.theConfig.enableSnapFeatures;
            restoreSnapZoomMenuItem.Enabled = BuildConfig.theConfig.enableSnapFeatures;
            snapFeaturesToolStripSeparator.Visible = BuildConfig.theConfig.enableSnapFeatures;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CloseMashup())
            {
                e.Cancel = true;
                return;
            }

            TeardownApplication();
            Application.Exit();
            base.OnClosing(e);
        }

        private void TeardownApplication()
        {
            alreadyExiting = true;
            backMakerRegistry.SetValue("gui_window_width", Width.ToString());
            backMakerRegistry.SetValue("gui_window_height", Height.ToString());
            backMakerRegistry.SetValue("gui_window_x", Location.X.ToString());
            backMakerRegistry.SetValue("gui_window_y", Location.Y.ToString());
            backMakerRegistry.SetValue("control_splitter_pos",
                controlSplitContainer.SplitterDistance.ToString());
            backMakerRegistry.SetValue("gui_splitter_pos", mapSplitContainer.SplitterDistance.ToString());
            UndoConstruction();
        }

        public void UndoConstruction()
        {
            Monitor.Enter(this);
            try
            {
                if (!undone)
                {
                    KillRenderWindow();
                    cachePackage.Dispose();
                    renderedTileCachePackage.Dispose();
                    backMakerRegistry.Dispose();
                    undone = true;
                }
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        public void SetVEMapStyle(string s)
        {
            if (!VirtualEarthWebDownloader.StyleIsValid(s))
            {
                return;
            }

            if (uiPosition != null)
            {
                uiPosition.GetVEPos().setStyle(s);
            }

            VEroadView.Checked = s == VirtualEarthWebDownloader.RoadStyle;
            VEaerialView.Checked = s == VirtualEarthWebDownloader.AerialStyle;
            VEhybridView.Checked = s == VirtualEarthWebDownloader.HybridStyle;
            veViewerControl.SetBaseLayer(new VETileSource(GetCachePackage(), s));
        }

        private void roadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.RoadStyle);
        }

        private void aerialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.AerialStyle);
        }

        private void hybridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetVEMapStyle(VirtualEarthWebDownloader.HybridStyle);
        }

        private void AddRegLayerMenuItem_Click(object sender, EventArgs e)
        {
            var layerSelector =
                RenderedLayerSelector.GetLayerSelector(veViewerControl, renderedTileCachePackage);
            if (layerSelector != null)
            {
                foreach (var current in layerSelector.tsmiList)
                {
                    mapOptionsToolStripMenuItem2.DropDownItems.Add(current);
                }

                uiPosition.GetVEPos().setPosition(layerSelector.defaultView);
            }
        }

        private void aboutMSRBackMakerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutForm =
                new AboutForm(MapCruncher.MSR.CVE.BackMaker.Resources.Version.ApplicationVersionNumber);
            aboutForm.ShowDialog();
        }

        private void viewMapCruncherTutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo("hh.exe")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    "MapCruncher.chm")
            });
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CloseMashup())
            {
                TeardownApplication();
                Application.Exit();
            }
        }

        private void viewRenderedMenuItem_Click(object sender, EventArgs e)
        {
            var renderedMashupViewer =
                new RenderedMashupViewer(renderedTileCachePackage, showDMSMenuItem);
            renderedMashupViewer.Show();
        }

        public void AddNewAssociation(string newPinName)
        {
            D.Assert(!((SourceMapViewManager)currentView).MapsLocked());
            var positionAssociation = new PositionAssociation(newPinName,
                uiPosition.GetSMPos().llz,
                uiPosition.GetSMPos().llz,
                uiPosition.GetVEPos().llz,
                displayedRegistration.model.dirtyEvent);
            CheckForDuplicatePushpin(positionAssociation, -1);
            displayedRegistration.model.AddAssociation(positionAssociation);
            updateRegistrationDisplay();
        }

        public void UpdateAssociation(PositionAssociation assoc, string newName)
        {
            var newAssoc = new PositionAssociation("proposed",
                uiPosition.GetSMPos().llz,
                uiPosition.GetSMPos().llz,
                uiPosition.GetVEPos().llz,
                new DirtyEvent());
            CheckForDuplicatePushpin(newAssoc, assoc.pinId);
            assoc.UpdateAssociation(uiPosition.GetSMPos().llz, uiPosition.GetVEPos().llz);
            if (newName != null && newName != "")
            {
                assoc.associationName = newName;
            }

            updateRegistrationDisplay();
        }

        private void CheckForDuplicatePushpin(PositionAssociation newAssoc, int ignorePinId)
        {
            foreach (var current in displayedRegistration.model.GetAssociationList())
            {
                if (ignorePinId == -1 || ignorePinId != current.pinId)
                {
                    bool flag = current.globalPosition.pinPosition == newAssoc.globalPosition.pinPosition;
                    bool flag2 = current.imagePosition.pinPosition == newAssoc.imagePosition.pinPosition;
                    string text = "";
                    if (flag && flag2)
                    {
                        text = "reference and source";
                    }
                    else
                    {
                        if (flag)
                        {
                            text = "reference";
                        }
                        else
                        {
                            if (flag2)
                            {
                                text = "source";
                            }
                        }
                    }

                    if (text != "")
                    {
                        throw new DuplicatePushpinException(text, current.pinId, current.associationName);
                    }
                }
            }
        }

        public void RemoveAssociation(PositionAssociation assoc)
        {
            displayedRegistration.model.RemoveAssociation(assoc);
            updateRegistrationDisplay();
        }

        public void ViewAssociation(PositionAssociation pa)
        {
            uiPosition.GetSMPos().setPosition(pa.sourcePosition.pinPosition);
            uiPosition.GetVEPos().setPosition(pa.globalPosition.pinPosition);
            SetVEMapStyle(uiPosition.GetVEPos().style);
        }

        public void setDisplayedRegistration(RegistrationControlRecord display)
        {
            var oldSelectedPA = registrationControls.GetSelected();
            displayedRegistration = display;
            updateRegistrationDisplay();
            PositionAssociation selected = null;
            if (oldSelectedPA != null && displayedRegistration != null)
            {
                selected = displayedRegistration.model.GetAssociationList()
                    .Find((PositionAssociation pa) => pa.pinId == oldSelectedPA.pinId);
            }

            registrationControls.SetSelected(selected);
        }

        private void updateRegistrationDisplay()
        {
            if (displayedRegistration != null)
            {
                Converter<PositionAssociation, PositionAssociationView> converter = (PositionAssociation pa) =>
                    new PositionAssociationView(pa, PositionAssociationView.WhichPosition.global);
                veViewerControl.setPinList(displayedRegistration.model.GetAssociationList()
                    .ConvertAll(converter));
                Converter<PositionAssociation, PositionAssociationView> converter2 = (PositionAssociation pa) =>
                    new PositionAssociationView(pa, PositionAssociationView.WhichPosition.source);
                var pinList = displayedRegistration.model.GetAssociationList()
                    .ConvertAll(converter2);
                smViewerControl.setPinList(pinList);
            }
            else
            {
                veViewerControl.setPinList(new List<PositionAssociationView>());
                smViewerControl.setPinList(new List<PositionAssociationView>());
            }

            UpdateOverviewPins();
            registrationControls.DisplayModel(displayedRegistration);
        }

        private void EnableMashupInterfaceItems(bool enable)
        {
            saveMashupMenuItem.Enabled = enable;
            saveMashupAsMenuItem.Enabled = enable;
            closeMashupMenuItem.Enabled = enable;
            addSourceMapMenuItem.Enabled = enable;
            addSourceMapFromUriMenuItem.Enabled = enable;
            controlSplitContainer.Visible = enable;
        }

        private void updateWindowTitle()
        {
            if (currentMashup == null)
            {
                Text = programName;
                return;
            }

            Text = string.Format("{0} - {1}", currentMashup.GetDisplayName(), programName);
        }

        private void OpenMashup(Mashup newmash)
        {
            D.Assert(currentMashup == null);
            currentMashup = newmash;
            OpenView(new NothingLayerViewManager(this));
            EnableMashupInterfaceItems(true);
            updateWindowTitle();
            layerControls.SetMashup(currentMashup);
            currentMashup.readyToLockEvent.Add(ReadyToLockChangedHandler);
            ReadyToLockChanged();
        }

        private void ReadyToLockChangedHandler()
        {
            var method = new ReadyToLockChangedDelegate(ReadyToLockChanged);
            Invoke(method);
        }

        private void ReadyToLockChanged()
        {
            RenderLaunchButton.Enabled = currentMashup.SomeSourceMapIsReadyToLock();
        }

        private bool SaveMashup()
        {
            if (currentMashup.GetFilename() == null)
            {
                return SaveMashupAs();
            }

            try
            {
                currentMashup.WriteXML();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Can't save mashup {0}:\n{1}", currentMashup.GetFilename(), ex.Message),
                    "Error Writing Mashup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                return false;
            }

            return true;
        }

        private void saveMashupMenuItem_Click(object sender, EventArgs e)
        {
            SaveMashup();
        }

        private bool SaveMashupAs()
        {
            while (true)
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = string.Format("MapCruncher Mashup Files (*.{0})|*.{0}", "yum");
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.Title = "Enter new mashup filename";
                saveFileDialog.AddExtension = true;
                saveFileDialog.CheckFileExists = false;
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.OverwritePrompt = false;
                if (currentMashup.GetFilename() != null)
                {
                    saveFileDialog.FileName = currentMashup.GetDisplayName();
                }
                else
                {
                    if (currentMashup.layerList.Count > 0 && currentMashup.layerList.First.Count > 0)
                    {
                        saveFileDialog.FileName = currentMashup.layerList.First.First.GetDisplayName() + ".yum";
                    }
                }

                if (saveFileDialog.ShowDialog() != DialogResult.OK || saveFileDialog.FileName == null)
                {
                    break;
                }

                string text = saveFileDialog.FileName;
                if (Path.GetExtension(text) != ".yum")
                {
                    text += ".yum";
                }

                if (!File.Exists(text))
                {
                    goto IL_14A;
                }

                if (MessageBox.Show(string.Format("{0} already exists.\nDo you want to replace it?", text),
                        "Overwrite Existing File?",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    try
                    {
                        File.Delete(text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Can't overwrite {0}:\n{1}", text, ex.Message),
                            "Error Deleting Existing File",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Hand);
                        goto IL_15C;
                    }

                    goto IL_14A;
                }

                IL_15C:
                if (currentMashup.GetFilename() != null && SaveMashup())
                {
                    return true;
                }

                continue;
                IL_14A:
                currentMashup.SetFilename(text);
                updateWindowTitle();
                goto IL_15C;
            }

            return false;
        }

        private void saveMashupAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveMashupAs();
        }

        private bool CloseMashup()
        {
            if (currentMashup == null)
            {
                return true;
            }

            if (currentMashup.IsDirty())
            {
                string text;
                if (currentMashup.GetFilename() == null)
                {
                    text = "Save untitled mashup?";
                }
                else
                {
                    text = string.Format("Save changes to mashup {0}?", currentMashup.GetFilename());
                }

                var dialogResult = MessageBox.Show(text,
                    "Save changes?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Exclamation);
                if (dialogResult == DialogResult.Cancel)
                {
                    return false;
                }

                if (dialogResult == DialogResult.Yes && !SaveMashup())
                {
                    return false;
                }
            }

            currentMashup.Close();
            currentMashup = null;
            SetInterfaceNoMashupOpen();
            return true;
        }

        private void SetInterfaceNoMashupOpen()
        {
            SetOptionsPanelVisibility(OptionsPanelVisibility.Nothing);
            KillRenderWindow();
            layerControls.SetMashup(null);
            CloseView();
            EnableMashupInterfaceItems(false);
            updateWindowTitle();
        }

        private void LoadMashup(string fileName)
        {
            MashupFileWarningList mashupFileWarningList = null;
            Mashup mashup;
            try
            {
                mashup = Mashup.OpenMashupInteractive(fileName, out mashupFileWarningList);
                if (mashup == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Can't open {0}:\n{1}", fileName, ex.Message),
                    "Error Opening Mashup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Hand);
                return;
            }

            if (mashupFileWarningList != null)
            {
                var dialogResult =
                    MessageBox.Show(
                        string.Format("Warnings for {0}:\n{1}\nContinue loading file?\n",
                            fileName,
                            mashupFileWarningList),
                        "Error Reading File",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Hand);
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            }

            OpenMashup(mashup);
        }

        private void openMashupMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter =
                string.Format("MapCruncher Mashup Files (*.{0})|*.{0};*.msh" + BuildConfig.theConfig.allFilesOption,
                    "yum");
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (!CloseMashup())
            {
                return;
            }

            LoadMashup(openFileDialog.FileName);
        }

        private void NewMashup()
        {
            if (!CloseMashup())
            {
                return;
            }

            OpenMashup(new Mashup());
        }

        private void newMashupMenuItem_Click(object sender, EventArgs e)
        {
            NewMashup();
        }

        private void closeMashupMenuItem_Click(object sender, EventArgs e)
        {
            CloseMashup();
        }

        public void CloseView()
        {
            if (currentView != null)
            {
                currentView.Dispose();
                currentView = null;
                if (currentMashup != null)
                {
                    currentMashup.SetLastView(new NoView());
                }
            }
        }

        public void OpenView(IViewManager newView)
        {
            try
            {
                Opening obj;
                Monitor.Enter(obj = opening);
                try
                {
                    if (opening.opening)
                    {
                        D.Sayf(0, "Warning: recursive open", new object[0]);
                        return;
                    }

                    opening.opening = true;
                }
                finally
                {
                    Monitor.Exit(obj);
                }

                CloseView();
                ResetOverviewWindow();
                D.Assert(currentView == null);
                currentView = newView;
                currentView.Activate();
                layerControls.SelectObject(newView.GetViewedObject());
            }
            finally
            {
                Opening obj2;
                Monitor.Enter(obj2 = opening);
                try
                {
                    opening.opening = false;
                }
                finally
                {
                    Monitor.Exit(obj2);
                }
            }
        }

        [DllImport("user32")]
        private static extern bool SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        public void OpenSourceMap(SourceMap sourceMap)
        {
            FreezePainting = true;
            DefaultReferenceView drv;
            if (currentView != null)
            {
                drv = new DefaultReferenceView(uiPosition.GetVEPos().llz);
            }
            else
            {
                drv = new DefaultReferenceView();
            }

            var sourceMapViewManager =
                new SourceMapViewManager(sourceMap, mapTileSourceFactory, this, drv);
            OpenView(sourceMapViewManager);
            FreezePainting = false;
            currentMashup.SetLastView(sourceMap.lastView);
            SetupOverviewWindow(sourceMapViewManager);
        }

        public void OpenLayer(Layer layer)
        {
            OpenView(new DynamicallyCompositingLayerViewManager(layer, mapTileSourceFactory, this));
            currentMashup.SetLastView(layer.lastView);
        }

        public void OpenLegend(Legend legend)
        {
            OpenView(new LegendViewManager(legend, mapTileSourceFactory, this));
            currentMashup.SetLastView(legend.lastView);
        }

        private void addSourceMapMenuItem_Click(object sender, EventArgs e)
        {
            AddSourceMap();
        }

        private void addSourceMapFromUriMenuItem_Click(object sender, EventArgs e)
        {
            AddSourceMapFromUri();
        }

        public void AddSourceMap()
        {
            var openFileDialog = new OpenFileDialog();
            string arg = string.Join(";",
                Array.ConvertAll(mapTileSourceFactory.GetKnownFileTypes(),
                    (string ext) => "*" + ext));
            string filter = string.Format("Supported Sources ({0})|{0}" + BuildConfig.theConfig.allFilesOption, arg);
            openFileDialog.Filter = filter;
            openFileDialog.FilterIndex = 1;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (openFileDialog.FileName == null)
            {
                return;
            }

            var undoAddSourceMap = new UndoAddSourceMap(openFileDialog.FileName, null, null, null, this);
            try
            {
                var fileStream = File.Open(openFileDialog.FileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);
                fileStream.Close();
                var sourceMap = new SourceMap(new FutureDocumentFromFilesystem(openFileDialog.FileName, 0),
                    currentMashup.GetFilenameContext,
                    currentMashup.dirtyEvent,
                    currentMashup.readyToLockEvent);
                var addedToLayer = layerControls.AddSourceMap(sourceMap);
                undoAddSourceMap = new UndoAddSourceMap(openFileDialog.FileName,
                    sourceMap,
                    addedToLayer,
                    layerControls,
                    this);
                new InsaneSourceMapRemover(sourceMap,
                    mapTileSourceFactory,
                    undoAddSourceMap.Undo);
                OpenSourceMap(sourceMap);
            }
            catch (Exception ex)
            {
                undoAddSourceMap.Undo(ex.Message);
            }
        }

        public void AddSourceMapFromUri()
        {
            var sourceMap = new SourceMap(
                new FutureDocumentFromUri(new Uri("http://www.srh.noaa.gov/ridge/lite/NCR/ATX_0.png"), 0),
                currentMashup.GetFilenameContext,
                currentMashup.dirtyEvent,
                currentMashup.readyToLockEvent);
            layerControls.AddSourceMap(sourceMap);
            OpenSourceMap(sourceMap);
        }

        public void RemoveSourceMap(SourceMap sourceMap)
        {
            currentMashup.layerList.RemoveSourceMap(sourceMap);
        }

        public void LaunchRenderedBrowser(Uri uri)
        {
            var renderedMashupViewer =
                new RenderedMashupViewer(renderedTileCachePackage, showDMSMenuItem);
            renderedMashupViewer.AddLayersFromUri(uri);
            renderedMashupViewer.Show();
        }

        public UIPositionManager GetUIPositionManager()
        {
            return uiPosition;
        }

        public ViewerControlIfc GetSMViewerControl()
        {
            return smViewerControl;
        }

        public ViewerControl GetVEViewerControl()
        {
            return veViewerControl;
        }

        public SourceMapInfoPanel GetSourceMapInfoPanel()
        {
            return sourceMapInfoPanel;
        }

        public TransparencyPanel GetTransparencyPanel()
        {
            return transparencyPanel;
        }

        public LegendOptionsPanel GetLegendPanel()
        {
            return legendOptionsPanel1;
        }

        public void SetOptionsPanelVisibility(OptionsPanelVisibility optionsPanelVisibility)
        {
            if (optionsPanelVisibility == OptionsPanelVisibility.Nothing)
            {
                synergyExplorer.Visible = false;
                return;
            }

            if (optionsPanelVisibility == OptionsPanelVisibility.SourceMapOptions)
            {
                synergyExplorer.TabPages.Clear();
                synergyExplorer.TabPages.Add(correspondencesTab);
                synergyExplorer.TabPages.Add(sourceInfoTab);
                synergyExplorer.TabPages.Add(transparencyTab);
                synergyExplorer.Visible = true;
                return;
            }

            if (optionsPanelVisibility == OptionsPanelVisibility.LegendOptions)
            {
                synergyExplorer.TabPages.Clear();
                synergyExplorer.TabPages.Add(legendTabPage);
                synergyExplorer.Visible = true;
            }
        }

        public CachePackage GetCachePackage()
        {
            return cachePackage;
        }

        public void LockMaps()
        {
            ((SourceMapViewManager)currentView).LockMaps();
            smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.LatLon);
        }

        public void UnlockMaps()
        {
            ((SourceMapViewManager)currentView).UnlockMaps();
            smViewerControl.SetLLZBoxLabelStyle(LLZBox.LabelStyle.XY);
        }

        public void SetDocumentMutable(bool mutable)
        {
            documentIsMutable = mutable;
            synergyExplorer.Enabled = mutable;
        }

        public bool GetDocumentMutable()
        {
            return documentIsMutable;
        }

        private void PreviewSourceMapZoom(SourceMap sourceMap)
        {
            if (currentView is SourceMapViewManager &&
                ((SourceMapViewManager)currentView).GetSourceMap() == sourceMap)
            {
                ((SourceMapViewManager)currentView).PreviewSourceMapZoom();
            }
        }

        private void LaunchRenderWindow()
        {
            if (renderWindow == null)
            {
                renderWindow = new RenderWindow();
                renderWindow.Disposed += renderWindow_Disposed;
            }

            renderWindow.Setup(currentMashup.GetRenderOptions(),
                currentMashup,
                mapTileSourceFactory,
                LaunchRenderedBrowser,
                flushRenderedTileCachePackage);
            renderWindow.Visible = true;
        }

        private void flushRenderedTileCachePackage()
        {
            renderedTileCachePackage.Flush();
        }

        private void renderWindow_Disposed(object sender, EventArgs e)
        {
            renderWindow = null;
        }

        private void KillRenderWindow()
        {
            if (renderWindow != null)
            {
                renderWindow.UndoConstruction();
            }
        }

        private void RenderLaunchButton_Click(object sender, EventArgs e)
        {
            if (renderWindow == null)
            {
                LaunchRenderWindow();
            }

            renderWindow.BringToFront();
        }

        private void showSourceMapOverviewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
            bool @checked = ((ToolStripMenuItem)sender).Checked;
            if (@checked && sourceMapOverviewWindow == null)
            {
                sourceMapOverviewWindow = new SourceMapOverviewWindow();
                sourceMapOverviewWindow.Initialize(
                    SourceMapOverviewWindowClosed,
                    new MapDrawingOption(veViewerControl, showDMSMenuItem, false));
                sourceMapOverviewWindow.viewerControl.ShowPushPins =
                    new MapDrawingOption(sourceMapOverviewWindow.viewerControl, showPushPinsMenuItem, true);
                sourceMapOverviewWindow.viewerControl.ShowSourceCrop = new MapDrawingOption(
                    sourceMapOverviewWindow.viewerControl,
                    showSourceCropToolStripMenuItem,
                    true);
                sourceMapOverviewWindow.viewerControl.ShowDMS =
                    new MapDrawingOption(sourceMapOverviewWindow.viewerControl, showDMSMenuItem, false);
                sourceMapOverviewWindow.Show();
                if (currentView is SourceMapViewManager)
                {
                    SetupOverviewWindow((SourceMapViewManager)currentView);
                    return;
                }
            }
            else
            {
                if (!@checked && sourceMapOverviewWindow != null)
                {
                    sourceMapOverviewWindow.Close();
                    sourceMapOverviewWindow = null;
                }
            }
        }

        private void UpdateOverviewPins()
        {
            if (sourceMapOverviewWindow != null)
            {
                List<PositionAssociationView> pinList;
                if (displayedRegistration != null)
                {
                    Converter<PositionAssociation, PositionAssociationView> converter = (PositionAssociation pa) =>
                        new PositionAssociationView(pa, PositionAssociationView.WhichPosition.image);
                    pinList = new RegistrationDefinition(displayedRegistration.model, new DirtyEvent())
                    {
                        isLocked = false
                    }.GetAssociationList().ConvertAll(converter);
                }
                else
                {
                    pinList = new List<PositionAssociationView>();
                }

                sourceMapOverviewWindow.viewerControl.setPinList(pinList);
            }
        }

        private void ResetOverviewWindow()
        {
            if (sourceMapOverviewWindow != null)
            {
                sourceMapOverviewWindow.viewerControl.ClearLayers();
                sourceMapOverviewWindow.viewerControl.setPinList(new List<PositionAssociationView>());
            }
        }

        private void SetupOverviewWindow(SourceMapViewManager smvm)
        {
            if (sourceMapOverviewWindow != null)
            {
                smvm.UpdateOverviewWindow(sourceMapOverviewWindow.viewerControl);
            }

            updateRegistrationDisplay();
        }

        public void SourceMapOverviewWindowClosed()
        {
            sourceMapOverviewWindow = null;
            showSourceMapOverviewMenuItem.Checked = false;
        }

        private void recordSnapViewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            smViewerControl.RecordSnapView();
        }

        private void restoreSnapViewMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            smViewerControl.RestoreSnapView();
        }

        private void recordSnapZoomMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            smViewerControl.RecordSnapZoom();
        }

        private void restoreSnapZoomMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = false;
            smViewerControl.RestoreSnapZoom();
        }

        private void showDiagnosticsUIToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DiagnosticUI.theDiagnostics.Visible = true;
        }

        private void enableDebugModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BigDebugKnob.theKnob.debugFeaturesEnabled = !BigDebugKnob.theKnob.debugFeaturesEnabled;
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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newMashupMenuItem = new ToolStripMenuItem();
            openMashupMenuItem = new ToolStripMenuItem();
            saveMashupMenuItem = new ToolStripMenuItem();
            saveMashupAsMenuItem = new ToolStripMenuItem();
            closeMashupMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            addSourceMapMenuItem = new ToolStripMenuItem();
            addSourceMapFromUriMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            viewRenderedMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            mapOptionsToolStripMenuItem2 = new ToolStripMenuItem();
            VEroadView = new ToolStripMenuItem();
            VEaerialView = new ToolStripMenuItem();
            VEhybridView = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            showCrosshairsMenuItem = new ToolStripMenuItem();
            showPushPinsMenuItem = new ToolStripMenuItem();
            showDMSMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            AddRegLayerMenuItem = new ToolStripMenuItem();
            showSourceMapOverviewMenuItem = new ToolStripMenuItem();
            snapFeaturesToolStripSeparator = new ToolStripSeparator();
            restoreSnapViewMenuItem = new ToolStripMenuItem();
            recordSnapViewMenuItem = new ToolStripMenuItem();
            restoreSnapZoomMenuItem = new ToolStripMenuItem();
            recordSnapZoomMenuItem = new ToolStripMenuItem();
            debugModeToolStripSeparator = new ToolStripSeparator();
            enableDebugModeToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            viewMapCruncherTutorialToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            aboutMSRBackMakerToolStripMenuItem = new ToolStripMenuItem();
            debugToolStripMenuItem = new ToolStripMenuItem();
            showTileNamesMenuItem = new ToolStripMenuItem();
            showSourceCropToolStripMenuItem = new ToolStripMenuItem();
            showTileBoundariesMenuItem = new ToolStripMenuItem();
            showDiagnosticsUIToolStripMenuItem = new ToolStripMenuItem();
            mapSplitContainer = new SplitContainer();
            smViewerControl = new ViewerControl();
            veViewerControl = new ViewerControl();
            controlSplitContainer = new SplitContainer();
            panel1 = new Panel();
            RenderLaunchButton = new Button();
            controlsSplitContainer = new SplitContainer();
            layerControls = new LayerControls();
            synergyExplorer = new TabControl();
            correspondencesTab = new TabPage();
            registrationControls = new registrationControls();
            transparencyTab = new TabPage();
            transparencyPanel = new TransparencyPanel();
            sourceInfoTab = new TabPage();
            sourceMapInfoPanel = new SourceMapInfoPanel();
            legendTabPage = new TabPage();
            legendOptionsPanel1 = new LegendOptionsPanel();
            menuStrip1.SuspendLayout();
            ((ISupportInitialize)mapSplitContainer).BeginInit();
            mapSplitContainer.Panel1.SuspendLayout();
            mapSplitContainer.Panel2.SuspendLayout();
            mapSplitContainer.SuspendLayout();
            ((ISupportInitialize)controlSplitContainer).BeginInit();
            controlSplitContainer.Panel1.SuspendLayout();
            controlSplitContainer.Panel2.SuspendLayout();
            controlSplitContainer.SuspendLayout();
            panel1.SuspendLayout();
            ((ISupportInitialize)controlsSplitContainer).BeginInit();
            controlsSplitContainer.Panel1.SuspendLayout();
            controlsSplitContainer.Panel2.SuspendLayout();
            controlsSplitContainer.SuspendLayout();
            synergyExplorer.SuspendLayout();
            correspondencesTab.SuspendLayout();
            transparencyTab.SuspendLayout();
            sourceInfoTab.SuspendLayout();
            legendTabPage.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[]
            {
                fileToolStripMenuItem, mapOptionsToolStripMenuItem2, helpToolStripMenuItem,
                debugToolStripMenuItem
            });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1028, 36);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                newMashupMenuItem, openMashupMenuItem, saveMashupMenuItem, saveMashupAsMenuItem,
                closeMashupMenuItem, toolStripSeparator1, addSourceMapMenuItem,
                addSourceMapFromUriMenuItem, toolStripSeparator4, viewRenderedMenuItem,
                toolStripSeparator2, exitToolStripMenuItem
            });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(54, 32);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newMashupMenuItem
            // 
            newMashupMenuItem.Name = "newMashupMenuItem";
            newMashupMenuItem.Size = new Size(306, 32);
            newMashupMenuItem.Text = "&New Mashup";
            newMashupMenuItem.Click += newMashupMenuItem_Click;
            // 
            // openMashupMenuItem
            // 
            openMashupMenuItem.Name = "openMashupMenuItem";
            openMashupMenuItem.Size = new Size(306, 32);
            openMashupMenuItem.Text = "&Open Mashup...";
            openMashupMenuItem.Click += openMashupMenuItem_Click;
            // 
            // saveMashupMenuItem
            // 
            saveMashupMenuItem.Enabled = false;
            saveMashupMenuItem.Name = "saveMashupMenuItem";
            saveMashupMenuItem.Size = new Size(306, 32);
            saveMashupMenuItem.Text = "&Save Mashup";
            saveMashupMenuItem.Click += saveMashupMenuItem_Click;
            // 
            // saveMashupAsMenuItem
            // 
            saveMashupAsMenuItem.Enabled = false;
            saveMashupAsMenuItem.Name = "saveMashupAsMenuItem";
            saveMashupAsMenuItem.Size = new Size(306, 32);
            saveMashupAsMenuItem.Text = "Save Mashup &As...";
            saveMashupAsMenuItem.Click += saveMashupAsMenuItem_Click;
            // 
            // closeMashupMenuItem
            // 
            closeMashupMenuItem.Enabled = false;
            closeMashupMenuItem.Name = "closeMashupMenuItem";
            closeMashupMenuItem.Size = new Size(306, 32);
            closeMashupMenuItem.Text = "&Close Mashup";
            closeMashupMenuItem.Click += closeMashupMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(303, 6);
            // 
            // addSourceMapMenuItem
            // 
            addSourceMapMenuItem.Enabled = false;
            addSourceMapMenuItem.Name = "addSourceMapMenuItem";
            addSourceMapMenuItem.Size = new Size(306, 32);
            addSourceMapMenuItem.Text = "Add Source &Map...";
            addSourceMapMenuItem.Click += addSourceMapMenuItem_Click;
            // 
            // addSourceMapFromUriMenuItem
            // 
            addSourceMapFromUriMenuItem.Enabled = false;
            addSourceMapFromUriMenuItem.Name = "addSourceMapFromUriMenuItem";
            addSourceMapFromUriMenuItem.Size = new Size(306, 32);
            addSourceMapFromUriMenuItem.Text = "Add Map From &Uri...";
            addSourceMapFromUriMenuItem.Visible = false;
            addSourceMapFromUriMenuItem.Click += addSourceMapFromUriMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(303, 6);
            // 
            // viewRenderedMenuItem
            // 
            viewRenderedMenuItem.Name = "viewRenderedMenuItem";
            viewRenderedMenuItem.Size = new Size(306, 32);
            viewRenderedMenuItem.Text = "Launch Mashup &Browser...";
            viewRenderedMenuItem.Click += viewRenderedMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(303, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(306, 32);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // mapOptionsToolStripMenuItem2
            // 
            mapOptionsToolStripMenuItem2.DropDownItems.AddRange(new ToolStripItem[]
            {
                VEroadView, VEaerialView, VEhybridView, toolStripSeparator3,
                showCrosshairsMenuItem, showPushPinsMenuItem, showDMSMenuItem,
                toolStripSeparator8, AddRegLayerMenuItem, showSourceMapOverviewMenuItem,
                snapFeaturesToolStripSeparator, restoreSnapViewMenuItem, recordSnapViewMenuItem,
                restoreSnapZoomMenuItem, recordSnapZoomMenuItem, debugModeToolStripSeparator,
                enableDebugModeToolStripMenuItem
            });
            mapOptionsToolStripMenuItem2.Name = "mapOptionsToolStripMenuItem2";
            mapOptionsToolStripMenuItem2.Size = new Size(65, 32);
            mapOptionsToolStripMenuItem2.Text = "&View";
            // 
            // VEroadView
            // 
            VEroadView.Name = "VEroadView";
            VEroadView.Size = new Size(334, 32);
            VEroadView.Text = "&Roads";
            VEroadView.Click += roadToolStripMenuItem_Click;
            // 
            // VEaerialView
            // 
            VEaerialView.Name = "VEaerialView";
            VEaerialView.Size = new Size(334, 32);
            VEaerialView.Text = "&Aerial Photos";
            VEaerialView.Click += aerialToolStripMenuItem_Click;
            // 
            // VEhybridView
            // 
            VEhybridView.Name = "VEhybridView";
            VEhybridView.Size = new Size(334, 32);
            VEhybridView.Text = "&Hybrid";
            VEhybridView.Click += hybridToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(331, 6);
            // 
            // showCrosshairsMenuItem
            // 
            showCrosshairsMenuItem.Name = "showCrosshairsMenuItem";
            showCrosshairsMenuItem.Size = new Size(334, 32);
            showCrosshairsMenuItem.Text = "Show &Crosshairs";
            // 
            // showPushPinsMenuItem
            // 
            showPushPinsMenuItem.Name = "showPushPinsMenuItem";
            showPushPinsMenuItem.Size = new Size(334, 32);
            showPushPinsMenuItem.Text = "Show &PushPins";
            // 
            // showDMSMenuItem
            // 
            showDMSMenuItem.Name = "showDMSMenuItem";
            showDMSMenuItem.Size = new Size(334, 32);
            showDMSMenuItem.Text = "Show locations in d°m\'s\"";
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(331, 6);
            // 
            // AddRegLayerMenuItem
            // 
            AddRegLayerMenuItem.Name = "AddRegLayerMenuItem";
            AddRegLayerMenuItem.Size = new Size(334, 32);
            AddRegLayerMenuItem.Text = "Show Rendered &Layer...";
            AddRegLayerMenuItem.Click += AddRegLayerMenuItem_Click;
            // 
            // showSourceMapOverviewMenuItem
            // 
            showSourceMapOverviewMenuItem.Name = "showSourceMapOverviewMenuItem";
            showSourceMapOverviewMenuItem.Size = new Size(334, 32);
            showSourceMapOverviewMenuItem.Text = "Show Source Map Overview";
            showSourceMapOverviewMenuItem.Click += showSourceMapOverviewMenuItem_Click;
            // 
            // snapFeaturesToolStripSeparator
            // 
            snapFeaturesToolStripSeparator.Name = "snapFeaturesToolStripSeparator";
            snapFeaturesToolStripSeparator.Size = new Size(331, 6);
            // 
            // restoreSnapViewMenuItem
            // 
            restoreSnapViewMenuItem.Name = "restoreSnapViewMenuItem";
            restoreSnapViewMenuItem.ShortcutKeyDisplayString = "F5";
            restoreSnapViewMenuItem.ShortcutKeys = Keys.F5;
            restoreSnapViewMenuItem.Size = new Size(334, 32);
            restoreSnapViewMenuItem.Text = "Restore SnapView";
            // 
            // recordSnapViewMenuItem
            // 
            recordSnapViewMenuItem.Name = "recordSnapViewMenuItem";
            recordSnapViewMenuItem.ShortcutKeyDisplayString = "Shift+F5";
            recordSnapViewMenuItem.Size = new Size(334, 32);
            recordSnapViewMenuItem.Text = "Record SnapView";
            // 
            // restoreSnapZoomMenuItem
            // 
            restoreSnapZoomMenuItem.Name = "restoreSnapZoomMenuItem";
            restoreSnapZoomMenuItem.ShortcutKeyDisplayString = "F6";
            restoreSnapZoomMenuItem.Size = new Size(334, 32);
            restoreSnapZoomMenuItem.Text = "Restore SnapZoom";
            // 
            // recordSnapZoomMenuItem
            // 
            recordSnapZoomMenuItem.Name = "recordSnapZoomMenuItem";
            recordSnapZoomMenuItem.ShortcutKeyDisplayString = "Shift+F6";
            recordSnapZoomMenuItem.Size = new Size(334, 32);
            recordSnapZoomMenuItem.Text = "Record SnapZoom";
            // 
            // debugModeToolStripSeparator
            // 
            debugModeToolStripSeparator.Name = "debugModeToolStripSeparator";
            debugModeToolStripSeparator.Size = new Size(331, 6);
            // 
            // enableDebugModeToolStripMenuItem
            // 
            enableDebugModeToolStripMenuItem.Name = "enableDebugModeToolStripMenuItem";
            enableDebugModeToolStripMenuItem.Size = new Size(334, 32);
            enableDebugModeToolStripMenuItem.Text = "Enable Debug Mode";
            enableDebugModeToolStripMenuItem.Click +=
                enableDebugModeToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                viewMapCruncherTutorialToolStripMenuItem, toolStripSeparator7,
                aboutMSRBackMakerToolStripMenuItem
            });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(65, 32);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // viewMapCruncherTutorialToolStripMenuItem
            // 
            viewMapCruncherTutorialToolStripMenuItem.Name = "viewMapCruncherTutorialToolStripMenuItem";
            viewMapCruncherTutorialToolStripMenuItem.Size = new Size(536, 32);
            viewMapCruncherTutorialToolStripMenuItem.Text = "MapCruncher for Microsoft Virtual Earth Help";
            viewMapCruncherTutorialToolStripMenuItem.Click +=
                viewMapCruncherTutorialToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(533, 6);
            // 
            // aboutMSRBackMakerToolStripMenuItem
            // 
            aboutMSRBackMakerToolStripMenuItem.Name = "aboutMSRBackMakerToolStripMenuItem";
            aboutMSRBackMakerToolStripMenuItem.Size = new Size(536, 32);
            aboutMSRBackMakerToolStripMenuItem.Text = "&About MapCruncher Beta for Microsoft Virtual Earth";
            aboutMSRBackMakerToolStripMenuItem.Click +=
                aboutMSRBackMakerToolStripMenuItem_Click;
            // 
            // debugToolStripMenuItem
            // 
            debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                showTileNamesMenuItem, showSourceCropToolStripMenuItem, showTileBoundariesMenuItem,
                showDiagnosticsUIToolStripMenuItem
            });
            debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            debugToolStripMenuItem.Size = new Size(83, 32);
            debugToolStripMenuItem.Text = "&Debug";
            // 
            // showTileNamesMenuItem
            // 
            showTileNamesMenuItem.Name = "showTileNamesMenuItem";
            showTileNamesMenuItem.Size = new Size(269, 32);
            showTileNamesMenuItem.Text = "Show Tile &Names";
            // 
            // showSourceCropToolStripMenuItem
            // 
            showSourceCropToolStripMenuItem.Name = "showSourceCropToolStripMenuItem";
            showSourceCropToolStripMenuItem.Size = new Size(269, 32);
            showSourceCropToolStripMenuItem.Text = "Show Source Crop";
            // 
            // showTileBoundariesMenuItem
            // 
            showTileBoundariesMenuItem.Name = "showTileBoundariesMenuItem";
            showTileBoundariesMenuItem.Size = new Size(269, 32);
            showTileBoundariesMenuItem.Text = "Show Tile &Boundaries";
            // 
            // showDiagnosticsUIToolStripMenuItem
            // 
            showDiagnosticsUIToolStripMenuItem.Name = "showDiagnosticsUIToolStripMenuItem";
            showDiagnosticsUIToolStripMenuItem.Size = new Size(269, 32);
            showDiagnosticsUIToolStripMenuItem.Text = "Show DiagnosticsUI";
            showDiagnosticsUIToolStripMenuItem.Click +=
                showDiagnosticsUIToolStripMenuItem_Click_1;
            // 
            // mapSplitContainer
            // 
            mapSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                        | AnchorStyles.Left
                                                        | AnchorStyles.Right;
            mapSplitContainer.Location = new Point(3, 3);
            mapSplitContainer.Name = "mapSplitContainer";
            // 
            // mapSplitContainer.Panel1
            // 
            mapSplitContainer.Panel1.Controls.Add(smViewerControl);
            mapSplitContainer.Panel1MinSize = 100;
            // 
            // mapSplitContainer.Panel2
            // 
            mapSplitContainer.Panel2.Controls.Add(veViewerControl);
            mapSplitContainer.Panel2MinSize = 100;
            mapSplitContainer.Size = new Size(691, 647);
            mapSplitContainer.SplitterDistance = 337;
            mapSplitContainer.TabIndex = 6;
            // 
            // smViewerControl
            // 
            smViewerControl.Dock = DockStyle.Fill;
            smViewerControl.Location = new Point(0, 0);
            smViewerControl.Margin = new Padding(4, 4, 4, 4);
            smViewerControl.Name = "smViewerControl";
            smViewerControl.Size = new Size(337, 647);
            smViewerControl.TabIndex = 0;
            // 
            // veViewerControl
            // 
            veViewerControl.Dock = DockStyle.Fill;
            veViewerControl.Location = new Point(0, 0);
            veViewerControl.Margin = new Padding(4, 4, 4, 4);
            veViewerControl.Name = "veViewerControl";
            veViewerControl.Size = new Size(350, 647);
            veViewerControl.TabIndex = 0;
            // 
            // controlSplitContainer
            // 
            controlSplitContainer.Dock = DockStyle.Fill;
            controlSplitContainer.Location = new Point(0, 36);
            controlSplitContainer.Name = "controlSplitContainer";
            // 
            // controlSplitContainer.Panel1
            // 
            controlSplitContainer.Panel1.Controls.Add(panel1);
            controlSplitContainer.Panel1.Controls.Add(controlsSplitContainer);
            // 
            // controlSplitContainer.Panel2
            // 
            controlSplitContainer.Panel2.Controls.Add(mapSplitContainer);
            controlSplitContainer.Size = new Size(1028, 650);
            controlSplitContainer.SplitterDistance = 330;
            controlSplitContainer.TabIndex = 7;
            // 
            // panel1
            // 
            panel1.Controls.Add(RenderLaunchButton);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 608);
            panel1.Name = "panel1";
            panel1.Size = new Size(330, 42);
            panel1.TabIndex = 9;
            // 
            // RenderLaunchButton
            // 
            RenderLaunchButton.Location = new Point(3, 4);
            RenderLaunchButton.Name = "RenderLaunchButton";
            RenderLaunchButton.Size = new Size(125, 30);
            RenderLaunchButton.TabIndex = 9;
            RenderLaunchButton.Text = "Render...";
            RenderLaunchButton.UseVisualStyleBackColor = true;
            RenderLaunchButton.Click += RenderLaunchButton_Click;
            // 
            // controlsSplitContainer
            // 
            controlsSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom
                                                             | AnchorStyles.Left
                                                             | AnchorStyles.Right;
            controlsSplitContainer.Location = new Point(0, 0);
            controlsSplitContainer.Name = "controlsSplitContainer";
            controlsSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // controlsSplitContainer.Panel1
            // 
            controlsSplitContainer.Panel1.Controls.Add(layerControls);
            // 
            // controlsSplitContainer.Panel2
            // 
            controlsSplitContainer.Panel2.Controls.Add(synergyExplorer);
            controlsSplitContainer.Size = new Size(327, 606);
            controlsSplitContainer.SplitterDistance = 130;
            controlsSplitContainer.TabIndex = 8;
            // 
            // layerControls
            // 
            layerControls.Dock = DockStyle.Fill;
            layerControls.Location = new Point(0, 0);
            layerControls.Margin = new Padding(4, 4, 4, 4);
            layerControls.Name = "layerControls";
            layerControls.Size = new Size(327, 130);
            layerControls.TabIndex = 10;
            // 
            // synergyExplorer
            // 
            synergyExplorer.Controls.Add(correspondencesTab);
            synergyExplorer.Controls.Add(transparencyTab);
            synergyExplorer.Controls.Add(sourceInfoTab);
            synergyExplorer.Controls.Add(legendTabPage);
            synergyExplorer.Dock = DockStyle.Fill;
            synergyExplorer.Location = new Point(0, 0);
            synergyExplorer.Multiline = true;
            synergyExplorer.Name = "synergyExplorer";
            synergyExplorer.SelectedIndex = 0;
            synergyExplorer.ShowToolTips = true;
            synergyExplorer.Size = new Size(327, 472);
            synergyExplorer.TabIndex = 7;
            // 
            // correspondencesTab
            // 
            correspondencesTab.Controls.Add(registrationControls);
            correspondencesTab.Location = new Point(4, 46);
            correspondencesTab.Name = "correspondencesTab";
            correspondencesTab.Padding = new Padding(3);
            correspondencesTab.Size = new Size(319, 422);
            correspondencesTab.TabIndex = 1;
            correspondencesTab.Text = "Correspondences";
            correspondencesTab.UseVisualStyleBackColor = true;
            // 
            // registrationControls
            // 
            registrationControls.Dock = DockStyle.Fill;
            registrationControls.Location = new Point(3, 3);
            registrationControls.Margin = new Padding(4, 4, 4, 4);
            registrationControls.Name = "registrationControls";
            registrationControls.Size = new Size(313, 416);
            registrationControls.TabIndex = 9;
            // 
            // transparencyTab
            // 
            transparencyTab.Controls.Add(transparencyPanel);
            transparencyTab.Location = new Point(4, 46);
            transparencyTab.Name = "transparencyTab";
            transparencyTab.Size = new Size(319, 412);
            transparencyTab.TabIndex = 4;
            transparencyTab.Text = "Transparency";
            transparencyTab.UseVisualStyleBackColor = true;
            // 
            // transparencyPanel
            // 
            transparencyPanel.Dock = DockStyle.Fill;
            transparencyPanel.Location = new Point(0, 0);
            transparencyPanel.Margin = new Padding(4, 4, 4, 4);
            transparencyPanel.Name = "transparencyPanel";
            transparencyPanel.Size = new Size(319, 412);
            transparencyPanel.TabIndex = 0;
            // 
            // sourceInfoTab
            // 
            sourceInfoTab.Controls.Add(sourceMapInfoPanel);
            sourceInfoTab.Location = new Point(4, 46);
            sourceInfoTab.Name = "sourceInfoTab";
            sourceInfoTab.Padding = new Padding(3);
            sourceInfoTab.Size = new Size(319, 412);
            sourceInfoTab.TabIndex = 3;
            sourceInfoTab.Text = "Source Info";
            sourceInfoTab.UseVisualStyleBackColor = true;
            // 
            // sourceMapInfoPanel
            // 
            sourceMapInfoPanel.Dock = DockStyle.Fill;
            sourceMapInfoPanel.Location = new Point(3, 3);
            sourceMapInfoPanel.Margin = new Padding(4, 4, 4, 4);
            sourceMapInfoPanel.Name = "sourceMapInfoPanel";
            sourceMapInfoPanel.Size = new Size(313, 406);
            sourceMapInfoPanel.TabIndex = 0;
            // 
            // legendTabPage
            // 
            legendTabPage.Controls.Add(legendOptionsPanel1);
            legendTabPage.Location = new Point(4, 46);
            legendTabPage.Name = "legendTabPage";
            legendTabPage.Padding = new Padding(3);
            legendTabPage.Size = new Size(319, 412);
            legendTabPage.TabIndex = 5;
            legendTabPage.Text = "Legend Options";
            legendTabPage.UseVisualStyleBackColor = true;
            // 
            // legendOptionsPanel1
            // 
            legendOptionsPanel1.Dock = DockStyle.Fill;
            legendOptionsPanel1.Location = new Point(3, 3);
            legendOptionsPanel1.Margin = new Padding(4, 4, 4, 4);
            legendOptionsPanel1.Name = "legendOptionsPanel1";
            legendOptionsPanel1.Size = new Size(313, 406);
            legendOptionsPanel1.TabIndex = 10;
            // 
            // MainAppForm
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            ClientSize = new Size(1028, 686);
            Controls.Add(controlSplitContainer);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainAppForm";
            Text = "MapCruncher Beta for Microsoft Virtual Earth";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            mapSplitContainer.Panel1.ResumeLayout(false);
            mapSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)mapSplitContainer).EndInit();
            mapSplitContainer.ResumeLayout(false);
            controlSplitContainer.Panel1.ResumeLayout(false);
            controlSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)controlSplitContainer).EndInit();
            controlSplitContainer.ResumeLayout(false);
            panel1.ResumeLayout(false);
            controlsSplitContainer.Panel1.ResumeLayout(false);
            controlsSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)controlsSplitContainer).EndInit();
            controlsSplitContainer.ResumeLayout(false);
            synergyExplorer.ResumeLayout(false);
            correspondencesTab.ResumeLayout(false);
            transparencyTab.ResumeLayout(false);
            sourceInfoTab.ResumeLayout(false);
            legendTabPage.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
