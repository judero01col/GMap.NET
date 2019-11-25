using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class Manifest
    {
        public delegate void TellManifestDirty();

        public class ManifestRecord
        {
            private const string ManifestRecordTag = "ManifestRecord";
            private const string PathAttr = "Path";
            private const string FileExistsAttr = "FileExists";
            private const string FileLengthAttr = "FileLength";
            private const string IndirectManifestBlockIdAttr = "IndirectManifestBlockId";
            private bool _fileExists;
            private long _fileLength;
            private static ManifestRecord _tailRecord;

            public ManifestBlock block
            {
                get;
            }

            public string path
            {
                get;
            }

            public bool fileExists
            {
                get
                {
                    return _fileExists;
                }
                set
                {
                    _fileExists = value;
                    block.SetDirty();
                }
            }

            public long fileLength
            {
                get
                {
                    return _fileLength;
                }
                set
                {
                    _fileLength = value;
                    block.SetDirty();
                }
            }

            public int indirectManifestBlockId
            {
                get;
            }

            public static ManifestRecord TailRecord
            {
                get
                {
                    if (_tailRecord == null)
                    {
                        _tailRecord = new ManifestRecord(null, null, false, -1L, -1);
                    }

                    return _tailRecord;
                }
            }

            public bool IsTailRecord
            {
                get
                {
                    return path == null;
                }
            }

            public ManifestRecord(ManifestBlock block, string path, bool fileExists, long fileLength,
                int indirectManifestBlockId)
            {
                this.block = block;
                this.path = path;
                _fileExists = fileExists;
                _fileLength = fileLength;
                this.indirectManifestBlockId = indirectManifestBlockId;
                D.Assert(block == null || path != null);
            }

            internal void WriteXML(XmlTextWriter xtw)
            {
                xtw.WriteStartElement("ManifestRecord");
                xtw.WriteAttributeString("Path", path);
                xtw.WriteAttributeString("FileExists", fileExists.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("FileLength", fileLength.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("IndirectManifestBlockId",
                    indirectManifestBlockId.ToString(CultureInfo.InvariantCulture));
                xtw.WriteEndElement();
            }

            public ManifestRecord(MashupParseContext context, ManifestBlock block)
            {
                this.block = block;
                XMLTagReader xMLTagReader = context.NewTagReader("ManifestRecord");
                path = context.GetRequiredAttribute("Path");
                _fileExists = context.GetRequiredAttributeBoolean("FileExists");
                _fileLength = context.GetRequiredAttributeLong("FileLength");
                indirectManifestBlockId = context.GetRequiredAttributeInt("IndirectManifestBlockId");
                xMLTagReader.SkipAllSubTags();
            }

            internal static string GetXmlTag()
            {
                return "ManifestRecord";
            }

            public override string ToString()
            {
                return string.Format("MR({0}, {1})", path, fileExists);
            }

            internal ManifestRecord ReplaceBlock(ManifestBlock manifestBlock)
            {
                return new ManifestRecord(manifestBlock,
                    path,
                    fileExists,
                    fileLength,
                    indirectManifestBlockId);
            }

            internal ManifestRecord ReplaceIndirect(int newIndirectBlockId)
            {
                return new ManifestRecord(block, path, fileExists, fileLength, newIndirectBlockId);
            }
        }

        public class ManifestSuperBlock
        {
            private const string NextUnassignedBlockIdAttr = "NextUnassignedBlockId";
            private const string SplitThresholdAttr = "SplitThreshold";
            private int _splitThreshold = 2000;
            private int _nextUnassignedBlockId;
            private TellManifestDirty tellDirty;

            public int splitThreshold
            {
                get
                {
                    return _splitThreshold;
                }
                set
                {
                    _splitThreshold = value;
                    tellDirty();
                }
            }

            public int nextUnassignedBlockId
            {
                get
                {
                    return _nextUnassignedBlockId;
                }
                set
                {
                    _nextUnassignedBlockId = value;
                    tellDirty();
                }
            }

            public ManifestSuperBlock(int nextUnassignedBlockId, TellManifestDirty tellDirty)
            {
                _nextUnassignedBlockId = nextUnassignedBlockId;
                this.tellDirty = tellDirty;
            }

            public ManifestSuperBlock(MashupParseContext context, TellManifestDirty tellDirty)
            {
                this.tellDirty = tellDirty;
                XMLTagReader xMLTagReader = context.NewTagReader(GetXmlTag());
                _nextUnassignedBlockId = context.GetRequiredAttributeInt("NextUnassignedBlockId");
                _splitThreshold = context.GetRequiredAttributeInt("SplitThreshold");
                xMLTagReader.SkipAllSubTags();
            }

            internal void WriteXML(XmlTextWriter xtw)
            {
                xtw.WriteStartElement(GetXmlTag());
                xtw.WriteAttributeString("NextUnassignedBlockId",
                    _nextUnassignedBlockId.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("SplitThreshold", _splitThreshold.ToString(CultureInfo.InvariantCulture));
                xtw.WriteEndElement();
            }

            internal static string GetXmlTag()
            {
                return "SuperBlock";
            }
        }

        public class ManifestBlock : IEnumerable<ManifestRecord>, IEnumerable
        {
            public delegate ManifestBlock CreateBlock();

            private const string ManifestsDir = "manifests";
            private const string ManifestBlockTag = "ManifestBlock";
            private List<ManifestRecord> recordList = new List<ManifestRecord>();
            public int blockId;
            private bool dirty;
            private TellManifestDirty tellManifestDirty;

            public ManifestSuperBlock superBlock
            {
                get;
            }

            public int Count
            {
                get
                {
                    return recordList.Count;
                }
            }

            public void SetDirty()
            {
                dirty = true;
                tellManifestDirty();
            }

            internal void CommitChanges(Manifest manifest)
            {
                if (!dirty)
                {
                    return;
                }

                WriteChanges(manifest.storageMethod);
                dirty = false;
            }

            private string manifestFilename(int blockId)
            {
                return string.Format("{0}.xml", blockId);
            }

            private void WriteChanges(RenderOutputMethod outputMethod)
            {
                Stream w = outputMethod.MakeChildMethod("manifests")
                    .CreateFile(manifestFilename(blockId), "text/xml");
                XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8);
                using (xmlTextWriter)
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.WriteStartDocument(true);
                    xmlTextWriter.WriteStartElement("ManifestBlock");
                    if (superBlock != null)
                    {
                        superBlock.WriteXML(xmlTextWriter);
                    }

                    foreach (ManifestRecord current in this)
                    {
                        current.WriteXML(xmlTextWriter);
                    }

                    xmlTextWriter.WriteEndElement();
                    xmlTextWriter.Close();
                }
            }

            public ManifestBlock(TellManifestDirty tellManifestDirty, RenderOutputMethod outputMethod, int blockId)
            {
                this.tellManifestDirty = tellManifestDirty;
                this.blockId = blockId;
                try
                {
                    Stream input = outputMethod.MakeChildMethod("manifests").ReadFile(manifestFilename(blockId));
                    XmlTextReader xmlTextReader = new XmlTextReader(input);
                    using (xmlTextReader)
                    {
                        MashupParseContext mashupParseContext = new MashupParseContext(xmlTextReader);
                        while (mashupParseContext.reader.Read())
                        {
                            if (mashupParseContext.reader.NodeType == XmlNodeType.Element &&
                                mashupParseContext.reader.Name == "ManifestBlock")
                            {
                                XMLTagReader xMLTagReader = mashupParseContext.NewTagReader("ManifestBlock");
                                while (xMLTagReader.FindNextStartTag())
                                {
                                    if (xMLTagReader.TagIs(ManifestRecord.GetXmlTag()))
                                    {
                                        recordList.Add(new ManifestRecord(mashupParseContext, this));
                                    }
                                    else
                                    {
                                        if (xMLTagReader.TagIs(ManifestSuperBlock.GetXmlTag()))
                                        {
                                            superBlock = new ManifestSuperBlock(mashupParseContext,
                                                new TellManifestDirty(SetDirty));
                                        }
                                    }
                                }

                                return;
                            }
                        }

                        throw new InvalidMashupFile(mashupParseContext, "No ManifestBlock tag");
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (blockId == 0 && superBlock == null)
                    {
                        superBlock = new ManifestSuperBlock(1, new TellManifestDirty(SetDirty));
                    }
                }
            }

            public void Insert(ManifestRecord newRecord, ManifestRecord afterRecord)
            {
                D.Assert(afterRecord.IsTailRecord || afterRecord.block == this);
                D.Assert(newRecord.block == this);
                if (afterRecord.IsTailRecord)
                {
                    recordList.Add(newRecord);
                }
                else
                {
                    recordList.Insert(
                        recordList.FindIndex((ManifestRecord mrb) => mrb.path == afterRecord.path),
                        newRecord);
                }

                SetDirty();
            }

            public IEnumerator<ManifestRecord> GetEnumerator()
            {
                return recordList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return recordList.GetEnumerator();
            }

            internal void Split(CreateBlock createBlock)
            {
                int num = 2;
                ManifestBlock[] subBlocks = new ManifestBlock[num];
                for (int j = 0; j < num; j++)
                {
                    subBlocks[j] = createBlock();
                }

                List<ManifestRecord> list = new List<ManifestRecord>();
                Converter<ManifestRecord, ManifestRecord> converter = null;
                for (int i = 0; i < num; i++)
                {
                    int index = (int)(i / (double)num * recordList.Count);
                    int num4 = (int)((i + 1.0) / num * recordList.Count);
                    if (converter == null)
                    {
                        converter = mr => mr.ReplaceBlock(subBlocks[i]);
                    }

                    subBlocks[i].recordList = recordList.GetRange(index, num4 - index)
                        .ConvertAll(converter);
                    ManifestRecord item = recordList[index].ReplaceIndirect(subBlocks[i].blockId);
                    list.Add(item);
                    subBlocks[i].SetDirty();
                }

                recordList = list;
                SetDirty();
            }
        }

        private delegate bool StopHere(string recP);

        private RenderOutputMethod storageMethod;
        private ManifestBlock rootBlock;
        internal Dictionary<int, ManifestBlock> blockCache = new Dictionary<int, ManifestBlock>();
        private int dirtyCount;

        public Manifest(RenderOutputMethod storageMethod)
        {
            this.storageMethod = storageMethod;
            rootBlock = FetchBlock(0);
        }

        public void Test_SetSplitThreshold(int splitThreshold)
        {
            rootBlock.superBlock.splitThreshold = splitThreshold;
        }

        private ManifestBlock FetchBlock(int blockId)
        {
            if (blockCache.ContainsKey(blockId))
            {
                return blockCache[blockId];
            }

            ManifestBlock manifestBlock =
                new ManifestBlock(new TellManifestDirty(SetDirty), storageMethod, blockId);
            blockCache[blockId] = manifestBlock;
            return manifestBlock;
        }

        private ManifestBlock CreateBlock()
        {
            ManifestBlock manifestBlock = new ManifestBlock(new TellManifestDirty(SetDirty),
                storageMethod,
                rootBlock.superBlock.nextUnassignedBlockId);
            rootBlock.superBlock.nextUnassignedBlockId++;
            D.Assert(!blockCache.ContainsKey(manifestBlock.blockId));
            blockCache[manifestBlock.blockId] = manifestBlock;
            return manifestBlock;
        }

        private ManifestRecord Search(StopHere stopHere)
        {
            ManifestBlock rootBlock = this.rootBlock;
            ManifestRecord tailRecord = ManifestRecord.TailRecord;
            while (true)
            {
                ManifestRecord record2 = null;
                bool flag = false;
                foreach (ManifestRecord record3 in new List<ManifestRecord>(rootBlock) {tailRecord})
                {
                    if (stopHere(record3.path))
                    {
                        if (record2 == null || record2.indirectManifestBlockId < 0)
                        {
                            return record3;
                        }

                        rootBlock = FetchBlock(record2.indirectManifestBlockId);
                        tailRecord = record3;
                        flag = true;
                        break;
                    }

                    record2 = record3;
                }

                if (!flag)
                {
                    D.Assert(false, "Should have stopped inside loop.");
                }
            }
        }

        internal ManifestRecord FindFirstGreaterThan(string p)
        {
            return Search((string recP) => recP == null || recP.CompareTo(p) > 0);
        }

        internal ManifestRecord FindFirstGreaterEqual(string p)
        {
            return Search((string recP) => recP == null || recP.CompareTo(p) >= 0);
        }

        internal ManifestRecord FindFirstEqual(string path)
        {
            ManifestRecord manifestRecord = FindFirstGreaterEqual(path);
            if (manifestRecord.path == path)
            {
                return manifestRecord;
            }

            return ManifestRecord.TailRecord;
        }

        internal void Add(string path, long fileLength)
        {
            ManifestRecord manifestRecord = FindFirstGreaterEqual(path);
            if (manifestRecord.path == path)
            {
                manifestRecord.fileExists = true;
                manifestRecord.fileLength = fileLength;
                return;
            }

            ManifestBlock manifestBlock = manifestRecord.block == null ? rootBlock : manifestRecord.block;
            ManifestRecord newRecord = new ManifestRecord(manifestBlock, path, true, fileLength, -1);
            manifestBlock.Insert(newRecord, manifestRecord);
            if (manifestBlock.Count > rootBlock.superBlock.splitThreshold)
            {
                manifestBlock.Split(new ManifestBlock.CreateBlock(CreateBlock));
            }
        }

        public void Remove(string p)
        {
            ManifestRecord manifestRecord = FindFirstEqual(p);
            if (manifestRecord.IsTailRecord)
            {
                return;
            }

            manifestRecord.fileExists = false;
            manifestRecord.fileLength = -1L;
        }

        public void CommitChanges()
        {
            foreach (ManifestBlock current in blockCache.Values)
            {
                current.CommitChanges(this);
            }
        }

        internal void SetDirty()
        {
            dirtyCount++;
            if (dirtyCount > 100)
            {
                CommitChanges();
                dirtyCount = 0;
            }
        }
    }
}
