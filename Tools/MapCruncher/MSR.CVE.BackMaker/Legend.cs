using System;
using System.Drawing;
using System.Globalization;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class Legend : PositionMemoryIfc, HasDisplayNameIfc, LastViewIfc
    {
        public class RenderFailedException : Exception
        {
            public RenderFailedException(string msg) : base(msg)
            {
            }
        }

        private const string displayNameAttr = "DisplayName";
        private const string renderedSizeAttr = "RenderedSize";
        public static RangeInt renderedSizeRange = new RangeInt(50, 1000);
        public DirtyEvent dirtyEvent;
        private string _displayName;
        private int _renderedSize = 500;
        private LegendView _lastView;

        public LatentRegionHolder latentRegionHolder
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

        public SourceMap sourceMap
        {
            get;
        }

        public int renderedSize
        {
            get
            {
                return _renderedSize;
            }
            set
            {
                if (value != _renderedSize)
                {
                    _renderedSize = value;
                    dirtyEvent.SetDirty();
                }
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

        public Legend(SourceMap sourceMap, DirtyEvent parentEvent, DirtyEvent parentBoundsChangedEvent)
        {
            this.sourceMap = sourceMap;
            dirtyEvent = new DirtyEvent(parentEvent);
            latentRegionHolder = new LatentRegionHolder(dirtyEvent, parentBoundsChangedEvent);
            _displayName = "legend";
        }

        public static string GetXMLTag()
        {
            return "Legend";
        }

        public Legend(SourceMap sourceMap, MashupParseContext context, DirtyEvent parentEvent,
            DirtyEvent parentBoundsChangedEvent)
        {
            this.sourceMap = sourceMap;
            dirtyEvent = new DirtyEvent(parentEvent);
            latentRegionHolder = new LatentRegionHolder(dirtyEvent, parentBoundsChangedEvent);
            _displayName = context.GetRequiredAttribute("DisplayName");
            string attribute = context.reader.GetAttribute("RenderedSize");
            if (attribute != null)
            {
                renderedSizeRange.Parse(context, "RenderedSize", attribute);
            }

            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            context.ExpectIdentity(this);
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(RenderRegion.GetXMLTag()))
                {
                    context.AssertUnique(latentRegionHolder.renderRegion);
                    latentRegionHolder.renderRegion = new RenderRegion(context,
                        dirtyEvent,
                        ContinuousCoordinateSystem.theInstance);
                }
                else
                {
                    if (xMLTagReader.TagIs(LegendView.GetXMLTag()))
                    {
                        _lastView = new LegendView(this, context);
                    }
                }
            }
        }

        public void WriteXML(MashupWriteContext context)
        {
            context.writer.WriteStartElement(GetXMLTag());
            context.WriteIdentityAttr(this);
            context.writer.WriteAttributeString("DisplayName", _displayName);
            context.writer.WriteAttributeString("RenderedSize",
                renderedSize.ToString(CultureInfo.InvariantCulture));
            if (latentRegionHolder.renderRegion != null)
            {
                latentRegionHolder.renderRegion.WriteXML(context.writer);
            }

            if (_lastView != null)
            {
                _lastView.WriteXML(context);
            }

            context.writer.WriteEndElement();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(_displayName);
            latentRegionHolder.renderRegion.AccumulateRobustHash(hash);
        }

        internal LegendView GetLastView()
        {
            return _lastView;
        }

        public void NotePositionUnlocked(LatLonZoom sourceMapPosition, MapPosition referenceMapPosition)
        {
            bool showingPreview = false;
            _lastView = new LegendView(this, showingPreview, sourceMapPosition, referenceMapPosition);
        }

        public void NotePositionLocked(MapPosition referenceMapPosition)
        {
            D.Assert(false, "legend view never locked");
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public void SetDisplayName(string value)
        {
            displayName = value;
        }

        public IFuture GetRenderedLegendFuture(IDisplayableSource displayableSource, FutureFeatures features)
        {
            RenderRegion renderRegion = latentRegionHolder.renderRegion;
            if (renderRegion == null)
            {
                throw new RenderFailedException("Region unavailable");
            }

            renderRegion = renderRegion.Copy(new DirtyEvent());
            MapRectangleParameter mapRectangleParameter = new MapRectangleParameter(renderRegion.GetBoundingBox());
            Size outputSize = OutputSizeFromRenderRegion(renderRegion);
            IFuturePrototype imagePrototype =
                displayableSource.GetImagePrototype(new ImageParameterFromRawBounds(outputSize), features);
            return imagePrototype.Curry(new ParamDict(new object[] {TermName.ImageBounds, mapRectangleParameter}));
        }

        private Size OutputSizeFromRenderRegion(RenderRegion renderRegion)
        {
            MapRectangleParameter mapRectangleParameter = new MapRectangleParameter(renderRegion.GetBoundingBox());
            return mapRectangleParameter.value.SizeWithAspectRatio(renderedSize);
        }

        public ImageRef RenderLegend(IDisplayableSource displayableSource)
        {
            Present present = GetRenderedLegendFuture(displayableSource, FutureFeatures.Cached)
                .Realize("Legend.RenderLegend");
            if (!(present is ImageRef))
            {
                throw new RenderFailedException("Render failed: " + present.ToString());
            }

            return (ImageRef)present;
        }

        internal Size GetOutputSizeSynchronously(IFuture synchronousUserBoundsFuture)
        {
            RenderRegion renderRegionSynchronously =
                latentRegionHolder.GetRenderRegionSynchronously(synchronousUserBoundsFuture);
            return OutputSizeFromRenderRegion(renderRegionSynchronously);
        }
    }
}
