using System;
using Gtk;

namespace Demo.GtkSharp
{
    class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Gtk.Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Gtk.Application.Run();
        }
    }
}
