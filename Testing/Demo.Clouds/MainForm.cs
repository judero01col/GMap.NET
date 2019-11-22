using System;
using System.Windows.Forms;
using Demo.Clouds.Properties;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;

namespace CloudsDemo
{
    public partial class MainForm : Form
    {
        // clouds boundaries
        readonly PointLatLng gtl = new PointLatLng(50.4066263673011, -127.620375523875);
        readonly PointLatLng gbr = new PointLatLng(21.652538062803, -66.517937876818);

        readonly GMapOverlay mainOverlay;
        readonly GMapImage clouds;

        public MainForm()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                try
                {
                    System.Net.IPHostEntry e = System.Net.Dns.GetHostEntry("www.bing.com");
                }
                catch
                {
                    mapControl.Manager.Mode = AccessMode.CacheOnly;
                    MessageBox.Show("No internet connection avaible, going to CacheOnly mode.",
                        "GMap.NET - CloudsDemo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                mapControl.MapProvider = GMapProviders.BingSatelliteMap;
                mapControl.OnMapZoomChanged += mapControl_OnMapZoomChanged;

                mainOverlay = new GMapOverlay("top");
                mapControl.Overlays.Add(mainOverlay);

                clouds = new GMapImage(gtl);
                clouds.Image = Resources.USOverlay;
                mainOverlay.Markers.Add(clouds);
            }
        }

        void mapControl_OnMapZoomChanged()
        {
            if (clouds != null)
            {
                var tl = mapControl.FromLatLngToLocal(gtl);
                var br = mapControl.FromLatLngToLocal(gbr);

                clouds.Position = gtl;
                clouds.Size = new System.Drawing.Size((int)(br.X - tl.X), (int)(br.Y - tl.Y));
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            mapControl.SetZoomToFitRect(RectLatLng.FromLTRB(gtl.Lng, gtl.Lat, gbr.Lng, gbr.Lat));
        }
    }
}
