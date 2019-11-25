using System.Collections.Generic;
using System.Globalization;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class Layer : HasDisplayNameIfc, PositionMemoryIfc, IRobustlyHashable, ExpandedMemoryIfc, LastViewIfc
    {
        private const string LayerTag = "Layer";
        private const string LayerDisplayNameAttr = "DisplayName";
        private const string LayerExpandedAttr = "Expanded";
        private const string SimulateTransparencyWithVEBackingLayerAttr = "SimulateTransparencyWithVEBackingLayer";
        private const string NewLayerName = "New Layer";
        private const string LastLayerViewPositionTag_compat = "LastLayerViewPosition";
        private List<SourceMap> sourceMaps = new List<SourceMap>();
        private bool _expanded = true;
        private string _displayName;
        private DirtyEvent dirtyEvent;
        private LayerView _lastView;

        public ICurrentView lastView
        {
            get
            {
                return _lastView;
            }
        }

        public bool expanded
        {
            get
            {
                return _expanded;
            }
            set
            {
                _expanded = value;
            }
        }

        public string displayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                dirtyEvent.SetDirty();
            }
        }

        public string simulateTransparencyWithVEBackingLayer
        {
            get;
        }

        public RenderClip renderClip
        {
            get;
        } = new RenderClip();

        public SourceMap First
        {
            get
            {
                return sourceMaps[0];
            }
        }

        public int Count
        {
            get
            {
                return sourceMaps.Count;
            }
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        public Layer(LayerList otherLayers, DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            int num = 1;
            string text = "New Layer";
            while (otherLayers.HasLayerNamed(text))
            {
                num++;
                text = string.Format("{0} {1}", "New Layer", num);
            }

            _displayName = text;
        }

        public Layer(MashupParseContext context, SourceMap.GetFilenameContext filenameContextDelegate,
            DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            XMLTagReader xMLTagReader = context.NewTagReader("Layer");
            context.ExpectIdentity(this);
            string attribute = context.reader.GetAttribute("DisplayName");
            if (attribute != null)
            {
                _displayName = attribute;
                context.GetAttributeBoolean("Expanded", ref _expanded);
                string attribute2 = context.reader.GetAttribute("SimulateTransparencyWithVEBackingLayer");
                if (attribute2 != null)
                {
                    simulateTransparencyWithVEBackingLayer = attribute2;
                }

                while (xMLTagReader.FindNextStartTag())
                {
                    if (xMLTagReader.TagIs(SourceMap.GetXMLTag()))
                    {
                        Add(new SourceMap(context,
                            filenameContextDelegate,
                            dirtyEvent,
                            parentReadyToLockEvent));
                    }
                    else
                    {
                        if (xMLTagReader.TagIs(LayerView.GetXMLTag()))
                        {
                            _lastView = new LayerView(this, context);
                        }
                        else
                        {
                            if (xMLTagReader.TagIs("LastLayerViewPosition"))
                            {
                                XMLTagReader xMLTagReader2 = context.NewTagReader("LastLayerViewPosition");
                                MapPosition mapPosition = null;
                                while (xMLTagReader2.FindNextStartTag())
                                {
                                    if (xMLTagReader2.TagIs(MapPosition.GetXMLTag(context.version)))
                                    {
                                        mapPosition = new MapPosition(context,
                                            null,
                                            MercatorCoordinateSystem.theInstance);
                                    }
                                }

                                if (mapPosition != null)
                                {
                                    _lastView = new LayerView(this, mapPosition);
                                }
                            }
                            else
                            {
                                if (xMLTagReader.TagIs(RenderClip.GetXMLTag()))
                                {
                                    renderClip = new RenderClip(context);
                                }
                            }
                        }
                    }
                }

                return;
            }

            throw new InvalidMashupFile(context, "Expected displayName attribute");
        }

        public void WriteXML(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement("Layer");
            wc.writer.WriteAttributeString("DisplayName", _displayName);
            wc.writer.WriteAttributeString("Expanded", _expanded.ToString(CultureInfo.InvariantCulture));
            wc.writer.WriteAttributeString("SimulateTransparencyWithVEBackingLayer",
                simulateTransparencyWithVEBackingLayer);
            wc.WriteIdentityAttr(this);
            foreach (SourceMap current in this)
            {
                current.WriteXML(wc);
            }

            if (lastView != null)
            {
                _lastView.WriteXML(wc.writer);
            }

            renderClip.WriteXML(wc);
            wc.writer.WriteEndElement();
        }

        internal static string GetXMLTag()
        {
            return "Layer";
        }

        internal static string GetLayerDisplayNameTag()
        {
            return "DisplayName";
        }

        internal string GetFilesystemName()
        {
            return RenderState.ForceValidFilename(string.Format("Layer_{0}", displayName));
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate("Layer(");
            hash.Accumulate(displayName);
            foreach (SourceMap current in sourceMaps)
            {
                current.AccumulateRobustHash(hash);
            }

            hash.Accumulate(")");
        }

        public void AccumulateRobustHash_PerTile(CachePackage cachePackage, IRobustHash hash)
        {
            hash.Accumulate("Layer(");
            foreach (SourceMap current in sourceMaps)
            {
                current.AccumulateRobustHash_PerTile(cachePackage, hash);
            }

            hash.Accumulate(")");
        }

        public List<SourceMap>.Enumerator GetEnumerator()
        {
            return sourceMaps.GetEnumerator();
        }

        public List<SourceMap> GetBackToFront()
        {
            List<SourceMap> list = new List<SourceMap>(sourceMaps);
            list.Reverse();
            return list;
        }

        public void Add(SourceMap sourceMap)
        {
            sourceMaps.Add(sourceMap);
            dirtyEvent.SetDirty();
        }

        internal bool Contains(SourceMap sourceMap)
        {
            return sourceMaps.Contains(sourceMap);
        }

        internal void Remove(SourceMap sourceMap)
        {
            sourceMaps.Remove(sourceMap);
            dirtyEvent.SetDirty();
        }

        internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
        {
            foreach (SourceMap current in sourceMaps)
            {
                current.AutoSelectMaxZoom(mapTileSourceFactory);
            }
        }

        public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
        {
            D.Assert(false, "Layers are never unlocked.");
        }

        public void NotePositionLocked(MapPosition referenceMapPosition)
        {
            _lastView = new LayerView(this, referenceMapPosition);
        }

        internal void AddAt(SourceMap sourceMap, int index)
        {
            sourceMaps.Insert(index, sourceMap);
        }

        internal int GetIndexOfSourceMap(SourceMap targetSourceMap)
        {
            return sourceMaps.FindIndex((SourceMap item) => item == targetSourceMap);
        }

        internal MapRectangle GetUserBoundingBox(MapTileSourceFactory mapTileSourceFactory)
        {
            MapRectangle mapRectangle = null;
            foreach (SourceMap current in sourceMaps)
            {
                mapRectangle = MapRectangle.Union(mapRectangle, current.GetUserBoundingBox(mapTileSourceFactory));
            }

            return mapRectangle;
        }

        internal bool SomeSourceMapIsReadyToLock()
        {
            foreach (SourceMap current in sourceMaps)
            {
                if (current.ReadyToLock())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
