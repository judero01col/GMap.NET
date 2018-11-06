# GMap.NET - Maps For Windows

![GMap.Net](https://raw.githubusercontent.com/judero01col/GMap.NET/master/GMap.ico "this is the result")

GMap.NET is great and Powerful, Free, cross platform, open source .NET control. Enable use routing, geocoding, directions and maps from Google, Yahoo!, Bing, OpenStreetMap, ArcGIS, Pergo, SigPac, Yendux, Mapy.cz, Maps.lt, iKarte.lv, NearMap, HereMap, CloudMade, WikiMapia, MapQuest in Windows Forms & Presentation, Supports caching and runs on Windows Forms, Presentation & Mobile!

# Installing
gmaps-api-net is available as a Nuget package from https://www.nuget.org/packages/GMap.NET.Windows/ and installable using the following command in the Package Manager Console:
```
PM> Install-Package GMap.NET.Windows
```

If you wish to use the cutting-edge version of gmaps-api-net, then you can clone the repository (or download the zip) and build the class library yourself. This should require minimal set-up, and also allows you to develop extra features for your own use (or to push upstream using a pull request for everyone else to use!).

# Documentation
https://github.com/judero01col/GMap.NET/wiki

# Release Notes

## Version 1.9.0
### GMap.NET.Windows Core
- Compatibility with Framework 4.0
- New Constructor GMapRoute(MapRoute oRoute)
- New UMP-pcPL Map Provided Added
- BingOSMapProvider Fix
- Routing instructions for OpenStreetMap
- Added TTL cache in GMapProvider, GeocodingProvider, RoutingProvider, DirectionsProvider, RoadsProvider
- Reimplement multi-touch method Contributed by bymindzz
### GMap.NET.Windows Forms
- Added IsZoomSignificant and IsHitTestVisible to overlay
### GMap.NET.WindowsPresentation
- Moved CreateRoutePath, CreatePolygonPath
- Regenerate Shape fix

## Version 1.8.8
### GMap.NET.Windows Core
- RoadsProvider Implementation
- Fixed GetDirections of DirectionsProvider
- Message return and error code of the GetRote method of DirectionsProvider
- Call Interface GeocodingProvider, DirectionsProvider, RoutingProvider from GMapControl
- Primary cache error correction of RoutingProvider, DirectionsProvider, GeocodingProvider
- Correction of duration field of the OpenStreetMap provider
### GMap.NET.Windows Forms
- Correction of error event double click  the GMapControl

## Version 1.8.7
### GMap.NET.Windows Core
- Call Interface GeocodingProvider, DirectionsProvider, RoutingProvider from GMapControl
- Primary cache error correction of RoutingProvider, DirectionsProvider, GeocodingProvider
- Correction of duration field of the OpenStreetMap provider
### GMap.NET.Windows Forms
- Correction of error event double click  the GMapControl

## Version 1.8.5
### GMap.NET.Windows Core
- Added time to live cache (BingMapProvider and GoogleMapProvider).
- Added Mouse double click event on Marker Polygon and Route
- Added Provider Bing Ordnance Survey (UK) 
- Added Methods to calculate distances from routes
- Set virtual keyword to RoutingProvider methods
- Check if a given point is within the given point based map boundary
### GMap.NET.Windows Presentation
- Wpf control for binding
- Adding CenterPosition Dependency Property which allows us to easily update map position based on position
- Adding an additional check if any Mouse button is being pressed on OnMouseMove event on GMapControl.cs before calling BeginDrag
- Added pinch and zoom gestures via manipulation events. 
- Deprecated TouchEnabled and made it a dependency property so you can turn it on and off. 
- TouchEnabled is now set to false by default instead of true. 
- Added multi touch code via manipulation events to handle any number of touch events. 
- Panning via single touch and pinch/zoom effect handled for greater than or equal to 2 points. 
- Created dependency propertiy for MultiTouchEnabled so it can be configured via xaml

