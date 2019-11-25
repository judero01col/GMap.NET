using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Demo.Testing
{
    public partial class Form1 : Form
    {
        public GMapOverlay OverlayMarkers = new GMapOverlay("Markers");
        public GMapOverlay OverlayRoutes = new GMapOverlay("Routes");
        public GMapOverlay OverlayPolygons = new GMapOverlay("Polygons");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            MainMap.RoutesEnabled = true;
            MainMap.PolygonsEnabled = true;
            MainMap.MarkersEnabled = true;
            MainMap.NegativeMode = false;
            MainMap.RetryLoadTile = 0;
            MainMap.ShowTileGridLines = false;
            MainMap.AllowDrop = true;
            MainMap.IgnoreMarkerOnMouseWheel = true;
            MainMap.DragButton = MouseButtons.Left;
            MainMap.DisableFocusOnMouseEnter = true;
            MainMap.MinZoom = 0;
            MainMap.MaxZoom = 24;
            MainMap.Zoom = 9;
            MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
            MainMap.MapProvider = GMapProviders.GoogleMap;

            MainMap.Overlays.Add(OverlayMarkers);
            MainMap.Overlays.Add(OverlayRoutes);
            MainMap.Overlays.Add(OverlayPolygons);

            MainMap.OnMapClick += UserControlGMap_OnMapClick;
        }

        private void UserControlGMap_OnMapClick(PointLatLng PointClick, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GMapMarker myCity = new GMarkerGoogle(PointClick, GMarkerGoogleType.green_small);
                myCity.ToolTipMode = MarkerTooltipMode.Always;
                myCity.ToolTipText = "Welcome to Lithuania! ;}";
                OverlayMarkers.Markers.Add(myCity);
            }
        }
    }
}
