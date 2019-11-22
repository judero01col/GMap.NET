namespace MSR.CVE.BackMaker
{
    public class VirtualEarthWebDownloader
    {
        public static string RoadStyle = "r";
        public static string AerialStyle = "a";
        public static string HybridStyle = "h";

        public static bool StyleIsValid(string s)
        {
            return s != null && (s == RoadStyle || s == AerialStyle || s == HybridStyle);
        }
    }
}
