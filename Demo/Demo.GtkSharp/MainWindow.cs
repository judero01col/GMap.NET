using System;
using Gtk;
using GMap.NET.MapProviders;
using GMap.NET;

public partial class MainWindow: Gtk.Window
{
	public MainWindow()
		: base(Gtk.WindowType.Toplevel)
	{
		Build();

		// config map         
		MainMap.MapProvider = GMapProviders.OpenStreetMap;
		MainMap.Position = new PointLatLng(59.93900, 30.31646);
		MainMap.MinZoom = 0;
		MainMap.MaxZoom = 24;
		MainMap.Zoom = 9;   
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Gtk.Application.Quit();
		a.RetVal = true;
	}

	public override void Destroy()
	{
		MainMap.Destroy();
		base.Destroy();
	}
}
