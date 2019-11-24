# GMap.NET - Maps For Windows

![GMap.Net](https://raw.githubusercontent.com/judero01col/GMap.NET/master/GMap.ico "this is the result")

GMap.NET Windows Forms & Presentation is an excellent open source, powerful, free and cross-platform .NET control.
Allows the use of routing, geocoding, directions and maps from Google, Yahoo!, Bing, OpenStreetMap, ArcGIS, Pergo, SigPac, Yendux, Mapy.cz, Maps.lt, iKarte.lv, NearMap, HereMap, CloudMade, WikiMapia, MapQuest and many more.

# Installing
GMap.NET is available as a Nuget package from:

https://www.nuget.org/packages/GMap.NET.Core

https://www.nuget.org/packages/GMap.NET.WinForms

https://www.nuget.org/packages/GMap.NET.WinPresentation 

Are installable using the following command in the Package Manager Console:

```
PM> Install-Package GMap.NET.Core
```
```
PM> Install-Package GMap.NET.WinForms
```
```
PM> Install-Package GMap.NET.WinPresentation
```

If you wish to use the cutting-edge version of gmaps-api-net, then you can clone the repository (or download the zip) and build the class library yourself. This should require minimal set-up, and also allows you to develop extra features for your own use (or to push upstream using a pull request for everyone else to use!).

# Documentation
https://github.com/judero01col/GMap.NET/wiki

# Release Notes

## Version 1.9.9.5

#### GMap.NET.Core
- None

#### GMap.NET.WinForms
- Add event OnMapClick
- Add event OnMapDoubleClick

#### GMap.NET.WinPresentation
- None

## Version 1.9.9

#### GMap.NET
Project converted to .NET Core 3.0 (Contributed by @zgabi)

#### GMap.NET.Core
- Add default image proxy class. (Contributed by @zgabi)
- Added decode and encode polyline methods in the PureProjection class
- Code organization

#### GMap.NET.WinForms
- Add event OnMapClick

#### GMap.NET.WinPresentation
- Adding an additional check if any Mouse button is being pressed on OnMouseMove event on GMapControl before calling BeginDrag
code formatting. (Contributed by @zgabi)

## Version 1.9.7.2

Library migrated to .Net Core and published new Nuget packages.
- GMap.NET.Core
- GMap.NET.WinForms
- GMap.NET.WinPresentation

## Version 1.9.7

#### GMap.NET.WindowsCore
- Correct spelling 'Sattelite' to 'Satellite' throughout. (Contributed by @zgabi)
- Updated Sweden Map Url. (Contributed by @zgabi)
- Google Maps: Change HTTP to HTTPS. (Contributed by @zgabi)
- Change Czech/CzechTuristMapProvider URL to a working one. (Contributed by @zgabi)

#### GMap.NET.WindowsForms

#### GMap.NET.WindowsPresentation
- Adding an additional check if any Mouse button is being pressed on OnMouseMove event on GMapControl before calling BeginDrag
code formatting. (Contributed by @zgabi)

#### GMap.NET.Demo
- Add googlemaps api key to WPF project, too replace the key to another from the Demos.Geocoding project, which currently works. (Contributed by @zgabi)
- Fic MapCruncher Resharper errors. (Contributed by @zgabi)
- MVC sample fix. (Contributed by @zgabi)
- Corrected map name to match provider in Testing/Demo.Docking. (Contributed by @zgabi)
- leafletjs fixed in WinForms demo. (Contributed by @zgabi)

## Version 1.9.5

#### GMap.NET
- Project converted to .NET Core 3.0 (Contributed by @zgabi)

## Version 1.9.4

#### GMap.NET.Windows Forms
- A bit nicer representationof the map scale info. (Contributed by @DrSeuthberg)

## Version 1.9.3

### GMap.NET.Windows Core
- Connection error correction with OpenStreetMap provider. (Contributed by @jbavay)
- Brings distance rounding in line with the Overpass Ways length OSM. (Contributed by @rododevr)
- Event was added for notification of exceptions and errors. (Contributed by @DrSeuthberg)
#### GMap.NET.Windows Forms
- GMapOverlay - ensure tooltip is on top of all objects of all overlays (Contributed by @DrSeuthberg)
#### GMap.NET.WindowsPresentation
- Added methods that allow for background rendering of WPF map controls. (Contributed by @chrisg32)

## Version 1.9.2
### GMap.NET.Windows Core
- Fix Minor

## Version 1.9.1
### GMap.NET.Windows Core
Custom MapProvider Add

## Version 1.9.0
### GMap.NET.Windows Core
- Compatibility with Framework 4.0
- New Constructor GMapRoute(MapRoute oRoute)
- New UMP-pcPL Map Provided Added (Contributed by lukaszkn)
- BingOSMapProvider Fix
- Routing instructions for OpenStreetMap
- Added TTL cache in GMapProvider, GeocodingProvider, RoutingProvider, DirectionsProvider, RoadsProvider
- Reimplement multi-touch method (Contributed by bymindzz)
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

