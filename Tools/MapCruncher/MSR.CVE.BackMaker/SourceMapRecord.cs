using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapRecord : ThumbnailCollection
    {
        public const string ReferenceNameAttr = "ReferenceName";
        public const string sourceMapRecordTag = "SourceMapRecord";
        public const string displayNameAttr = "DisplayName";
        public const string maxZoomAttr = "MaxZoom";
        internal string displayName;
        private SourceMapLegendFrame sourceMapLegendFrame;
        private SourceMapInfo sourceMapInfo;
        private MapRectangle userBoundingRect;
        private IImageTransformer imageTransformer;
        private int maxZoom;
        private List<LegendRecord> legendRecords = new List<LegendRecord>();
        private List<ThumbnailRecord> thumbnailRecords = new List<ThumbnailRecord>();

        //[CompilerGenerated]
        //private static Comparison<ThumbnailRecord> <>9__CachedAnonymousMethodDelegate1;

        public SourceMapRecord(Layer layer, SourceMap sourceMap, MapTileSourceFactory mapTileSourceFactory)
        {
            displayName = sourceMap.displayName;
            sourceMapInfo = sourceMap.sourceMapInfo;
            userBoundingRect = sourceMap.GetUserBoundingBox(mapTileSourceFactory);
            maxZoom = sourceMap.sourceMapRenderOptions.maxZoom;
            try
            {
                imageTransformer =
                    sourceMap.registration.warpStyle.getImageTransformer(sourceMap.registration,
                        InterpolationMode.Invalid);
            }
            catch (Exception)
            {
            }

            foreach (Legend current in sourceMap.legendList)
            {
                legendRecords.Add(new LegendRecord("legends",
                    sourceMap.GetLegendFilename(current),
                    current.displayName,
                    current.GetOutputSizeSynchronously(mapTileSourceFactory.CreateDisplayableUnwarpedSource(sourceMap)
                        .GetUserBounds(current.latentRegionHolder, FutureFeatures.Cached))));
            }

            sourceMapLegendFrame = new SourceMapLegendFrame(layer,
                sourceMap,
                legendRecords,
                thumbnailForLegendFrame);
        }

        public SourceMapRecord(SourceMapInfo sourceMapInfo)
        {
            displayName = "";
            this.sourceMapInfo = sourceMapInfo;
            userBoundingRect = null;
        }

        public SourceMapRecord(MashupParseContext context)
        {
            displayName = context.GetRequiredAttribute("DisplayName");
            XMLTagReader xMLTagReader = context.NewTagReader("SourceMapRecord");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(SourceMapInfo.GetXMLTag()))
                {
                    sourceMapInfo = new SourceMapInfo(context, new DirtyEvent());
                }
                else
                {
                    if (xMLTagReader.TagIs(MapRectangle.GetXMLTag()))
                    {
                        userBoundingRect = new MapRectangle(context, MercatorCoordinateSystem.theInstance);
                    }
                    else
                    {
                        if (xMLTagReader.TagIs(LegendRecord.GetXMLTag()))
                        {
                            legendRecords.Add(new LegendRecord(context));
                        }
                        else
                        {
                            if (xMLTagReader.TagIs(SourceMapLegendFrame.GetXMLTag()))
                            {
                                context.AssertUnique(sourceMapLegendFrame);
                                sourceMapLegendFrame = new SourceMapLegendFrame(context);
                            }
                        }
                    }
                }
            }
        }

        public static string GetXMLTag()
        {
            return "SourceMapRecord";
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(GetXMLTag());
            writer.WriteAttributeString("DisplayName", displayName);
            writer.WriteAttributeString("ReferenceName", SampleHTMLWriter.ReferenceName(displayName));
            writer.WriteAttributeString("MaxZoom", maxZoom.ToString());
            sourceMapLegendFrame.WriteXML(writer);
            sourceMapInfo.WriteXML(writer);
            if (userBoundingRect != null)
            {
                userBoundingRect.WriteXML(writer);
            }

            foreach (LegendRecord current in legendRecords)
            {
                current.WriteXML(writer);
            }

            if (imageTransformer != null)
            {
                imageTransformer.writeToXml(writer);
            }

            foreach (ThumbnailRecord current2 in thumbnailRecords)
            {
                current2.WriteXML(writer);
            }

            writer.WriteEndElement();
        }

        public void WriteSourceMapLegendFrame(RenderOutputMethod renderOutput)
        {
            sourceMapLegendFrame.WriteSourceMapLegendFrame(renderOutput);
        }

        private ThumbnailRecord thumbnailForLegendFrame()
        {
            if (thumbnailRecords.Count == 0)
            {
                return null;
            }

            ThumbnailRecord[] array = thumbnailRecords.ToArray();
            Array.Sort(array,
                (ThumbnailRecord r0, ThumbnailRecord r1) =>
                    Math.Abs(Math.Max(r0.size.Width, r0.size.Height) - 200) -
                    Math.Abs(Math.Max(r1.size.Width, r1.size.Height) - 200));
            return array[0];
        }

        public void Add(ThumbnailRecord thumbnailRecord)
        {
            thumbnailRecords.Add(thumbnailRecord);
        }
    }
}
