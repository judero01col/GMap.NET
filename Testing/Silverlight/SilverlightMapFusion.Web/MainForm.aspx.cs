using System;
using System.Diagnostics;
using System.Web.UI;
using GMap.NET;

namespace SilverlightMapFusion.Web
{
    public partial class MainForm : Page
    {
        static MainForm()
        {
            try
            {
                //GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                // or
                //GMapProvider.WebProxy = new WebProxy("127.0.0.1", 1080);
                //GMapProvider.IsSocksProxy = true;

                GMaps.Instance.EnableTileHost(8844);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainForm: " + ex);
                throw;
            }

            //GMaps.Instance.DisableTileHost();
            //GMaps.Instance.CancelTileCaching();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
