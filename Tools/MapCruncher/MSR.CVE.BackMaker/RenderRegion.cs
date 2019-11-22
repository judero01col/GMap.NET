using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class RenderRegion : IRobustlyHashable
    {
        private const string RenderRegionTag = "RenderRegion";
        private List<LatLon> vertexList = new List<LatLon>();
        private int cachedHashCode;
        public DirtyEvent dirtyEvent;

        internal int Count
        {
            get
            {
                return vertexList.Count;
            }
        }

        public RenderRegion(MapRectangle rect, DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            vertexList.Add(rect.GetNW());
            vertexList.Add(rect.GetSW());
            vertexList.Add(rect.GetSE());
            vertexList.Add(rect.GetNE());
        }

        internal RenderRegion Copy(DirtyEvent parentDirty)
        {
            return new RenderRegion(new List<LatLon>(vertexList), parentDirty);
        }

        public RenderRegion(List<LatLon> vertexList, DirtyEvent parentDirty)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            this.vertexList = vertexList;
        }

        public RenderDebug IntersectWithRectangleDebug(MapRectangle mapWindow)
        {
            RenderDebug renderDebug = new RenderDebug();
            renderDebug.IntersectedVertexList = new List<TracedVertex>();
            List<TracedVertex> list = new List<TracedVertex>();
            int num = 0;
            foreach (LatLon current in vertexList)
            {
                list.Add(new TracedVertex(num, current));
                num++;
            }

            TracedVertex tracedVertex = list[list.Count - 1];
            foreach (TracedVertex current2 in list)
            {
                ParametricLine parametricLine = new ParametricLine(tracedVertex.position, current2.position);
                List<ParametricLine.Intersection> list2 = new List<ParametricLine.Intersection>();
                list2.Add(parametricLine.LatitudeIntersection(mapWindow.lat0));
                list2.Add(parametricLine.LatitudeIntersection(mapWindow.lat1));
                list2.Add(parametricLine.LongitudeIntersection(mapWindow.lon0));
                list2.Add(parametricLine.LongitudeIntersection(mapWindow.lon1));
                list2.Sort();
                foreach (ParametricLine.Intersection current3 in list2)
                {
                    if (!current3.IsParallel && current3.t > 0.0 && current3.t < 1.0)
                    {
                        LatLon position = parametricLine.t(current3.t);
                        renderDebug.IntersectedVertexList.Add(new TracedVertex(tracedVertex.originalIndex, position));
                    }
                }

                renderDebug.IntersectedVertexList.Add(current2);
                tracedVertex = current2;
            }

            double val;
            double val2;
            if (mapWindow.lat1 > mapWindow.lat0)
            {
                val = mapWindow.lat0;
                val2 = mapWindow.lat1;
            }
            else
            {
                val = mapWindow.lat1;
                val2 = mapWindow.lat0;
            }

            double val3;
            double val4;
            if (mapWindow.lon1 > mapWindow.lon0)
            {
                val3 = mapWindow.lon0;
                val4 = mapWindow.lon1;
            }
            else
            {
                val3 = mapWindow.lon1;
                val4 = mapWindow.lon0;
            }

            renderDebug.FinalClipRegion = new List<TracedVertex>();
            tracedVertex = null;
            foreach (TracedVertex current4 in renderDebug.IntersectedVertexList)
            {
                LatLon position2 = current4.position;
                LatLon position3 = new LatLon(Math.Max(val, Math.Min(position2.lat, val2)),
                    Math.Max(val3, Math.Min(position2.lon, val4)));
                TracedVertex tracedVertex2 = new TracedVertex(current4.originalIndex, position3);
                if (tracedVertex == null || tracedVertex.position.lat != tracedVertex2.position.lat ||
                    tracedVertex.position.lon != tracedVertex2.position.lon)
                {
                    renderDebug.FinalClipRegion.Add(tracedVertex2);
                }

                tracedVertex = tracedVertex2;
            }

            return renderDebug;
        }

        private List<TracedVertex> IntersectWithRectangle(MapRectangle mapWindow)
        {
            RenderDebug renderDebug = IntersectWithRectangleDebug(mapWindow);
            return renderDebug.FinalClipRegion;
        }

        public Region GetClipRegion(MapRectangle mapWindow, int zoom, CoordinateSystemIfc csi)
        {
            if (csi is MercatorCoordinateSystem)
            {
                Region region = null;
                for (int i = -360; i <= 0; i += 360)
                {
                    MapRectangle clippedMapWindow = new MapRectangle(mapWindow.lat0,
                        mapWindow.lon0 + i,
                        mapWindow.lat1,
                        mapWindow.lon1 + i);
                    Region clipRegionComponent = GetClipRegionComponent(clippedMapWindow, zoom, csi);
                    if (region == null)
                    {
                        region = clipRegionComponent;
                    }
                    else
                    {
                        region.Union(clipRegionComponent);
                    }
                }

                return region;
            }

            return GetClipRegionComponent(mapWindow, zoom, csi);
        }

        private Region GetClipRegionComponent(MapRectangle clippedMapWindow, int zoom, CoordinateSystemIfc csi)
        {
            TracedScreenPoint[] path = GetPath(clippedMapWindow, zoom, csi);
            PointF[] array = new PointF[path.GetLength(0)];
            for (int i = 0; i < path.GetLength(0); i++)
            {
                array[i] = path[i].pointf;
            }

            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddLines(array);
            graphicsPath.CloseFigure();
            return new Region(graphicsPath);
        }

        public TracedScreenPoint[] GetPath(MapRectangle mapWindow, int zoom, CoordinateSystemIfc csi)
        {
            MapRectangle mapWindow2 = mapWindow.GrowFraction(0.1);
            List<TracedVertex> list = IntersectWithRectangle(mapWindow2);
            TracedScreenPoint[] array = new TracedScreenPoint[list.Count];
            int num = 0;
            foreach (TracedVertex current in list)
            {
                array[num] = new TracedScreenPoint(current.originalIndex,
                    csi.GetTranslationInPixels(new LatLonZoom(mapWindow.GetNW().lat, mapWindow.GetNW().lon, zoom),
                        current.position));
                num++;
            }

            return array;
        }

        public void UpdatePoint(int index, LatLon newPosition)
        {
            vertexList[index] = newPosition;
            SetDirty();
        }

        public List<LatLon> GetAsLatLonList()
        {
            return vertexList;
        }

        internal LatLon GetPoint(int index)
        {
            return vertexList[index];
        }

        internal void InsertPoint(int index, LatLon newPosition)
        {
            vertexList.Insert(index, newPosition);
            SetDirty();
        }

        internal void RemovePoint(int index)
        {
            vertexList.RemoveAt(index);
            SetDirty();
        }

        public static string GetXMLTag()
        {
            return "RenderRegion";
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("RenderRegion");
            foreach (LatLon current in vertexList)
            {
                current.WriteXML(writer);
            }

            writer.WriteEndElement();
        }

        public RenderRegion(MashupParseContext context, DirtyEvent parentDirty, CoordinateSystemIfc coordSys)
        {
            dirtyEvent = new DirtyEvent(parentDirty);
            XMLTagReader xMLTagReader = context.NewTagReader("RenderRegion");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(LatLon.GetXMLTag()))
                {
                    vertexList.Add(new LatLon(context, coordSys));
                }
            }
        }

        public override int GetHashCode()
        {
            return cachedHashCode;
        }

        public void SetDirty()
        {
            cachedHashCode = 19;
            foreach (LatLon arg_1D_0 in vertexList)
            {
                cachedHashCode = cachedHashCode * 131 + vertexList.GetHashCode();
            }

            dirtyEvent.SetDirty();
        }

        internal void AccumulateBoundingBox(ref MapRectangle boundingBox)
        {
            foreach (LatLon current in vertexList)
            {
                boundingBox = MapRectangle.AddToBoundingBox(boundingBox, current);
            }
        }

        internal MapRectangle GetBoundingBox()
        {
            MapRectangle result = null;
            AccumulateBoundingBox(ref result);
            return result;
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            foreach (LatLon current in vertexList)
            {
                current.AccumulateRobustHash(hash);
            }
        }
    }
}
