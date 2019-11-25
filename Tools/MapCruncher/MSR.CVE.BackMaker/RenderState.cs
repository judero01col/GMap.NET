using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class RenderState : ITileWorkFeedback
    {
        public delegate void FlushRenderedTileCachePackageDelegate();

        private enum States
        {
            ReadyToRender,
            Rendering,
            Paused,
            Aborted,
            DoneRendering
        }

        private class RenderAborted : Exception
        {
        }

        private class RenderDisposed : Exception
        {
        }

        private class SetupFailed : Exception
        {
            private string prettyMessage;

            public SetupFailed(bool pretty, string m) : base(m)
            {
                if (pretty)
                {
                    prettyMessage = m;
                }
            }

            public override string ToString()
            {
                if (prettyMessage != null)
                {
                    return prettyMessage;
                }

                return base.ToString();
            }
        }

        private class LayerApplierMaker
        {
            private CachePackage cachePackage;

            private Dictionary<IRenderableSource, OneLayerBoundApplier> dict =
                new Dictionary<IRenderableSource, OneLayerBoundApplier>();

            public LayerApplierMaker(CachePackage cachePackage)
            {
                cachePackage = cachePackage;
            }

            public OneLayerBoundApplier MakeApplier(IRenderableSource source, Layer layer)
            {
                if (!dict.ContainsKey(source))
                {
                    dict[source] = new OneLayerBoundApplier(source, layer, cachePackage);
                }

                return dict[source];
            }
        }

        private class ProposedTileSet : Dictionary<TileAddress, CompositeTileUnit>
        {
            public CompositeTileUnit MakeLayeredTileWork(TileAddress tileAddress, Layer layer,
                RenderOutputMethod renderOutput, string outputFilename, OutputTileType outputTileType)
            {
                if (!ContainsKey(tileAddress))
                {
                    base[tileAddress] =
                        new CompositeTileUnit(layer, tileAddress, renderOutput, outputFilename, outputTileType);
                }

                return base[tileAddress];
            }
        }

        private class SourceMapRenderInfo
        {
            public SourceMap sourceMap;
            public IRenderableSource warpedMapTileSource;
            public MapRectangle renderBoundsBoundingBox;
        }

        private const int sortPseudoLayer = 1;
        private const int TileCountReportInterval = 10000;
        private const int UnreasonablyManyTiles = 1000000;
        private const int ErrorMessageLimit = 100;
        private const int RangeQuerySizeLimit = 100;
        private const string thumbnailPathPrefix = "thumbnails";
        private Mashup _mashupDocument_UseScratchCopy;
        private Mashup mashupScratchCopy;
        private readonly RenderUIIfc _renderUI;
        private FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage;
        private MapTileSourceFactory mapTileSourceFactory;
        private bool disposeFlag;
        private States _state;
        private bool pauseRenderingFlag;

        private EventWaitHandle startRenderEvent =
            new CountedEventWaitHandle(false, EventResetMode.AutoReset, "RenderState.startRenderEvent");

        private MercatorCoordinateSystem mercatorCoordinateSystem = new MercatorCoordinateSystem();
        private RenderOutputMethod renderOutput;
        private List<TileRectangle> boundsList;
        private RangeQueryData rangeQueryData;
        private Queue<RenderWorkUnit> renderQueue = new Queue<RenderWorkUnit>();
        private Dictionary<string, bool> credits = new Dictionary<string, bool>();
        private bool complainedAboutInsaneTileCount;
        private int estimateProgressLayerCount;
        private int estimateProgressSourceMapCount;
        private int estimateProgressSourceMapsThisLayer = 1;
        private Uri renderedXMLDescriptor;
        private Uri sampleHTMLUri;
        private int initialQueueSize;
        private string statusString;
        private List<string> postedMessages = new List<string>();
        private RenderComplaintBox complaintBox;
        private ImageRef lastRenderedImageRef;
        private string[] lastRenderedImageLabel = new string[0];
        private StreamWriter logWriter;
        public static string SourceDataDirName = "SourceData";

        private States state
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                _renderUI.uiChanged();
            }
        }

        public RenderState(Mashup mashupDocument, RenderUIIfc renderUI,
            FlushRenderedTileCachePackageDelegate flushRenderedTileCachePackage,
            MapTileSourceFactory mapTileSourceFactory)
        {
            _mashupDocument_UseScratchCopy = mashupDocument;
            _renderUI = renderUI;
            this.flushRenderedTileCachePackage = flushRenderedTileCachePackage;
            this.mapTileSourceFactory = mapTileSourceFactory;
            DebugThreadInterrupter.theInstance.AddThread("RenderState",
                ThreadTask,
                ThreadPriority.BelowNormal);
            var resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("RenderState", -1);
            resourceCounter.crement(1);
            complaintBox = new RenderComplaintBox(PostMessage);
        }

        private void OpenLog()
        {
            if (logWriter == null)
            {
                logWriter =
                    new StreamWriter(new FileStream(FileUtilities.MakeTempFilename(".", "RenderLog"), FileMode.Create));
                logWriter.AutoFlush = true;
            }
        }

        internal void Dispose()
        {
            disposeFlag = true;
            startRenderEvent.Set();
            var resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("RenderState", -1);
            resourceCounter.crement(-1);
            if (logWriter != null)
            {
                logWriter.Close();
                logWriter.Dispose();
                logWriter = null;
            }
        }

        internal void RenderClick()
        {
            switch (state)
            {
                case States.ReadyToRender:
                case States.Paused:
                    startRenderEvent.Set();
                    _renderUI.uiChanged();
                    return;
                case States.Rendering:
                    pauseRenderingFlag = true;
                    _renderUI.uiChanged();
                    return;
                default:
                    return;
            }
        }

        internal void UI_UpdateRenderControlButtonLabel(Button renderControlButton)
        {
            switch (state)
            {
                case States.ReadyToRender:
                    renderControlButton.Text = "Start";
                    renderControlButton.Enabled = true;
                    return;
                case States.Rendering:
                    renderControlButton.Text = "Pause";
                    renderControlButton.Enabled = true;
                    return;
                case States.Paused:
                    renderControlButton.Text = "Resume";
                    renderControlButton.Enabled = true;
                    return;
                case States.Aborted:
                    renderControlButton.Text = "Render Aborted.";
                    renderControlButton.Enabled = false;
                    return;
                case States.DoneRendering:
                    renderControlButton.Text = "Render Complete.";
                    renderControlButton.Enabled = false;
                    return;
                default:
                    D.Assert(false, "Invalid state.");
                    return;
            }
        }

        public void StartRender()
        {
            OpenLog();
            if (state != States.ReadyToRender)
            {
                throw new Exception("I kind of imagined that you'd only call this to implement renderOnLaunch.");
            }

            startRenderEvent.Set();
            _renderUI.uiChanged();
        }

        internal string UI_GetStatusString()
        {
            return statusString;
        }

        internal List<string> UI_GetPostedMessages()
        {
            return postedMessages;
        }

        private void PostStatus(string statusString)
        {
            if (logWriter != null)
            {
                logWriter.Write("STATUS: " + statusString + "\n");
            }

            this.statusString = statusString;
            _renderUI.uiChanged();
        }

        public void PostMessage(string message)
        {
            if (logWriter != null)
            {
                logWriter.Write(message + "\n");
            }

            postedMessages.Add(message);
            _renderUI.uiChanged();
        }

        public void PostComplaint(NonredundantRenderComplaint complaint)
        {
            complaintBox.Complain(complaint);
        }

        public void PostImageResult(ImageRef image, Layer layer, string sourceMapName, TileAddress address)
        {
            //using (MemoryStream m = new MemoryStream())
            //{
            //    image.image.Save(m, ImageFormat.Png);

            //    var ret = m.ToArray();

            //    m.Close();
            //}

            var imageRef = (ImageRef)image.Duplicate("RenderState.PostImageResult");
            Monitor.Enter(this);
            ImageRef imageRef2;
            try
            {
                imageRef2 = lastRenderedImageRef;
                lastRenderedImageRef = imageRef;
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (imageRef2 != null)
            {
                imageRef2.Dispose();
            }

            lastRenderedImageLabel = new[] {layer.displayName, sourceMapName, address.ToString()};
            _renderUI.uiChanged();
        }

        private void ThreadTask()
        {
            try
            {
                RenderAll();
            }
            catch (Exception arg)
            {
                PostMessage(string.Format("Didn't expect to see an exception here: {0}", arg));
            }

            Exception failure = null;
            if (state != States.DoneRendering)
            {
                failure = new Exception("Rendering incomplete.");
            }

            _renderUI.notifyRenderComplete(failure);
        }

        private void CheckSignal()
        {
            if (disposeFlag)
            {
                throw new RenderDisposed();
            }

            if (pauseRenderingFlag)
            {
                state = States.Paused;
                pauseRenderingFlag = false;
                _renderUI.uiChanged();
                startRenderEvent.WaitOne();
                state = States.Rendering;
                _renderUI.uiChanged();
            }
        }

        private void RenderAll()
        {
            try
            {
                if (BuildConfig.theConfig.logInteractiveRenders)
                {
                    OpenLog();
                }

                startRenderEvent.WaitOne();
                CheckSignal();
                state = States.Rendering;
                _mashupDocument_UseScratchCopy.AutoSelectMaxZooms(mapTileSourceFactory);
                CheckSignal();
                mashupScratchCopy = DuplicateMashupDocumentForRender();
                CheckSignal();
                flushRenderedTileCachePackage();
                SetupRenderOutput();
                EstimateOuterLoop();
                CheckSignal();
                PurgeDirectory(renderOutput, "legends");
                CheckSignal();
                PostStatus("Creating XML mashup descriptor");
                var crunchedFile = CreateCrunchedFileDescriptor();
                PostStatus("Creating HTML sample file");
                sampleHTMLUri = SampleHTMLWriter.Write(mashupScratchCopy,
                    PostMessage,
                    renderOutput);
                CopySourceData();
                CheckSignal();
                PostStatus("Checking for reusable tiles from prior render");
                ArrangeLayerDirectories(renderOutput);
                CheckSignal();
                PostStatus("Rendering Legends");
                RenderLegends();
                CheckSignal();
                PurgeDirectory(renderOutput, "thumbnails");
                PostStatus("Rendering Thumbnails");
                RenderThumbnails(crunchedFile);
                CheckSignal();
                WriteCrunchedFileDescriptor(crunchedFile);
                PostStatus("Rendering");
                while (renderQueue.Count > 0)
                {
                    CheckSignal();
                    var renderWorkUnit = renderQueue.Dequeue();
                    bool flag = renderWorkUnit.DoWork(this);
                    if (flag)
                    {
                        _renderUI.uiChanged();
                    }

                    if (logWriter != null)
                    {
                        logWriter.Write("Completed: " + renderWorkUnit + "\n");
                    }
                }

                CommitManifest();
                PostStatus("Render complete.");
                state = States.DoneRendering;
                _renderUI.uiChanged();
            }
            catch (RenderDisposed)
            {
            }
            catch (RenderAborted)
            {
                state = States.Aborted;
            }
            catch (Exception ex)
            {
                PostMessage(string.Format("Something broke: {0}", ex.Message));
                state = States.Aborted;
            }
        }

        private Mashup DuplicateMashupDocumentForRender()
        {
            var mashup = _mashupDocument_UseScratchCopy.Duplicate();
            var list = new List<Layer>();
            foreach (var current in mashup.layerList)
            {
                if (!current.SomeSourceMapIsReadyToLock())
                {
                    list.Add(current);
                }
            }

            foreach (var current2 in list)
            {
                mashup.layerList.Remove(current2);
            }

            return mashup;
        }

        private void EstimateOuterLoop()
        {
            state = States.Rendering;
            D.Say(0, "EstimateOuterLoop starts");
            estimateProgressLayerCount = 0;
            renderQueue.Clear();
            var list = new List<RenderWorkUnit>();
            boundsList = new List<TileRectangle>();
            rangeQueryData = new RangeQueryData();
            bool flag = true;
            if (mashupScratchCopy != null)
            {
                foreach (var current in mashupScratchCopy.layerList)
                {
                    List<RenderWorkUnit> list2;
                    try
                    {
                        list2 = EstimateOneLayer(current, boundsList);
                    }
                    catch (RenderAborted)
                    {
                        flag = false;
                        break;
                    }
                    catch (RenderDisposed)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        PostMessage(string.Format("Skipping layer {0}: {1}", current.displayName, ex.ToString()));
                        D.Say(0, string.Format("ConstructRenderListThread failed: {0}", ex));
                        continue;
                    }

                    list2.Sort();
                    var list3 = new List<RangeDescriptor>();
                    rangeQueryData[current] = list3;
                    foreach (var current2 in list2)
                    {
                        if (list3.Count >= 100)
                        {
                            break;
                        }

                        if (current2 is CompositeTileUnit)
                        {
                            rangeQueryData[current]
                                .Add(new RangeDescriptor(((CompositeTileUnit)current2).GetTileAddress()));
                        }
                    }

                    list.AddRange(list2);
                    estimateProgressLayerCount++;
                    estimateProgressSourceMapCount = 0;
                    estimateProgressSourceMapsThisLayer = 1;
                    _renderUI.uiChanged();
                }
            }

            if (list.Count == 0)
            {
                PostMessage("Nothing to render.");
                flag = false;
            }

            if (flag)
            {
                PostStatus("Sorting");
                list.Sort();
                renderQueue = new Queue<RenderWorkUnit>(list);
                initialQueueSize = renderQueue.Count;
                string message = string.Format("Estimated output size: {0} tiles, about {1:f}MB",
                    renderQueue.Count,
                    renderQueue.Count * 0.085);
                estimateProgressLayerCount++;
                PostStatus(message);
                PostMessage(message);
                D.Say(0, "EstimateOuterLoop ends");
                return;
            }

            state = States.Aborted;
            PostStatus("Estimation canceled.");
            throw new RenderAborted();
        }

        private void SetupRenderOutput()
        {
            try
            {
                var renderToOptions = mashupScratchCopy.GetRenderOptions().renderToOptions;
                RenderOutputMethod baseMethod;
                if (renderToOptions is RenderToFileOptions)
                {
                    var renderToFileOptions = (RenderToFileOptions)renderToOptions;
                    if (renderToFileOptions.outputFolder == "")
                    {
                        throw new SetupFailed(true, "Please select an output folder.");
                    }

                    var fileOutputMethod = new FileOutputMethod(renderToFileOptions.outputFolder);
                    PostStatus(string.Format("Creating {0}", renderToFileOptions.outputFolder));
                    try
                    {
                        fileOutputMethod.CreateDirectory();
                    }
                    catch (Exception ex)
                    {
                        throw new SetupFailed(true, ex.Message);
                    }

                    baseMethod = fileOutputMethod;
                }
                else
                {
                    if (!(renderToOptions is RenderToS3Options))
                    {
                        throw new Exception("Unimplemented renderToOptions type");
                    }

                    var renderToS3Options = (RenderToS3Options)renderToOptions;
                    S3Credentials s3Credentials;
                    try
                    {
                        s3Credentials = new S3Credentials(renderToS3Options.s3credentialsFilename, false);
                    }
                    catch (Exception arg)
                    {
                        throw new SetupFailed(false,
                            string.Format("Can't load credentials file {0}: {1}",
                                renderToS3Options.s3credentialsFilename,
                                arg));
                    }

                    var s3adaptor = new S3Adaptor(s3Credentials.accessKeyId, s3Credentials.secretAccessKey);
                    var s3OutputMethod = new S3OutputMethod(s3adaptor,
                        renderToS3Options.s3bucket,
                        renderToS3Options.s3pathPrefix);
                    baseMethod = s3OutputMethod;
                }

                if (BuildConfig.theConfig.usingManifests)
                {
                    renderOutput = new ManifestOutputMethod(baseMethod);
                }
                else
                {
                    renderOutput = baseMethod;
                }
            }
            catch (SetupFailed setupFailed)
            {
                PostMessage(setupFailed.ToString());
                PostStatus(setupFailed.ToString());
                MessageBox.Show(setupFailed.ToString(), "Render Setup Failed");
                state = States.Aborted;
                throw new RenderAborted();
            }
        }

        private void CommitManifest()
        {
            if (renderOutput is ManifestOutputMethod)
            {
                ((ManifestOutputMethod)renderOutput).CommitChanges();
            }
        }

        private void DebugEmitRenderPlan(Queue<RenderWorkUnit> renderQueue)
        {
            var fileStream = new FileStream("RenderPlan.txt", FileMode.Create, FileAccess.Write);
            var streamWriter = new StreamWriter(fileStream);
            foreach (var current in renderQueue)
            {
                streamWriter.WriteLine(current.ToString());
            }

            streamWriter.Close();
            fileStream.Close();
        }

        private List<RenderWorkUnit> EstimateOneLayer(Layer layer, List<TileRectangle> boundsList)
        {
            if (layer.Count == 0)
            {
                return new List<RenderWorkUnit>();
            }

            EstimateLayer_SetupUI(layer);
            int spillCountBefore = EstimateLayer_PrepareToSelectRenderingStrategy();
            var sourceMapRenderInfosBackToFront = EstimateLayer_MakeSourceMapList(layer);
            var proposedTileSet =
                EstimateLayer_MakeProposedTileSet(layer, boundsList, sourceMapRenderInfosBackToFront);
            bool useStagedRendering = EstimateLayer_SelectRenderingStrategy(layer, spillCountBefore);
            var list = new List<CompositeTileUnit>(proposedTileSet.Values);
            list.Sort();
            return EstimateLayer_CreateRenderList(layer,
                sourceMapRenderInfosBackToFront,
                useStagedRendering,
                list);
        }

        private void EstimateLayer_SetupUI(Layer layer)
        {
            estimateProgressSourceMapCount = 0;
            estimateProgressSourceMapsThisLayer = layer.Count * 2 + 1;
            if (estimateProgressSourceMapsThisLayer == 0)
            {
                estimateProgressSourceMapsThisLayer = 1;
            }

            _renderUI.uiChanged();
        }

        private int EstimateLayer_PrepareToSelectRenderingStrategy()
        {
            mapTileSourceFactory.PurgeOpenSourceDocumentCache();
            return mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
        }

        private List<SourceMapRenderInfo> EstimateLayer_MakeSourceMapList(Layer layer)
        {
            var list = new List<SourceMapRenderInfo>();
            foreach (var current in layer.GetBackToFront())
            {
                CheckSignal();
                PostStatus(string.Format("(opening {0})", current.displayName));
                var sourceMapRenderInfo = new SourceMapRenderInfo();
                sourceMapRenderInfo.sourceMap = current;
                if (!current.ReadyToLock())
                {
                    PostMessage(string.Format("Skipping SourceMap {0} because it's not ready to lock.",
                        current.GetDisplayName()));
                }
                else
                {
                    try
                    {
                        sourceMapRenderInfo.warpedMapTileSource =
                            mapTileSourceFactory.CreateRenderableWarpedSource(current);
                    }
                    catch (Exception ex)
                    {
                        PostMessage(string.Format("Skipping SourceMap {0} because locking is failing: {1}.",
                            current.GetDisplayName(),
                            ex.ToString()));
                        continue;
                    }

                    list.Add(sourceMapRenderInfo);
                    estimateProgressSourceMapCount++;
                    _renderUI.uiChanged();
                }
            }

            return list;
        }

        private ProposedTileSet EstimateLayer_MakeProposedTileSet(Layer layer, List<TileRectangle> boundsList,
            List<SourceMapRenderInfo> sourceMapRenderInfosBackToFront)
        {
            int num = 0;
            var rect = layer.renderClip.rect;
            var proposedTileSet = new ProposedTileSet();
            foreach (var current in sourceMapRenderInfosBackToFront)
            {
                AddCredit(current.warpedMapTileSource.GetRendererCredit());
                current.warpedMapTileSource.GetOpenDocumentFuture(FutureFeatures.Cached)
                    .Realize("EstimateLayer_MakeProposedTileSet");
                var boundsPresent = (BoundsPresent)current.warpedMapTileSource
                    .GetUserBounds(null, FutureFeatures.Cached).Realize("RenderState.EstimateOneLayer");
                current.renderBoundsBoundingBox = boundsPresent.GetRenderRegion().GetBoundingBox();
                if (rect != null)
                {
                    current.renderBoundsBoundingBox = current.renderBoundsBoundingBox.ClipTo(rect);
                }

                current.renderBoundsBoundingBox = current.renderBoundsBoundingBox.ClipTo(
                    CoordinateSystemUtilities.GetRangeAsMapRectangle(MercatorCoordinateSystem.theInstance));
                var renderBounds =
                    mercatorCoordinateSystem.MakeRenderBounds(current.renderBoundsBoundingBox);
                string fileSuffix = "." + mashupScratchCopy.GetRenderOptions().outputTileType.extn;
                RenderedTileNamingScheme renderedTileNamingScheme =
                    new VENamingScheme(layer.GetFilesystemName(), fileSuffix);
                boundsPresent.Dispose();
                PostStatus(string.Format("(counting {0})", current.sourceMap.displayName));
                int num2 = Math.Max(current.sourceMap.sourceMapRenderOptions.minZoom, renderBounds.MinZoom);
                int num3 = Math.Min(current.sourceMap.sourceMapRenderOptions.maxZoom, renderBounds.MaxZoom);
                boundsList.Add(renderBounds.tileRectangle[num3]);
                string text = renderedTileNamingScheme.GetFileSuffix();
                D.Assert(text.StartsWith("."));
                text = text.Substring(1);
                for (int i = num2; i <= num3; i++)
                {
                    var tileRectangle = renderBounds.tileRectangle[i];
                    for (int j = tileRectangle.TopLeft.TileY;
                        j <= tileRectangle.BottomRight.TileY;
                        j += tileRectangle.StrideY)
                    {
                        for (int k = tileRectangle.TopLeft.TileX;
                            k <= tileRectangle.BottomRight.TileX;
                            k += tileRectangle.StrideX)
                        {
                            var tileAddress = new TileAddress(k, j, i);
                            proposedTileSet.MakeLayeredTileWork(tileAddress,
                                layer,
                                renderOutput.MakeChildMethod(renderedTileNamingScheme.GetFilePrefix()),
                                renderedTileNamingScheme.GetTileFilename(tileAddress),
                                mashupScratchCopy.GetRenderOptions().outputTileType);
                            CheckSignal();
                            if (!complainedAboutInsaneTileCount && proposedTileSet.Count > 1000000)
                            {
                                var dialogResult = MessageBox.Show(
                                    string.Format(
                                        "Estimate exceeds {0} tiles; consider canceling the estimation and selecting lower max zoom levels.",
                                        1000000),
                                    "That's a lot of tiles.",
                                    MessageBoxButtons.OKCancel,
                                    MessageBoxIcon.Exclamation);
                                if (dialogResult == DialogResult.Cancel)
                                {
                                    throw new RenderAborted();
                                }
                            }

                            num++;
                            if (num % 10000 == 0)
                            {
                                PostStatus(string.Format("(counting {0} ... {1} tiles)",
                                    current.sourceMap.displayName,
                                    proposedTileSet.Count));
                            }
                        }
                    }
                }

                estimateProgressSourceMapCount++;
            }

            return proposedTileSet;
        }

        private void AddCredit(string rendererCredit)
        {
            if (rendererCredit != null && !credits.ContainsKey(rendererCredit))
            {
                credits.Add(rendererCredit, true);
                PostMessage(rendererCredit);
            }
        }

        private bool EstimateLayer_SelectRenderingStrategy(Layer layer, int spillCountBefore)
        {
            bool result = false;
            int openSourceDocumentCacheSpillCount = mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
            if (openSourceDocumentCacheSpillCount == spillCountBefore)
            {
                mapTileSourceFactory.CreateUnwarpedSource(layer.First).GetOpenDocumentFuture(FutureFeatures.Cached)
                    .Realize("RenderState.EstimateOneLayer spill test");
                openSourceDocumentCacheSpillCount = mapTileSourceFactory.GetOpenSourceDocumentCacheSpillCount();
            }

            if (openSourceDocumentCacheSpillCount != spillCountBefore)
            {
                PostMessage(string.Format("Layer {0} spills memory; using staged rendering plan.",
                    layer.GetDisplayName()));
                result = true;
            }

            return result;
        }

        private List<RenderWorkUnit> EstimateLayer_CreateRenderList(Layer layer,
            List<SourceMapRenderInfo> sourceMapRenderInfosBackToFront, bool useStagedRendering,
            List<CompositeTileUnit> compositeTileUnits)
        {
            VETileSource vETileSource = null;
            if (layer.simulateTransparencyWithVEBackingLayer != null &&
                layer.simulateTransparencyWithVEBackingLayer != "")
            {
                vETileSource = new VETileSource(mapTileSourceFactory.GetCachePackage(),
                    layer.simulateTransparencyWithVEBackingLayer);
            }

            PostStatus(string.Format("Organizing {0}", layer.displayName));
            var layerApplierMaker = new LayerApplierMaker(mapTileSourceFactory.GetCachePackage());
            var list = new List<RenderWorkUnit>();
            int num = 0;
            foreach (var current in compositeTileUnits)
            {
                current.stage = num / 100;
                if (vETileSource != null)
                {
                    current.AddSupplier(layerApplierMaker.MakeApplier(vETileSource, layer));
                }

                var mapRectangle =
                    CoordinateSystemUtilities.TileAddressToMapRectangle(mercatorCoordinateSystem,
                        current.GetTileAddress());
                foreach (var current2 in sourceMapRenderInfosBackToFront)
                {
                    if (mapRectangle.intersects(current2.renderBoundsBoundingBox))
                    {
                        current.AddSupplier(layerApplierMaker.MakeApplier(current2.warpedMapTileSource, layer));
                    }
                }

                list.Add(current);
                if (useStagedRendering)
                {
                    foreach (var current3 in current.GetSingleSourceUnits())
                    {
                        list.Add(current3);
                    }
                }

                num++;
                CheckSignal();
            }

            return list;
        }

        private void ArrangeLayerDirectories(RenderOutputMethod baseOutputMethod)
        {
            try
            {
                var dictionary = new Dictionary<string, object>();
                var dictionary2 = new Dictionary<string, object>();
                foreach (var current in mashupScratchCopy.layerList)
                {
                    string filesystemName = current.GetFilesystemName();
                    var outputMethod = baseOutputMethod.MakeChildMethod(filesystemName);
                    var encodableHash = new EncodableHash();
                    current.AccumulateRobustHash_PerTile(mapTileSourceFactory.GetCachePackage(), encodableHash);
                    try
                    {
                        var layerMetadataFile = LayerMetadataFile.Read(outputMethod);
                        if (layerMetadataFile.encodableHash == encodableHash)
                        {
                            dictionary[filesystemName] = true;
                        }
                        else
                        {
                            dictionary2[filesystemName] = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (!(mashupScratchCopy.GetRenderOptions().renderToOptions is RenderToFileOptions))
                {
                    D.Sayf(0, "TODO: Generalize to S3 by recording layer manifest", new object[0]);
                }
                else
                {
                    string outputFolder =
                        ((RenderToFileOptions)mashupScratchCopy.GetRenderOptions().renderToOptions).outputFolder;
                    var directories = Directory.GetDirectories(outputFolder, "Layer_*");
                    for (int i = 0; i < directories.Length; i++)
                    {
                        string path = directories[i];
                        string fileName = Path.GetFileName(path);
                        if (!dictionary.ContainsKey(fileName))
                        {
                            dictionary2[fileName] = true;
                        }
                    }
                }

                foreach (string current2 in dictionary2.Keys)
                {
                    CheckSignal();
                    D.Assert(!dictionary.ContainsKey(current2));
                    try
                    {
                        PostMessage(string.Format("Deleting stale directory {0}", current2));
                        var renderOutputMethod = baseOutputMethod.MakeChildMethod(current2);
                        renderOutputMethod.EmptyDirectory();
                    }
                    catch (Exception arg)
                    {
                        PostMessage(string.Format(
                            "Failed to remove stale directory {0}; ignoring. Exception was {1}",
                            current2,
                            arg));
                    }
                }

                foreach (var current3 in mashupScratchCopy.layerList)
                {
                    string filesystemName2 = current3.GetFilesystemName();
                    var renderOutputMethod2 = baseOutputMethod.MakeChildMethod(filesystemName2);
                    var encodableHash2 = new EncodableHash();
                    current3.AccumulateRobustHash_PerTile(mapTileSourceFactory.GetCachePackage(), encodableHash2);
                    new LayerMetadataFile(renderOutputMethod2, encodableHash2).Write();
                }
            }
            catch (Exception ex)
            {
                PostMessage(string.Format(
                    "Cannot prepare output directory: {0}. Please correct the problem or select a different render output directory.",
                    ex.Message));
                throw new RenderAborted();
            }
        }

        private void PurgeDirectory(RenderOutputMethod baseOutputMethod, string dirName)
        {
            var method = baseOutputMethod.MakeChildMethod(dirName);
            PostStatus(string.Format("Deleting old {0} directory", dirName));
            Label_0019:
            try
            {
                method.EmptyDirectory();
            }
            catch (Exception exception)
            {
                PostMessage(string.Format(
                    "Cannot delete {0} directory: {1}. Please correct the problem or select a different render output directory.",
                    dirName,
                    exception.Message));
                pauseRenderingFlag = true;
                CheckSignal();
                goto Label_0019;
            }
        }

        private void RenderLegends()
        {
            var renderOutputMethod = renderOutput.MakeChildMethod("legends");
            foreach (var current in mashupScratchCopy.layerList)
            {
                foreach (var current2 in current)
                {
                    PostStatus("Opening " + current2.GetDisplayName() + " for legends.");
                    var displayableSource = mapTileSourceFactory.CreateUnwarpedSource(current2);
                    foreach (var current3 in current2.legendList)
                    {
                        CheckSignal();
                        string text = current2.GetLegendFilename(current3);
                        text = ForceValidFilename(text);
                        PostStatus("Rendering legend " + text);
                        ImageRef imageRef;
                        try
                        {
                            imageRef = current3.RenderLegend(displayableSource);
                        }
                        catch (Legend.RenderFailedException arg)
                        {
                            PostMessage(string.Format("Skipping {0}: {1}", text, arg));
                            continue;
                        }

                        try
                        {
                            RenderOutputUtil.SaveImage(imageRef,
                                renderOutputMethod,
                                text,
                                ImageTypeMapper.ByExtension(Path.GetExtension(text)).imageFormat);
                        }
                        catch (Exception arg2)
                        {
                            PostMessage(string.Format("Failed to save {0}: {1}", text, arg2));
                        }
                    }
                }
            }
        }

        private void RenderThumbnails(CrunchedFile crunchedFile)
        {
            var thumbnailOutput = renderOutput.MakeChildMethod("thumbnails");
            foreach (int current in new List<int> {200, 500})
            {
                foreach (var current2 in mashupScratchCopy.layerList)
                {
                    var crunchedLayer = crunchedFile[current2];
                    if (current2.SomeSourceMapIsReadyToLock())
                    {
                        RenderThumbnail(thumbnailOutput,
                            crunchedLayer,
                            ForceValidFilename(string.Format("{0}_{1}.png", current2.displayName, current)),
                            new CompositeTileSource(current2, mapTileSourceFactory),
                            current);
                    }

                    foreach (var current3 in current2)
                    {
                        if (current3.ReadyToLock())
                        {
                            var thumbnailCollection = crunchedLayer[current3];
                            RenderThumbnail(thumbnailOutput,
                                thumbnailCollection,
                                ForceValidFilename(string.Format("{0}_{1}_{2}.png",
                                    current2.displayName,
                                    current3.displayName,
                                    current)),
                                mapTileSourceFactory.CreateDisplayableWarpedSource(current3),
                                current);
                        }
                    }
                }
            }
        }

        private void RenderThumbnail(RenderOutputMethod thumbnailOutput, ThumbnailCollection thumbnailCollection,
            string thumbnailFilename, IDisplayableSource displayableSource, int maxImageDimension)
        {
            PostStatus("Rendering thumbnail " + thumbnailFilename);
            var latentRegionHolder = new LatentRegionHolder(new DirtyEvent(), new DirtyEvent());
            var present = displayableSource.GetUserBounds(latentRegionHolder, FutureFeatures.Cached)
                .Realize("RenderState.RenderThumbnails");
            if (!(present is BoundsPresent))
            {
                PostMessage(string.Format("Failure writing thumbnail {0}; skipping.", thumbnailFilename, present));
                return;
            }

            var boundingBox = ((BoundsPresent)present).GetRenderRegion().GetBoundingBox();
            new MercatorCoordinateSystem();
            var nW = MercatorCoordinateSystem.LatLonToMercator(boundingBox.GetSW());
            var sE = MercatorCoordinateSystem.LatLonToMercator(boundingBox.GetNE());
            var mapRectangle = new MapRectangle(nW, sE);
            var size = mapRectangle.SizeWithAspectRatio(maxImageDimension);
            var imagePrototype =
                displayableSource.GetImagePrototype(new ImageParameterFromRawBounds(size), FutureFeatures.Cached);
            var future = imagePrototype.Curry(new ParamDict(new object[]
            {
                TermName.ImageBounds, new MapRectangleParameter(boundingBox)
            }));
            var present2 = future.Realize("RenderState.RenderThumbnails");
            if (!(present2 is ImageRef))
            {
                PostMessage(string.Format("Failure writing thumbnail {0}; skipping: {1}",
                    thumbnailFilename,
                    present2));
                return;
            }

            try
            {
                RenderOutputUtil.SaveImage((ImageRef)present2,
                    thumbnailOutput,
                    thumbnailFilename,
                    ImageTypeMapper.ByExtension(Path.GetExtension(thumbnailFilename)).imageFormat);
                thumbnailCollection.Add(new ThumbnailRecord("thumbnails/" + thumbnailFilename, size));
            }
            catch (Exception arg)
            {
                PostMessage(string.Format("Failed to save {0}: {1}", thumbnailFilename, arg));
            }
        }

        private CrunchedFile CreateCrunchedFileDescriptor()
        {
            CrunchedFile crunchedFile = null;
            CrunchedFile result;
            try
            {
                string sourceMashupFilename = mashupScratchCopy.GetRenderOptions().publishSourceData
                    ? string.Format("{0}/{1}", SourceDataDirName, mashupScratchCopy.GetPublishedFilename())
                    : null;
                crunchedFile = new CrunchedFile(mashupScratchCopy,
                    rangeQueryData,
                    renderOutput,
                    sourceMashupFilename,
                    boundsList,
                    mapTileSourceFactory);
                result = crunchedFile;
            }
            catch (Exception arg)
            {
                PostMessage(string.Format("Couldn't generate XML output file {0}: {1}",
                    CrunchedFileLocation(crunchedFile),
                    arg));
                result = null;
            }

            return result;
        }

        private void WriteCrunchedFileDescriptor(CrunchedFile crunchedFile)
        {
            try
            {
                crunchedFile.WriteXML();
                crunchedFile.WriteSourceMapLegendFrames();
                renderedXMLDescriptor = renderOutput.GetUri(crunchedFile.GetRelativeFilename());
                _renderUI.uiChanged();
            }
            catch (Exception arg)
            {
                PostMessage(string.Format("Couldn't write XML output file {0}: {1}",
                    CrunchedFileLocation(crunchedFile),
                    arg));
            }
        }

        private string CrunchedFileLocation(CrunchedFile crunchedFile)
        {
            string result;
            try
            {
                result = crunchedFile.GetRelativeFilename();
            }
            catch (Exception)
            {
                result = "in " + mashupScratchCopy.GetRenderOptions().renderToOptions.ToString();
            }

            return result;
        }

        private void CopySourceData()
        {
            if (mashupScratchCopy.GetRenderOptions().publishSourceData)
            {
                PostStatus("Copying Source Data");
                var renderOutputMethod = renderOutput.MakeChildMethod(SourceDataDirName);
                var stream =
                    renderOutputMethod.CreateFile(mashupScratchCopy.GetPublishedFilename(), "text/xml");
                mashupScratchCopy.WriteXML(stream);
                stream.Close();
                foreach (var current in mashupScratchCopy.layerList)
                {
                    foreach (var current2 in current)
                    {
                        var sourceDocument =
                            current2.documentFuture.RealizeSynchronously(mapTileSourceFactory.GetCachePackage());
                        string filesystemAbsolutePath = sourceDocument.localDocument.GetFilesystemAbsolutePath();
                        PostStatus(string.Format("Copying {0}", current2.GetDisplayName()));
                        RenderOutputUtil.CopyFile(filesystemAbsolutePath,
                            renderOutputMethod,
                            Path.GetFileName(filesystemAbsolutePath),
                            ImageTypeMapper.ByExtension(Path.GetExtension(filesystemAbsolutePath)).mimeType);
                    }
                }
            }
        }

        private void CopyFile(string sourceFile, string targetDirectory, string mimeType)
        {
            CopyFileWithRename(sourceFile, targetDirectory, Path.GetFileName(sourceFile), mimeType);
        }

        private void CopyFileWithRename(string sourceFile, string targetDirectory, string targetFilename,
            string mimeType)
        {
            try
            {
                string relativeDestPath = Path.Combine(targetDirectory, targetFilename);
                RenderOutputUtil.CopyFile(sourceFile, renderOutput, relativeDestPath, mimeType);
            }
            catch (Exception ex)
            {
                PostMessage(string.Format("Failed to copy {0}: {1}", sourceFile, ex.ToString()));
            }
        }

        internal void UI_UpdateProgress(ProgressBar renderProgressBar)
        {
            if (mashupScratchCopy != null)
            {
                int num = estimateProgressSourceMapCount * 100 / estimateProgressSourceMapsThisLayer;
                int num2 = (mashupScratchCopy.layerList.Count + 1) * 100;
                int num3 = estimateProgressLayerCount * 100 + num;
                int num4 = initialQueueSize;
                int num5 = initialQueueSize - renderQueue.Count;
                if (num5 < 0)
                {
                    num5 = 0;
                }

                if (num5 > initialQueueSize)
                {
                    num5 = initialQueueSize;
                }

                double num6 = num3 * 1.0 / num2 * 0.1;
                double num7 = 0.0;
                if (num4 > 0)
                {
                    num7 = num5 * 1.0 / num4 * 0.9;
                }

                double num8 = num6 + num7;
                num8 = Math.Max(Math.Min(num8, 1.0), 0.0);
                renderProgressBar.Minimum = 0;
                renderProgressBar.Maximum = 1000;
                renderProgressBar.Value = (int)(1000.0 * num8);
                renderProgressBar.Enabled = state == States.Rendering;
                return;
            }

            renderProgressBar.Minimum = 0;
            renderProgressBar.Maximum = 0;
            renderProgressBar.Value = 0;
            renderProgressBar.Enabled = false;
        }

        internal void UI_UpdateLinks(LinkLabel previewRenderedResultsLinkLabel, LinkLabel viewInBrowserLinkLabel)
        {
            previewRenderedResultsLinkLabel.Visible = renderedXMLDescriptor != null;
            viewInBrowserLinkLabel.Visible = sampleHTMLUri != null;
        }

        internal ImageRef UI_GetLastRenderedImageRef()
        {
            Monitor.Enter(this);
            ImageRef result;
            try
            {
                var imageRef = lastRenderedImageRef;
                if (imageRef == null)
                {
                    result = null;
                }
                else
                {
                    result = (ImageRef)imageRef.Duplicate("RenderState.UI_GetLastRenderedImageRef");
                }
            }
            finally
            {
                Monitor.Exit(this);
            }

            return result;
        }

        internal string[] UI_GetTileDisplayLabel()
        {
            return lastRenderedImageLabel;
        }

        public Uri GetRenderedXMLDescriptor()
        {
            return renderedXMLDescriptor;
        }

        public Uri GetSampleHTMLUri()
        {
            return sampleHTMLUri;
        }

        public static string ForceValidFilename(string inStr)
        {
            string text = "";
            string text2 = new string(Path.GetInvalidFileNameChars()) + "%\\\"'& :<>";
            for (int i = 0; i < inStr.Length; i++)
            {
                char c = inStr[i];
                if (text2.IndexOf(c) < 0)
                {
                    text += c;
                }
            }

            return text;
        }
    }
}
