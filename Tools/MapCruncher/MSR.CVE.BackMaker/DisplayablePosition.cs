using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class DisplayablePosition
    {
        public enum ErrorMarker
        {
            AsOutlier,
            AsContributor
        }

        private const int NumErrorMarkers = 2;
        private LatLonZoom _pinPosition;
        private ErrorPosition[] _errorPositions = new ErrorPosition[2];

        public LatLonZoom pinPosition
        {
            get
            {
                return _pinPosition;
            }
        }

        public bool invertError
        {
            get;
            set;
        }

        public DisplayablePosition(LatLonZoom position)
        {
            _pinPosition = position;
        }

        public static string GetXMLTag(MashupXMLSchemaVersion version)
        {
            if (version == MonolithicMapPositionsSchema.schema)
            {
                return MapPosition.GetXMLTag(version);
            }

            return LatLonZoom.GetXMLTag();
        }

        public DisplayablePosition(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            if (context.version == MonolithicMapPositionsSchema.schema)
            {
                MapPosition mapPosition = new MapPosition(context, null, coordSys);
                _pinPosition = mapPosition.llz;
                return;
            }

            _pinPosition = new LatLonZoom(context, coordSys);
        }

        public ErrorPosition GetErrorPosition(ErrorMarker errorMarker)
        {
            return _errorPositions[(int)errorMarker];
        }

        public void SetErrorPosition(ErrorMarker errorMarker, LatLon errorPosition)
        {
            _errorPositions[(int)errorMarker] = new ErrorPosition(errorPosition);
        }

        public void WriteXML(XmlTextWriter writer)
        {
            _pinPosition.WriteXML(writer);
        }

        public override int GetHashCode()
        {
            return _pinPosition.GetHashCode();
        }
    }
}
