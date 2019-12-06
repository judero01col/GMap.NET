using GMap.NET.MapProviders;

namespace GMap.NET
{
    public interface Interface
    {
        PointLatLng Position
        {
            get;
            set;
        }

        GPoint PositionPixel
        {
            get;
        }

        string CacheLocation
        {
            get;
            set;
        }

        bool IsDragging
        {
            get;
        }

        RectLatLng ViewArea
        {
            get;
        }

        GMapProvider MapProvider
        {
            get;
            set;
        }

        bool CanDragMap
        {
            get;
            set;
        }

        RenderMode RenderMode
        {
            get;
        }

        // events
        event PositionChanged OnPositionChanged;
        event TileLoadComplete OnTileLoadComplete;
        event TileLoadStart OnTileLoadStart;
        event MapDrag OnMapDrag;
        event MapZoomChanged OnMapZoomChanged;
        event MapTypeChanged OnMapTypeChanged;

        void ReloadMap();

        PointLatLng FromLocalToLatLng(int x, int y);
        GPoint FromLatLngToLocal(PointLatLng point);

#if SQLite
        bool ShowExportDialog();
        bool ShowImportDialog();
#endif
    }
}
