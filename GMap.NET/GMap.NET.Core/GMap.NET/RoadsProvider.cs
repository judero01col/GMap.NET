
namespace GMap.NET
{
    using System.Collections.Generic;

    /// <summary>
    /// roads interface
    /// </summary>
    public interface RoadsProvider
    {        
        MapRoute GetRoadsRoute(List<PointLatLng> points, bool interpolate);
       
        MapRoute GetRoadsRoute(string points, bool interpolate);
    }
}
