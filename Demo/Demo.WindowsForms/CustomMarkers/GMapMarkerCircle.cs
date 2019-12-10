
namespace Demo.WindowsForms.CustomMarkers
{
   using System;
   using System.Drawing;
   using System.Runtime.Serialization;
   using GMap.NET;
   using GMap.NET.WindowsForms;

   [Serializable]
   public class GMapMarkerCircle : GMapMarker, ISerializable
   {
      /// <summary>
      /// In Meters
      /// </summary>
      public int Radius;

      /// <summary>
      /// specifies how the outline is painted
      /// </summary>
      [NonSerialized]
      public Pen Stroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));

      /// <summary>
      /// background color
      /// </summary>
      [NonSerialized]
      public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));

      /// <summary>
      /// is filled
      /// </summary>
      public bool IsFilled = true;

      public GMapMarkerCircle(PointLatLng p)
         : base(p)
      {
         Radius = 888; // 888m
         IsHitTestVisible = false;
      }

      public override void OnRender(Graphics g)
      {
         int r = (int)((Radius) / Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, Position.Lat)) * 2;

         if(IsFilled)
         {
            g.FillEllipse(Fill, new Rectangle(LocalPosition.X - r / 2, LocalPosition.Y - r / 2, r, r));
         }
         g.DrawEllipse(Stroke, new Rectangle(LocalPosition.X - r / 2, LocalPosition.Y - r / 2, r, r));
      }

      public override void Dispose()
      {
         if(Stroke != null)
         {
            Stroke.Dispose();
            Stroke = null;
         }

         if(Fill != null)
         {
            Fill.Dispose();
            Fill = null;
         }

         base.Dispose();
      }

      #region ISerializable Members

      void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
      {
         base.GetObjectData(info, context);

         // TODO: Radius, IsFilled
      }

      protected GMapMarkerCircle(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
         // TODO: Radius, IsFilled
      }

      #endregion
   }
}
