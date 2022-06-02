using System.Collections.Generic;
using Newtonsoft.Json;

namespace GMap.NET.Entity
{
    public class OpenStreetMapGraphHopperRouteEntity
    {
        public Hints hints { get; set; }
        public Info info { get; set; }
        public List<Path> paths { get; set; }

        public class Details
        {

        }

        public class Hints
        {
            [JsonProperty("visited_nodes.sum")]
            public int VisitedNodesSum { get; set; }

            [JsonProperty("visited_nodes.average")]
            public double VisitedNodesAverage { get; set; }
        }

        public class Info
        {
            public List<string> copyrights { get; set; }
            public int took { get; set; }
        }

        public class Instruction
        {
            public double distance { get; set; }
            public double heading { get; set; }
            public int sign { get; set; }
            public List<int> interval { get; set; }
            public string text { get; set; }
            public int time { get; set; }
            public string street_name { get; set; }
            public double? last_heading { get; set; }
        }

        public class Path
        {
            public double distance { get; set; }
            public double weight { get; set; }
            public int time { get; set; }
            public int transfers { get; set; }
            public bool points_encoded { get; set; }
            public List<double> bbox { get; set; }
            public string points { get; set; }
            public List<Instruction> instructions { get; set; }
            public List<object> legs { get; set; }
            public Details details { get; set; }
            public double ascend { get; set; }
            public double descend { get; set; }
            public string snapped_waypoints { get; set; }
        }
    }

    public class OpenStreetMapGraphHopperGeocodeEntity
    {
        public List<Hit> hits { get; set; }
        public string locale { get; set; }

        public class Hit
        {
            public Point point { get; set; }
            public List<double> extent { get; set; }
            public string name { get; set; }
            public string country { get; set; }
            public string countrycode { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public long osm_id { get; set; }
            public string osm_type { get; set; }
            public string osm_key { get; set; }
            public string osm_value { get; set; }
            public string postcode { get; set; }
        }

        public class Point
        {
            public double lat { get; set; }
            public double lng { get; set; }
        }
    }
}
