using System.Threading;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class InsaneSourceMapRemover
    {
        public delegate void UndoAdddSourceMapDelegate(string message);

        private SourceMap sourceMap;
        private MapTileSourceFactory mapTileSourceFactory;
        private UndoAdddSourceMapDelegate undoAddSourceMapDelegate;
        private bool handled;
        private int tryCount;

        public InsaneSourceMapRemover(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory,
            UndoAdddSourceMapDelegate removeSourceMapDelegate) : this(sourceMap,
            mapTileSourceFactory,
            removeSourceMapDelegate,
            0)
        {
        }

        private InsaneSourceMapRemover(SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory,
            UndoAdddSourceMapDelegate removeSourceMapDelegate, int tryCount)
        {
            this.sourceMap = sourceMap;
            this.mapTileSourceFactory = mapTileSourceFactory;
            undoAddSourceMapDelegate = removeSourceMapDelegate;
            this.tryCount = tryCount;
            if (tryCount > 3)
            {
                return;
            }

            Present present = mapTileSourceFactory.CreateUnwarpedSource(sourceMap)
                .GetImagePrototype(new ImageParameterFromTileAddress(ContinuousCoordinateSystem.theInstance),
                    (FutureFeatures)7)
                .Curry(new ParamDict(new object[]
                {
                    TermName.TileAddress,
                    new TileAddress(0, 0, ContinuousCoordinateSystem.theInstance.GetDefaultView().zoom)
                })).Realize("SourceMap.CheckRendererSanity");
            AsyncRef asyncRef = (AsyncRef)present;
            asyncRef.AddCallback(RendererSanityCheckComplete);
            new PersistentInterest(asyncRef);
        }

        private void RendererSanityCheckComplete(AsyncRef asyncRef)
        {
            Monitor.Enter(this);
            try
            {
                if (handled)
                {
                    return;
                }

                handled = true;
            }
            finally
            {
                Monitor.Exit(this);
            }

            if (!(asyncRef.present is ImageRef))
            {
                if (asyncRef.present is RequestCanceledPresent)
                {
                    new InsaneSourceMapRemover(sourceMap,
                        mapTileSourceFactory,
                        undoAddSourceMapDelegate,
                        tryCount + 1);
                    return;
                }

                string message;
                if (asyncRef.present is PresentFailureCode)
                {
                    message = ((PresentFailureCode)asyncRef.present).exception.Message;
                }
                else
                {
                    message = string.Format("Unexpected result type {0}", asyncRef.present.GetType().ToString());
                }

                undoAddSourceMapDelegate(message);
                return;
            }
        }
    }
}
