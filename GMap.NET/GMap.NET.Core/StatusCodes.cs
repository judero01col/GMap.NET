namespace GMap.NET
{
    /// <summary>
    ///     GeoCoder StatusCode
    /// </summary>
    public enum GeoCoderStatusCode : int
    {
        UNKNOWN_ERROR,
        OK,
        ZERO_RESULTS,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        INVALID_REQUEST,
        ERROR,
        EXCEPTION_IN_CODE,
    }

    /// <summary>
    ///     Direction StatusCode
    /// </summary>
    public enum DirectionsStatusCode : int
    {
        UNKNOWN_ERROR,
        OK = 0,
        NOT_FOUND,
        ZERO_RESULTS,
        MAX_WAYPOINTS_EXCEEDED,
        INVALID_REQUEST,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        EXCEPTION_IN_CODE,
    }

    /// <summary>
    ///     Direction StatusCode
    /// </summary>
    public enum RouteStatusCode
    {
        UNKNOWN_ERROR,
        OK,
        INVALID_REQUEST,
        OVER_QUERY_LIMIT,
        REQUEST_DENIED,
        ZERO_RESULTS,
        INVALID_ARGUMENT,
        PERMISSION_DENIED,
        ERROR,
        EXCEPTION_IN_CODE,
    }
}
