using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMap : IDisposable, HasDisplayNameIfc, ReadyToLockIfc, PositionMemoryIfc, IRobustlyHashable,
        ExpandedMemoryIfc, LastViewIfc
    {
        public delegate string GetFilenameContext();

        private const string SourceMapTag = "SourceMap";
        private const string SourceMapFilenameAttribute = "SourceMapFilename";
        private const string SourceMapDisplayNameAttr = "DisplayName";
        private const string SourceMapOpenedAttr = "Opened";
        private const string SourceMapPageNumberAttr = "PageNumber";
        private const string ExpandedAttr = "Expanded";
        private const string LastSourcePositionTag_compat = "LastSourceMapPosition";
        private const string LastVEPositionTag_compat = "LastVEPosition";
        private const string LastVEStyleTag = "Style";
        private const string SnapViewTag = "SnapView";
        private const string SnapContextAttr = "Context";
        private const string SourceContextValue = "Source";
        private const string ReferenceContextValue = "Reference";
        private const string SnapZoomTag = "SnapZoom";
        private const string ZoomAttr = "Zoom";
        private string _displayName;
        private GetFilenameContext filenameContextDelegate;
        private bool _expanded;
        public RegistrationDefinition registration;
        public LatentRegionHolder latentRegionHolder;
        public LegendList legendList;
        private SourceMapRegistrationView _lastView;
        public DirtyEvent dirtyEvent;
        public DirtyEvent readyToLockChangedEvent;
        public LatLonZoom sourceSnap = LatLonZoom.World();
        public LatLonZoom referenceSnap = LatLonZoom.World();
        public int sourceSnapZoom = 1;
        public int referenceSnapZoom = 1;

        public RenderRegion renderRegion
        {
            get
            {
                return latentRegionHolder.renderRegion;
            }
            set
            {
                latentRegionHolder.renderRegion = value;
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

        public GeneralDocumentFuture documentFuture
        {
            get;
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

        public SourceMapInfo sourceMapInfo
        {
            get;
        }

        public SourceMapRenderOptions sourceMapRenderOptions
        {
            get;
        }

        public TransparencyOptions transparencyOptions
        {
            get;
        }

        public ICurrentView lastView
        {
            get
            {
                return _lastView;
            }
        }

        public SourceMap(IDocumentFuture documentDescriptor, GetFilenameContext filenameContextDelegate,
            DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            readyToLockChangedEvent = new DirtyEvent(parentReadyToLockEvent);
            documentFuture = new GeneralDocumentFuture(documentDescriptor);
            _displayName = documentDescriptor.GetDefaultDisplayName();
            this.filenameContextDelegate = filenameContextDelegate;
            sourceMapInfo = new SourceMapInfo(dirtyEvent);
            sourceMapRenderOptions = new SourceMapRenderOptions(dirtyEvent);
            transparencyOptions = new TransparencyOptions(dirtyEvent);
            registration = new RegistrationDefinition(dirtyEvent);
            registration.dirtyEvent.Add(readyToLockChangedEvent);
            latentRegionHolder = new LatentRegionHolder(dirtyEvent, readyToLockChangedEvent);
            legendList = new LegendList(this, dirtyEvent, readyToLockChangedEvent);
            renderRegion = null;
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        public RenderRegion GetUserRegion()
        {
            return renderRegion;
        }

        public void Dispose()
        {
        }

        public static string GetXMLTag()
        {
            return "SourceMap";
        }

        public void WriteXML(MashupWriteContext wc)
        {
            var writer = wc.writer;
            writer.WriteStartElement("SourceMap");
            writer.WriteAttributeString("DisplayName", displayName);
            writer.WriteAttributeString("Expanded", _expanded.ToString(CultureInfo.InvariantCulture));
            wc.WriteIdentityAttr(this);
            documentFuture.WriteXML(wc, filenameContextDelegate());
            sourceMapInfo.WriteXML(writer);
            sourceMapRenderOptions.WriteXML(writer);
            transparencyOptions.WriteXML(writer);
            if (_lastView != null)
            {
                _lastView.WriteXML(writer);
            }

            writer.WriteStartElement("SnapView");
            writer.WriteAttributeString("Context", "Source");
            sourceSnap.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteStartElement("SnapView");
            writer.WriteAttributeString("Context", "Reference");
            referenceSnap.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteStartElement("SnapZoom");
            writer.WriteAttributeString("Context", "Source");
            writer.WriteAttributeString("Zoom", sourceSnapZoom.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            writer.WriteStartElement("SnapZoom");
            writer.WriteAttributeString("Context", "Reference");
            writer.WriteAttributeString("Zoom", referenceSnapZoom.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            registration.WriteXML(writer);
            if (renderRegion != null)
            {
                renderRegion.WriteXML(writer);
            }

            legendList.WriteXML(wc);
            writer.WriteEndElement();
        }

        public SourceMap(MashupParseContext context, GetFilenameContext filenameContextDelegate, DirtyEvent parentDirty,
            DirtyEvent parentReadyToLockEvent)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            readyToLockChangedEvent = new DirtyEvent(parentReadyToLockEvent);
            this.filenameContextDelegate = filenameContextDelegate;
            latentRegionHolder = new LatentRegionHolder(dirtyEvent, readyToLockChangedEvent);
            var xMLTagReader = context.NewTagReader("SourceMap");
            context.ExpectIdentity(this);
            string attribute = context.reader.GetAttribute("SourceMapFilename");
            if (attribute != null)
            {
                string path = Path.Combine(filenameContextDelegate(), attribute);
                int pageNumber = 0;
                context.GetAttributeInt("PageNumber", ref pageNumber);
                documentFuture = new GeneralDocumentFuture(new FutureDocumentFromFilesystem(path, pageNumber));
            }

            context.GetAttributeBoolean("Expanded", ref _expanded);
            string attribute2 = context.reader.GetAttribute("DisplayName");
            MapPosition mapPosition = null;
            MapPosition mapPosition2 = null;
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(RegistrationDefinition.GetXMLTag()))
                {
                    context.AssertUnique(registration);
                    registration = new RegistrationDefinition(context, dirtyEvent);
                }
                else
                {
                    if (xMLTagReader.TagIs(GeneralDocumentFuture.GetXMLTag()))
                    {
                        context.AssertUnique(documentFuture);
                        documentFuture = new GeneralDocumentFuture(context, filenameContextDelegate());
                    }
                    else
                    {
                        if (xMLTagReader.TagIs(LocalDocumentDescriptor.GetXMLTag()))
                        {
                            context.AssertUnique(documentFuture);
                            var localDocumentDescriptor =
                                new LocalDocumentDescriptor(context, filenameContextDelegate());
                            documentFuture = new GeneralDocumentFuture(
                                new FutureDocumentFromFilesystem(localDocumentDescriptor.GetFilesystemAbsolutePath(),
                                    localDocumentDescriptor.GetPageNumber()));
                        }
                        else
                        {
                            if (xMLTagReader.TagIs("LastSourceMapPosition"))
                            {
                                var xMLTagReader2 = context.NewTagReader("LastSourceMapPosition");
                                while (xMLTagReader2.FindNextStartTag())
                                {
                                    if (xMLTagReader2.TagIs(MapPosition.GetXMLTag(context.version)))
                                    {
                                        mapPosition = new MapPosition(context,
                                            null,
                                            ContinuousCoordinateSystem.theInstance);
                                    }
                                }
                            }
                            else
                            {
                                if (xMLTagReader.TagIs("LastVEPosition"))
                                {
                                    var xMLTagReader3 = context.NewTagReader("LastVEPosition");
                                    while (xMLTagReader3.FindNextStartTag())
                                    {
                                        if (xMLTagReader3.TagIs(MapPosition.GetXMLTag(context.version)))
                                        {
                                            mapPosition2 = new MapPosition(context,
                                                null,
                                                MercatorCoordinateSystem.theInstance);
                                        }
                                    }
                                }
                                else
                                {
                                    if (xMLTagReader.TagIs(RenderRegion.GetXMLTag()))
                                    {
                                        context.AssertUnique(renderRegion);
                                        renderRegion = new RenderRegion(context,
                                            dirtyEvent,
                                            ContinuousCoordinateSystem.theInstance);
                                    }
                                    else
                                    {
                                        if (xMLTagReader.TagIs(SourceMapInfo.GetXMLTag()))
                                        {
                                            context.AssertUnique(sourceMapInfo);
                                            sourceMapInfo = new SourceMapInfo(context, dirtyEvent);
                                        }
                                        else
                                        {
                                            if (xMLTagReader.TagIs(SourceMapRenderOptions.GetXMLTag()))
                                            {
                                                context.AssertUnique(sourceMapRenderOptions);
                                                sourceMapRenderOptions =
                                                    new SourceMapRenderOptions(context, dirtyEvent);
                                            }
                                            else
                                            {
                                                if (xMLTagReader.TagIs(TransparencyOptions.GetXMLTag()))
                                                {
                                                    transparencyOptions =
                                                        new TransparencyOptions(context, dirtyEvent);
                                                }
                                                else
                                                {
                                                    if (xMLTagReader.TagIs(SourceMapRegistrationView.GetXMLTag()))
                                                    {
                                                        context.AssertUnique(_lastView);
                                                        _lastView = new SourceMapRegistrationView(this, context);
                                                    }
                                                    else
                                                    {
                                                        if (xMLTagReader.TagIs(LegendList.GetXMLTag()))
                                                        {
                                                            context.AssertUnique(legendList);
                                                            legendList = new LegendList(this,
                                                                context,
                                                                dirtyEvent);
                                                        }
                                                        else
                                                        {
                                                            if (xMLTagReader.TagIs("SnapView"))
                                                            {
                                                                var xMLTagReader4 =
                                                                    context.NewTagReader("SnapView");
                                                                string requiredAttribute =
                                                                    context.GetRequiredAttribute("Context");
                                                                var latLonZoom = default(LatLonZoom);
                                                                bool flag = false;
                                                                bool flag2 = true;
                                                                CoordinateSystemIfc coordSys;
                                                                if (requiredAttribute == "Source")
                                                                {
                                                                    coordSys = ContinuousCoordinateSystem.theInstance;
                                                                }
                                                                else
                                                                {
                                                                    if (!(requiredAttribute == "Reference"))
                                                                    {
                                                                        throw new InvalidMashupFile(context,
                                                                            string.Format("Invalid {0} value {1}",
                                                                                "Context",
                                                                                requiredAttribute));
                                                                    }

                                                                    coordSys = MercatorCoordinateSystem.theInstance;
                                                                }

                                                                while (xMLTagReader4.FindNextStartTag())
                                                                {
                                                                    if (xMLTagReader4.TagIs(LatLonZoom.GetXMLTag()))
                                                                    {
                                                                        if (flag)
                                                                        {
                                                                            context.ThrowUnique();
                                                                        }

                                                                        try
                                                                        {
                                                                            latLonZoom = new LatLonZoom(context,
                                                                                coordSys);
                                                                        }
                                                                        catch (InvalidLLZ)
                                                                        {
                                                                            flag2 = false;
                                                                        }

                                                                        flag = true;
                                                                    }
                                                                }

                                                                if (flag2)
                                                                {
                                                                    if (!flag)
                                                                    {
                                                                        context.AssertPresent(null,
                                                                            LatLonZoom.GetXMLTag());
                                                                    }

                                                                    if (requiredAttribute == "Source")
                                                                    {
                                                                        sourceSnap = latLonZoom;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (requiredAttribute == "Reference")
                                                                        {
                                                                            referenceSnap = latLonZoom;
                                                                        }
                                                                        else
                                                                        {
                                                                            D.Assert(false, "handled above.");
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (xMLTagReader.TagIs("SnapZoom"))
                                                                {
                                                                    context.NewTagReader("SnapZoom");
                                                                    string requiredAttribute2 =
                                                                        context.GetRequiredAttribute("Context");
                                                                    bool flag3 = false;
                                                                    CoordinateSystemIfc theInstance;
                                                                    if (requiredAttribute2 == "Source")
                                                                    {
                                                                        theInstance = ContinuousCoordinateSystem
                                                                            .theInstance;
                                                                    }
                                                                    else
                                                                    {
                                                                        if (!(requiredAttribute2 == "Reference"))
                                                                        {
                                                                            throw new InvalidMashupFile(context,
                                                                                string.Format("Invalid {0} value {1}",
                                                                                    "Context",
                                                                                    requiredAttribute2));
                                                                        }

                                                                        theInstance = MercatorCoordinateSystem
                                                                            .theInstance;
                                                                    }

                                                                    int num = 0;
                                                                    try
                                                                    {
                                                                        theInstance.GetZoomRange()
                                                                            .Parse(context, "Zoom");
                                                                        flag3 = true;
                                                                    }
                                                                    catch (InvalidMashupFile)
                                                                    {
                                                                    }

                                                                    if (flag3)
                                                                    {
                                                                        if (requiredAttribute2 == "Source")
                                                                        {
                                                                            sourceSnapZoom = num;
                                                                        }
                                                                        else
                                                                        {
                                                                            if (requiredAttribute2 == "Reference")
                                                                            {
                                                                                referenceSnapZoom = num;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (context.version == InlineSourceMapInfoSchema.schema)
                {
                    if (xMLTagReader.TagIs("MapFileURL"))
                    {
                        sourceMapInfo.mapFileURL = XMLUtils.ReadStringXml(context, "MapFileURL");
                    }
                    else
                    {
                        if (xMLTagReader.TagIs("MapHomePage"))
                        {
                            sourceMapInfo.mapHomePage = XMLUtils.ReadStringXml(context, "MapHomePage");
                        }
                        else
                        {
                            if (xMLTagReader.TagIs("MapDescription"))
                            {
                                sourceMapInfo.mapDescription = XMLUtils.ReadStringXml(context, "MapDescription");
                            }
                        }
                    }
                }
            }

            if (attribute2 != null)
            {
                _displayName = attribute2;
            }
            else
            {
                _displayName = documentFuture.documentFuture.GetDefaultDisplayName();
            }

            if (_lastView == null && mapPosition != null && mapPosition2 != null)
            {
                _lastView = new SourceMapRegistrationView(this, mapPosition.llz, mapPosition2);
            }

            if (documentFuture == null)
            {
                throw new Exception("Source Map element missing document descriptor tag");
            }

            if (registration == null)
            {
                registration = new RegistrationDefinition(dirtyEvent);
            }

            registration.dirtyEvent.Add(readyToLockChangedEvent);
            if (legendList == null)
            {
                legendList = new LegendList(this, dirtyEvent, readyToLockChangedEvent);
            }

            if (sourceMapInfo == null)
            {
                sourceMapInfo = new SourceMapInfo(dirtyEvent);
            }

            if (sourceMapRenderOptions == null)
            {
                sourceMapRenderOptions = new SourceMapRenderOptions(dirtyEvent);
            }

            if (transparencyOptions == null)
            {
                transparencyOptions = new TransparencyOptions(dirtyEvent);
            }
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            documentFuture.AccumulateRobustHash(hash);
            AccumulateRobustHash_Common(hash);
            sourceMapRenderOptions.AccumulateRobustHash(hash);
        }

        public void AccumulateRobustHash_PerTile(CachePackage cachePackage, IRobustHash hash)
        {
            hash.Accumulate("SourceMap:");
            var sourceDocument = documentFuture.RealizeSynchronously(cachePackage);
            sourceDocument.localDocument.AccumulateRobustHash(hash);
            AccumulateRobustHash_Common(hash);
        }

        private void AccumulateRobustHash_Common(IRobustHash hash)
        {
            registration.AccumulateRobustHash(hash);
            if (renderRegion != null)
            {
                renderRegion.AccumulateRobustHash(hash);
            }
            else
            {
                hash.Accumulate("null-render-region");
            }

            transparencyOptions.AccumulateRobustHash(hash);
        }

        public bool ReadyToLock()
        {
            return registration != null && registration.ReadyToLock() && renderRegion != null;
        }

        public void AutoSelectMaxZoom(MapTileSourceFactory mapTileSourceFactory)
        {
            if (sourceMapRenderOptions.maxZoom == -1)
            {
                var userBoundingBox = GetUserBoundingBox(mapTileSourceFactory);
                if (userBoundingBox == null)
                {
                    return;
                }

                var size = new Size(600, 600);
                var bestViewContaining =
                    new MercatorCoordinateSystem().GetBestViewContaining(userBoundingBox, size);
                var intParameter = (IntParameter)mapTileSourceFactory.CreateUnwarpedSource(this)
                    .GetImageDetailPrototype(FutureFeatures.Cached)
                    .Curry(new ParamDict(new object[] {TermName.ImageDetail, new SizeParameter(size)}))
                    .Realize("SourceMap.AutoSelectMaxZoom");
                sourceMapRenderOptions.maxZoom = MercatorCoordinateSystem.theInstance.GetZoomRange()
                    .Constrain(bestViewContaining.zoom + intParameter.value + BuildConfig.theConfig.autoMaxZoomOffset);
            }
        }

        public MapRectangle GetUserBoundingBox(MapTileSourceFactory mapTileSourceFactory)
        {
            WarpedMapTileSource warpedMapTileSource = null;
            try
            {
                warpedMapTileSource = mapTileSourceFactory.CreateWarpedSource(this);
            }
            catch (InsufficientCorrespondencesException)
            {
            }

            if (warpedMapTileSource == null)
            {
                return null;
            }

            var present = warpedMapTileSource.GetUserBounds(null, FutureFeatures.Cached)
                .Realize("SourceMap.AutoSelectMaxZoom");
            if (!(present is BoundsPresent))
            {
                return null;
            }

            var boundsPresent = (BoundsPresent)present;
            var boundingBox = boundsPresent.GetRenderRegion().GetBoundingBox();
            return boundingBox.ClipTo(
                CoordinateSystemUtilities.GetRangeAsMapRectangle(MercatorCoordinateSystem.theInstance));
        }

        public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
        {
            _lastView = new SourceMapRegistrationView(this, sourceMapPosition, referenceMapPosition);
        }

        public void NotePositionLocked(MapPosition referenceMapPosition)
        {
            _lastView = new SourceMapRegistrationView(this, referenceMapPosition);
        }

        public string GetLegendFilename(Legend legend)
        {
            return RenderState.ForceValidFilename(string.Format("{0}_{1}.png",
                GetDisplayName(),
                legend.displayName));
        }
    }
}
