
namespace Demo.WindowsForms
{
   using System.Windows.Forms;
   using GMap.NET.WindowsForms;
   using System.Drawing;
   using System;
   using System.Globalization;

   /// <summary>
   /// custom map of GMapControl
   /// </summary>
   public class Map : GMapControl
   {
      public long ElapsedMilliseconds;

#if DEBUG
      private int _counter;
      readonly Font _debugFont = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Regular);
      readonly Font _debugFontSmall = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
      DateTime _start;
      DateTime _end;
      int _delta;

      protected override void OnPaint(PaintEventArgs e)
      {
         _start = DateTime.Now;

         base.OnPaint(e);

         _end = DateTime.Now;
         _delta = (int)(_end - _start).TotalMilliseconds;
      }

      /// <summary>
      /// any custom drawing here
      /// </summary>
      /// <param name="drawingContext"></param>
      protected override void OnPaintOverlays(Graphics g)
      {
         base.OnPaintOverlays(g);

         g.DrawString(string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider + ", refresh: " + _counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + _delta + "ms", _debugFont, Brushes.Blue, _debugFont.Height, _debugFont.Height + 20);

         //g.DrawString(ViewAreaPixel.Location.ToString(), DebugFontSmall, Brushes.Blue, DebugFontSmall.Height, DebugFontSmall.Height);

         //string lb = ViewAreaPixel.LeftBottom.ToString();
         //var lbs = g.MeasureString(lb, DebugFontSmall);
         //g.DrawString(lb, DebugFontSmall, Brushes.Blue, DebugFontSmall.Height, Height - DebugFontSmall.Height * 2);

         //string rb = ViewAreaPixel.RightBottom.ToString();
         //var rbs = g.MeasureString(rb, DebugFontSmall);
         //g.DrawString(rb, DebugFontSmall, Brushes.Blue, Width - rbs.Width - DebugFontSmall.Height, Height - DebugFontSmall.Height * 2);

         //string rt = ViewAreaPixel.RightTop.ToString();
         //var rts = g.MeasureString(rb, DebugFontSmall);
         //g.DrawString(rt, DebugFontSmall, Brushes.Blue, Width - rts.Width - DebugFontSmall.Height, DebugFontSmall.Height);
      }     
#endif
   }
}
