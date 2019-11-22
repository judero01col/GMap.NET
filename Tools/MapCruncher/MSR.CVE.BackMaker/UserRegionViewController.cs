using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    internal class UserRegionViewController
    {
        private struct State
        {
            public LatLonZoom center;
            public Size size;
            public bool valid;

            public override bool Equals(object o2)
            {
                if (o2 is State)
                {
                    State state = (State)o2;
                    return center == state.center && size == state.size && valid == state.valid;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return center.GetHashCode() ^ size.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", center, size);
            }

            public State(State state)
            {
                center = state.center;
                size = new Size(state.size.Width, state.size.Height);
                valid = state.valid;
            }
        }

        private class ClickableThing
        {
            public enum ClickedWhich
            {
                Vertex,
                Segment
            }

            public TracedScreenPoint vertexLocation;
            public GraphicsPath path;
            public ClickedWhich clickedWhich;
            public int pointIndex;
        }

        private class VertexMouseAction : ViewerControl.MouseAction
        {
            private UserRegionViewController controller;
            private int draggedVertexIndex;

            public VertexMouseAction(UserRegionViewController controller, int draggedVertexIndex)
            {
                this.controller = controller;
                this.draggedVertexIndex = draggedVertexIndex;
            }

            public void Dragged(Point diff)
            {
                controller.DragVertex(diff, draggedVertexIndex);
            }

            public Cursor GetCursor(bool dragging)
            {
                return Cursors.Arrow;
            }

            public void OnPopup(ContextMenu menu)
            {
                MenuItem menuItem = menu.MenuItems.Add("Remove corner", new EventHandler(RemoveCorner));
                menuItem.Enabled = controller.RemoveEnabled();
            }

            public void RemoveCorner(object sender, EventArgs e)
            {
                controller.RemoveCorner(draggedVertexIndex);
            }
        }

        private class SegmentMouseAction : ViewerControl.MouseAction
        {
            private UserRegionViewController controller;
            private int originalVertexIndex;
            private Point clickedPoint;
            private int menuIncarnation;
            private static int menuIncarnationCounter;

            public SegmentMouseAction(UserRegionViewController controller, int originalVertexIndex, Point clickedPoint)
            {
                this.controller = controller;
                this.originalVertexIndex = originalVertexIndex;
                menuIncarnationCounter++;
                menuIncarnation = menuIncarnationCounter;
                this.clickedPoint = clickedPoint;
            }

            public void Dragged(Point diff)
            {
            }

            public Cursor GetCursor(bool dragging)
            {
                return Cursors.Cross;
            }

            public void OnPopup(ContextMenu menu)
            {
                menu.MenuItems.Add("Add corner", new EventHandler(AddCorner));
                D.Say(0, string.Format("Updating menu from incarnation {0}", menuIncarnation));
            }

            public void AddCorner(object sender, EventArgs e)
            {
                D.Say(0, string.Format("AddCorner from incarnation {0}", menuIncarnation));
                controller.AddCorner(clickedPoint, originalVertexIndex);
            }
        }

        private const int vertexRadius = 3;
        private const int edgeClickWidth = 4;
        private const int vertexClickRadius = 4;
        private CoordinateSystemIfc csi;
        private SVDisplayParams svdp;
        private LatentRegionHolder latentRegionHolder;
        private IDisplayableSource displayableSource;
        private Brush vertexFillBrush;
        private Pen vertexStrokePen;
        private Brush segmentFillBrush;
        private State lastState;
        private ClickableThing[] clickableThings;

        public UserRegionViewController(CoordinateSystemIfc csi, SVDisplayParams svdp,
            LatentRegionHolder latentRegionHolder, IDisplayableSource unwarpedMapTileSource)
        {
            this.csi = csi;
            this.svdp = svdp;
            this.latentRegionHolder = latentRegionHolder;
            displayableSource = unwarpedMapTileSource;
            vertexFillBrush = new SolidBrush(Color.LightBlue);
            vertexStrokePen = new Pen(Color.DarkBlue, 1f);
            segmentFillBrush = new SolidBrush(Color.DarkBlue);
        }

        private void UpdateState(State state)
        {
            if (state.Equals(lastState))
            {
                return;
            }

            TracedScreenPoint[] path = GetUserRegion()
                .GetPath(CoordinateSystemUtilities.GetBounds(csi, state.center, state.size),
                    state.center.zoom,
                    csi);
            List<TracedScreenPoint> list = new List<TracedScreenPoint>();
            int length = path.GetLength(0);
            for (int i = 0; i < length; i++)
            {
                D.Assert(path[i].originalIndex >= 0);
                int num = (i + length - 1) % length;
                if (!(path[num].pointf == path[i].pointf))
                {
                    list.Add(path[i]);
                }
            }

            list.ToArray();
            int count = list.Count;
            List<ClickableThing> list2 = new List<ClickableThing>();
            List<ClickableThing> list3 = new List<ClickableThing>();
            for (int j = 0; j < count; j++)
            {
                ClickableThing clickableThing = new ClickableThing();
                list2.Add(clickableThing);
                clickableThing.vertexLocation = list[j];
                clickableThing.path = new GraphicsPath();
                clickableThing.path.AddEllipse(list[j].pointf.X - 4f, list[j].pointf.Y - 4f, 8f, 8f);
                clickableThing.clickedWhich = ClickableThing.ClickedWhich.Vertex;
                clickableThing.pointIndex = j;
                ClickableThing clickableThing2 = new ClickableThing();
                list3.Add(clickableThing2);
                clickableThing2.vertexLocation = list[j];
                clickableThing2.path = new GraphicsPath();
                clickableThing2.path.AddLine(list[j].pointf, list[(j + 1) % count].pointf);
                clickableThing2.path.Widen(new Pen(Color.Black, 4f));
                clickableThing2.clickedWhich = ClickableThing.ClickedWhich.Segment;
                clickableThing2.pointIndex = j;
            }

            list2.AddRange(list3);
            clickableThings = list2.ToArray();
            lastState = new State(state);
        }

        internal RenderRegion GetUserRegion()
        {
            return latentRegionHolder.renderRegion;
        }

        internal void Paint(PaintSpecification e, LatLonZoom center, Size size)
        {
            if (GetUserRegion() == null)
            {
                return;
            }

            State state;
            state.center = center;
            state.size = size;
            state.valid = true;
            UpdateState(state);
            ClickableThing[] array = clickableThings;
            for (int i = 0; i < array.Length; i++)
            {
                ClickableThing clickableThing = array[i];
                e.Graphics.FillPath(segmentFillBrush, clickableThing.path);
            }
        }

        internal ViewerControl.MouseAction ImminentAction(MouseEventArgs e)
        {
            if (clickableThings == null)
            {
                return null;
            }

            int i = 0;
            while (i < clickableThings.GetLength(0))
            {
                ClickableThing clickableThing = clickableThings[i];
                if (clickableThing.vertexLocation.originalIndex >= 0 && clickableThing.path.IsVisible(e.Location))
                {
                    if (clickableThing.clickedWhich == ClickableThing.ClickedWhich.Segment)
                    {
                        return new SegmentMouseAction(this, clickableThing.vertexLocation.originalIndex, e.Location);
                    }

                    return new VertexMouseAction(this, clickableThing.vertexLocation.originalIndex);
                }
                else
                {
                    i++;
                }
            }

            return null;
        }

        internal void DragVertex(Point diff, int draggedVertexIndex)
        {
            LatLon point = GetUserRegion().GetPoint(draggedVertexIndex);
            LatLonZoom center = new LatLonZoom(point.lat, point.lon, svdp.MapCenter().zoom);
            diff = new Point(-diff.X, -diff.Y);
            LatLonZoom translationInLatLon = csi.GetTranslationInLatLon(center, diff);
            GetUserRegion().UpdatePoint(draggedVertexIndex, translationInLatLon.latlon);
            Invalidate();
        }

        internal void AddCorner(Point newCornerPoint, int originalVertexIndex)
        {
            LatLonZoom center = svdp.MapCenter();
            Point offsetInPixels = new Point(svdp.ScreenCenter().X - newCornerPoint.X,
                svdp.ScreenCenter().Y - newCornerPoint.Y);
            LatLonZoom translationInLatLon = csi.GetTranslationInLatLon(center, offsetInPixels);
            D.Say(0, string.Format("newCornerPosition= {0}", translationInLatLon));
            GetUserRegion().InsertPoint(originalVertexIndex + 1, translationInLatLon.latlon);
            Invalidate();
        }

        internal void RemoveCorner(int originalVertexIndex)
        {
            GetUserRegion().RemovePoint(originalVertexIndex);
            Invalidate();
        }

        internal bool RemoveEnabled()
        {
            return GetUserRegion().Count > 3;
        }

        private void Invalidate()
        {
            AsyncRef asyncRef = (AsyncRef)displayableSource
                .GetUserBounds(latentRegionHolder, (FutureFeatures)7)
                .Realize("UserRegionViewController.Invalidate");
            asyncRef.ProcessSynchronously();
            asyncRef.Dispose();
            svdp.InvalidateView();
            lastState.valid = false;
        }
    }
}
