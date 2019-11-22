using System.Collections.Generic;

namespace GMap.NET
{
    /// <summary>
    ///     roads interface
    /// </summary>
    public interface RoadsProvider
    {
        MapRoute GetRoadsRoute(List<PointLatLng> points, bool interpolate);

        MapRoute GetRoadsRoute(string points, bool interpolate);
    }
}
