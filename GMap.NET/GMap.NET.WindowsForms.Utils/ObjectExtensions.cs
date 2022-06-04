using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms.Markers;
using LINQPad;
using Polyline = Polylines.Polyline;

namespace GMap.NET.WindowsForms.MapUtils
{
    public static class ObjectExtensions
    {
        private static readonly object PanelSync = new object();
        public static IEnumerable<T> DumpMarkers<T>(this IEnumerable<T> items, Func<T, (double latitude, double longitude)> coordinateSelector, Func<T, string> tooltipSelector = null, string panel = "Map", string overlay = "Markers", Func<T, GMarkerGoogleType> markerTypeSelector = null, Action<T, GoogleMarkerWithData> config = null)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));

            var mapControl = GetMapControl(null, panel);

            var mapOverlay = mapControl.GetMapOverlay(overlay);

            var list = new List<T>();

            foreach (var item in items)
            {
                var marker = new GoogleMarkerWithData(coordinateSelector(item), markerTypeSelector?.Invoke(item) ?? GMarkerGoogleType.red, item)
                {
                    ToolTipText = tooltipSelector?.Invoke(item) ?? item.ToString()
                };

                config?.Invoke(item, marker);
                mapOverlay.Markers.Add(marker);
                list.Add(item);
            }
            return list;
        }

        public static IEnumerable<T> DumpRoute<T>(this IEnumerable<T> items, Func<T, (double latitude, double longitude)> coordinateSelector, string routeName = "Route", string panel = "Map", string overlay = "Routes", Action<GMapRoute> config = null)
        {
            if (panel == null) throw new ArgumentNullException(nameof(panel));

            var mapControl = GetMapControl(null, panel);

            var mapOverlay = mapControl.GetMapOverlay(overlay);
            var route = new GMapRoute(routeName);
            config?.Invoke(route);
            mapOverlay.Routes.Add(route);
            var list = new List<T>();
            foreach (var item in items)
            {
                var (latitude, longitude) = coordinateSelector(item);
                route.Points.Add(new PointLatLng(latitude, longitude));
                list.Add(item);
            }
            return list;
        }

        public static IEnumerable<T> DumpRoute<T>(this IEnumerable<T> items, Func<T, string> encodedPolylineSelector, Func<T, string> routeNameSelector = null, string panel = "Map", string overlay = "Routes", Action<T, GMapRoute> config = null)
        {
            var list = new List<T>();
            foreach (var item in items)
            {
                Polyline.DecodePolyline(encodedPolylineSelector(item))
                    .DumpRoute(x => (x.Latitude, x.Longitude), routeNameSelector?.Invoke(item), panel, overlay, r => config?.Invoke(item, r));
                list.Add(item);
            }
            return list;
        }

        public static GMapOverlay GetMapOverlay(this GMapControl mapControl, string overlay)
        {
            GMapOverlay mapOverlay;
            lock (mapControl)
            {
                mapOverlay = mapControl.Overlays.FirstOrDefault(o => o.Id == overlay);
                if (mapOverlay == null)
                {
                    mapOverlay = new GMapOverlay(overlay);
                    mapControl.Overlays.Add(mapOverlay);
                }
            }
            return mapOverlay;
        }

        public static GMapControl GetMapControl(this DataContextBase _, string panel = "Map")
        {
            GMapControl mapControl;
            lock (PanelSync)
            {
                var outputPanel = PanelManager.GetOutputPanel(panel);
                if (outputPanel == null)
                {
                    mapControl = new GMapControl
                    {
                        MapProvider = GMapProviders.GoogleMap,
                        MinZoom = 0,
                        MaxZoom = 18,
                        CanDragMap = true,
                        DragButton = MouseButtons.Left,
                        IgnoreMarkerOnMouseWheel = true,
                    };
                    var timer = new System.Timers.Timer();
                    timer.Elapsed += (sender, e) =>
                    {
                        timer.Stop();
                        var rect = mapControl.Overlays
                            .SelectMany(o => new[]
                            {
                            mapControl.GetRectOfAllMarkers(o.Id),
                            mapControl.GetRectOfAllRoutes(o.Id),
                            })
                            .Where(r => r.HasValue)
                            .Select(r => r.Value)
                            .Aggregate((c, a) => RectLatLng.Union(a, c));
                        mapControl.SetZoomToFitRect(rect);
                        timer.Dispose();
                    };
                    mapControl.Load += (sender, e) => timer.Start();
                    mapControl.OnMarkerClick += (marker, sender) =>
                    {
                        switch (marker)
                        {
                            case GoogleMarkerWithData markerWithData:
                                markerWithData.Item.Dump();
                                break;
                        }
                    };
                    PanelManager.DisplayControl(mapControl, panel);
                }
                else
                {
                    mapControl = (GMapControl)outputPanel.GetControl();
                }
            }
            return mapControl;
        }

        public class GoogleMarkerWithData : GMarkerGoogle
        {
            public object Item { get; }

            public GoogleMarkerWithData((double latitude, double longitude) coords, GMarkerGoogleType markerType, object item)
                : base(new PointLatLng(coords.latitude, coords.longitude), markerType)
            {
                Item = item;
            }
        }
    }
}
