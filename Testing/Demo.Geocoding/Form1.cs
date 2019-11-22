using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace Demo.Geocoding
{
    public partial class FormMainMap : Form
    {
        GMapOverlay objects = new GMapOverlay("objects");

        int Exito;
        int Falla;
        int Total;

        public FormMainMap()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MainMap.Position = new PointLatLng(11.0041072, -74.8069813);
            MainMap.MinZoom = 0;
            MainMap.MaxZoom = 24;
            MainMap.Zoom = 9;
            MainMap.Overlays.Add(objects);
            MainMap.MapProvider = GMapProviders.GoogleMap;

            GoogleMapProvider.Instance.ApiKey = "AIzaSyAmO6pIPTz0Lt8lmYZEIAaixitKjq-4WlB";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            Exito = 0;
            Falla = 0;
            Total = 0;

            label1.Text = "Total: " + Total;
            label2.Text = "Exito: " + Exito;
            label3.Text = "Falla: " + Falla;

            string sLine = "";
            List<string> arrText = new List<string>();

            using (StreamReader objReader = new StreamReader("D:\\DireccionPrueba10012019.txt"))
            {
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();

                    if (sLine != null)
                    {
                        sLine = sLine.Trim();
                        sLine = sLine.Replace("  ", "");
                        sLine = sLine.Replace("N�", "#");
                        sLine = sLine.Replace("N°", "#");
                        sLine = sLine.Trim();

                        arrText.Add(sLine);
                    }
                }

                objReader.Close();
            }


            //Parallel.For(0, arrText.Count - 1, (i, state) =>
            //{
            //    Total += 1;
            //    AddLocation("Colombia", "Atlantico", "Barranquilla", arrText[i]);
            //});


            if (arrText != null && arrText.Count > 0)
            {
                arrText = arrText.Distinct().ToList();

                for (int i = 0; i < arrText.Count - 1; i++)
                {
                    Total += 1;
                    ProcesarDatos("Colombia", "Atlantico", "Barranquilla", arrText[i]).Wait();
                }

                //foreach (string sOutput in arrText)
                //    AddLocation("Colombia", "Atlantico", "Barranquilla", sOutput);

                //Parallel.ForEach(arrText, line =>
                //{
                //    Total += 1;

                //    GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
                //    PointLatLng? pos = GMapProviders.GoogleMap.GetPoint("Colombia, Atlantico, Barranquilla," + line, out status);

                //    if (pos != null && status == GeoCoderStatusCode.G_GEO_SUCCESS)
                //    {
                //        GMapMarker myCity = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green_small);
                //        myCity.ToolTipText = line;
                //        myCity.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                //        objects.Markers.Add(myCity);

                //        Exito += 1;
                //    }
                //    else
                //    {
                //        Falla += 1;
                //    }
                //});
            }

            label1.Text = "Total: " + Total;
            label2.Text = "Exito: " + Exito;
            label3.Text = "Falla: " + Falla;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MainMap.Overlays.Add(objects);

            MainMap.ImportFromKmz("D:\\BARRANQUILLA.kmz");
        }

        public Task ProcesarDatos(string Country, string Department, string City, string Address)
        {
            Task task = Task.Run(() =>
            {
                GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;

                string fulladress = (string.IsNullOrEmpty(Country) ? "" : Country + ", ") +
                                    (string.IsNullOrEmpty(Department) ? "" : Department + ", ") +
                                    (string.IsNullOrEmpty(City) ? "" : City + ", " + Address);

                PointLatLng? pos = GMapProviders.GoogleMap.GetPoint(fulladress, out status);

                if (pos != null && status == GeoCoderStatusCode.OK)
                {
                    GMapMarker myCity = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green_small);
                    myCity.ToolTipText = Address;
                    myCity.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    objects.Markers.Add(myCity);

                    Exito += 1;
                }
                else
                {
                    Falla += 1;
                }

                if (Exito % 100 == 0)
                {
                }

                if (Total == Exito + Falla)
                {
                }
            });
            return task;
        }

        void AddLocation(string Country, string Department, string City, string Address)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;

            string fulladress = (string.IsNullOrEmpty(Country) ? "" : Country + ", ") +
                                (string.IsNullOrEmpty(Department) ? "" : Department + ", ") +
                                (string.IsNullOrEmpty(City) ? "" : City + ", " + Address);

            PointLatLng? pos = GMapProviders.GoogleMap.GetPoint(fulladress, out status);

            if (pos != null && status == GeoCoderStatusCode.OK)
            {
                GMapMarker myCity = new GMarkerGoogle(pos.Value, GMarkerGoogleType.green_small);
                myCity.ToolTipText = Address;
                myCity.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                objects.Markers.Add(myCity);

                Exito += 1;
            }
            else
            {
                Falla += 1;
            }
        }
    }
}
