using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class Mashup
    {
        private const string mashupFileTag = "MapGrinderMashupFile";
        private bool dirty;
        private bool autoSaveDirty;
        private bool autoSaveFailNotified;
        public DirtyEvent dirtyEvent = new DirtyEvent();
        public DirtyEvent readyToLockEvent = new DirtyEvent();
        private string fileName;
        private RenderOptions renderOptions;
        private static string LastViewTag = "LastView";
        private static string LastView_TargetIdAttr = "TargetId";

        public ICurrentView lastView
        {
            get;
            private set;
        }

        public LayerList layerList
        {
            get;
            private set;
        }

        public void SetDirty()
        {
            dirtyEvent.SetDirty();
        }

        public Mashup()
        {
            dirtyEvent.Add(SetDirtyFlag);
            layerList = new LayerList(dirtyEvent);
            renderOptions = new RenderOptions(dirtyEvent);
        }

        private void SetDirtyFlag()
        {
            dirty = true;
            autoSaveDirty = true;
        }

        public bool IsDirty()
        {
            return dirty;
        }

        private void ClearDirty()
        {
            dirty = false;
            autoSaveDirty = false;
        }

        public RenderOptions GetRenderOptions()
        {
            return renderOptions;
        }

        public void SetFilename(string fileName)
        {
            if (File.Exists(GetAutoSaveName(this.fileName)))
            {
                RemoveAutoSaveBackup();
                autoSaveDirty = true;
            }

            this.fileName = fileName;
            D.Assert(Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
            if (autoSaveDirty)
            {
                AutoSaveBackup();
            }
        }

        public string GetFilename()
        {
            return fileName;
        }

        public string GetDisplayName()
        {
            string text = fileName;
            if (text == null)
            {
                text = "Untitled Mashup";
            }
            else
            {
                text = text.Remove(0, text.LastIndexOf('\\') + 1);
            }

            return text;
        }

        public string GetPublishedFilename()
        {
            string str;
            if (GetFilename() == null)
            {
                str = "unsaved";
            }
            else
            {
                str = Path.GetFileName(GetFilename());
            }

            return str + ".xml";
        }

        public string GetFilenameContext()
        {
            if (fileName == null)
            {
                return "";
            }

            return Path.GetDirectoryName(fileName);
        }

        public void WriteXML()
        {
            D.Assert(fileName != null);
            WriteXML(fileName);
            ClearDirty();
            RemoveAutoSaveBackup();
        }

        public void WriteXML(Stream outStream)
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(outStream, Encoding.UTF8);
            using (xmlTextWriter)
            {
                MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
                WriteXML(wc);
            }
        }

        private void WriteXML(string saveName)
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(saveName, Encoding.UTF8);
            using (xmlTextWriter)
            {
                MashupWriteContext wc = new MashupWriteContext(xmlTextWriter);
                WriteXML(wc);
            }
        }

        private void WriteXML(MashupWriteContext wc)
        {
            XmlTextWriter writer = wc.writer;
            writer.Formatting = Formatting.Indented;
            writer.WriteStartDocument(true);
            writer.WriteStartElement("MapGrinderMashupFile");
            CurrentSchema.schema.WriteXMLAttribute(writer);
            renderOptions.WriteXML(writer);
            layerList.WriteXML(wc);
            WriteXML_LastView(wc);
            writer.WriteEndElement();
            writer.Close();
        }

        private void WriteXML_LastView(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement(LastViewTag);
            if (lastView != null && !(lastView is NoView))
            {
                wc.writer.WriteAttributeString(LastView_TargetIdAttr, wc.GetIdentity(lastView.GetViewedObject()));
            }

            wc.writer.WriteEndElement();
        }

        public void ReadXML(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader("MapGrinderMashupFile");
            context.version = MashupXMLSchemaVersion.ReadXMLAttribute(context.reader);
            SingleMaxZoomForEntireMashupCompatibilityBlob singleMaxZoomForEntireMashupCompatibilityBlob = null;
            string text = null;
            while (xMLTagReader.FindNextStartTag())
            {
                if (context.version != MonolithicMapPositionsSchema.schema && xMLTagReader.TagIs(LayerList.GetXMLTag()))
                {
                    layerList = new LayerList(context,
                        GetFilenameContext,
                        dirtyEvent,
                        readyToLockEvent);
                }
                else
                {
                    if (context.version == MonolithicMapPositionsSchema.schema &&
                        xMLTagReader.TagIs(SourceMap.GetXMLTag()))
                    {
                        if (layerList != null && layerList.Count > 0)
                        {
                            throw new InvalidMashupFile(context,
                                string.Format("Multiple SourceMaps in Version {0} file.",
                                    context.version.versionNumberString));
                        }

                        SourceMap sourceMap = new SourceMap(context,
                            GetFilenameContext,
                            dirtyEvent,
                            readyToLockEvent);
                        layerList = new LayerList(dirtyEvent);
                        layerList.AddNewLayer();
                        layerList.First.Add(sourceMap);
                    }
                    else
                    {
                        if (xMLTagReader.TagIs(RenderOptions.GetXMLTag()))
                        {
                            renderOptions = new RenderOptions(context,
                                dirtyEvent,
                                ref singleMaxZoomForEntireMashupCompatibilityBlob);
                        }
                        else
                        {
                            if (xMLTagReader.TagIs(LastViewTag))
                            {
                                XMLTagReader xMLTagReader2 = context.NewTagReader(LastViewTag);
                                text = context.reader.GetAttribute(LastView_TargetIdAttr);
                                xMLTagReader2.SkipAllSubTags();
                            }
                        }
                    }
                }
            }

            lastView = new NoView();
            if (text != null)
            {
                object obj = context.FetchObjectByIdentity(text);
                if (obj != null && obj is LastViewIfc)
                {
                    lastView = ((LastViewIfc)obj).lastView;
                }
            }

            if (renderOptions == null)
            {
                if (context.version != MonolithicMapPositionsSchema.schema)
                {
                    context.warnings.Add(new MashupFileWarning("RenderOptions tag absent."));
                }

                renderOptions = new RenderOptions(dirtyEvent);
            }

            if (singleMaxZoomForEntireMashupCompatibilityBlob != null)
            {
                D.Assert(context.version == SingleMaxZoomForEntireMashupSchema.schema);
                foreach (Layer current in layerList)
                {
                    foreach (SourceMap current2 in current)
                    {
                        current2.sourceMapRenderOptions.maxZoom = singleMaxZoomForEntireMashupCompatibilityBlob.maxZoom;
                    }
                }
            }
        }

        public static Mashup OpenMashupInteractive(string fileName, out MashupFileWarningList warningList)
        {
            if (File.Exists(GetAutoSaveName(fileName)))
            {
                RecoverAutoSavedFileDialog recoverAutoSavedFileDialog = new RecoverAutoSavedFileDialog();
                recoverAutoSavedFileDialog.Initialize(GetAutoSaveName(fileName));
                DialogResult dialogResult = recoverAutoSavedFileDialog.ShowDialog();
                if (dialogResult == DialogResult.Yes)
                {
                    Mashup mashup = new Mashup(GetAutoSaveName(fileName), out warningList);
                    mashup.fileName = Path.Combine(Path.GetDirectoryName(fileName),
                        "Copy of " + Path.GetFileName(fileName));
                    mashup.SetDirty();
                    mashup.AutoSaveBackup();
                    File.Delete(GetAutoSaveName(fileName));
                    return mashup;
                }

                if (dialogResult == DialogResult.Ignore)
                {
                    File.Delete(GetAutoSaveName(fileName));
                }
                else
                {
                    if (dialogResult == DialogResult.Cancel)
                    {
                        warningList = null;
                        return null;
                    }

                    D.Assert(false, "Invalid enum");
                }
            }

            return new Mashup(fileName, out warningList);
        }

        public Mashup(string fileName, out MashupFileWarningList warningList) : this(fileName,
            File.Open(fileName, FileMode.Open, FileAccess.Read),
            out warningList)
        {
        }

        private Mashup(string fileName, Stream fromStream, out MashupFileWarningList warningList)
        {
            dirtyEvent.Add(SetDirtyFlag);
            this.fileName = fileName;
            D.Assert(fileName == null || Path.GetFullPath(fileName).ToLower().Equals(fileName.ToLower()));
            bool flag = false;
            XmlTextReader reader = new XmlTextReader(fromStream);
            MashupParseContext mashupParseContext = new MashupParseContext(reader);
            using (mashupParseContext)
            {
                while (mashupParseContext.reader.Read() && !flag)
                {
                    if (mashupParseContext.reader.NodeType == XmlNodeType.Element &&
                        mashupParseContext.reader.Name == "MapGrinderMashupFile")
                    {
                        flag = true;
                        ReadXML(mashupParseContext);
                    }
                }

                mashupParseContext.Dispose();
            }

            warningList = null;
            if (mashupParseContext.warnings.Count > 0)
            {
                warningList = mashupParseContext.warnings;
            }

            if (!flag)
            {
                throw new InvalidMashupFile(mashupParseContext,
                    string.Format("{0} doesn't appear to be a valid mashup file.", fileName));
            }

            ClearDirty();
        }

        internal void AutoSaveBackup()
        {
            if (!autoSaveDirty)
            {
                return;
            }

            try
            {
                WriteXML(GetAutoSaveName(fileName));
                autoSaveDirty = false;
                autoSaveFailNotified = false;
            }
            catch (Exception ex)
            {
                if (!autoSaveFailNotified)
                {
                    autoSaveFailNotified = true;
                    MessageBox.Show(
                        string.Format("Failed to autosave {0}:\n{1}", GetAutoSaveName(fileName), ex.Message),
                        "AutoSave Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Hand);
                }
            }
        }

        private static string GetAutoSaveName(string fileName)
        {
            if (fileName == null)
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(folderPath, "backup.Unnamed Crunchup.yum");
            }

            return Path.Combine(Path.GetDirectoryName(fileName), "backup." + Path.GetFileName(fileName));
        }

        private void RemoveAutoSaveBackup()
        {
            try
            {
                File.Delete(GetAutoSaveName(fileName));
            }
            catch (Exception ex)
            {
                D.Say(0, "Mashup.Close(): " + ex.ToString());
            }
        }

        internal void Close()
        {
            RemoveAutoSaveBackup();
        }

        public void SetLastView(ICurrentView lastView)
        {
            this.lastView = lastView;
        }

        internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
        {
            layerList.AutoSelectMaxZooms(mapTileSourceFactory);
        }

        internal Mashup Duplicate()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (memoryStream)
            {
                WriteXML(new MashupWriteContext(new XmlTextWriter(memoryStream, null)));
            }

            MemoryStream memoryStream3 = new MemoryStream(memoryStream.ToArray());
            Mashup result;
            using (memoryStream3)
            {
                MashupFileWarningList mashupFileWarningList;
                Mashup mashup = new Mashup(fileName, memoryStream3, out mashupFileWarningList);
                D.Assert(mashupFileWarningList == null);
                result = mashup;
            }

            return result;
        }

        internal bool SomeSourceMapIsReadyToLock()
        {
            return layerList.SomeSourceMapIsReadyToLock();
        }
    }
}
