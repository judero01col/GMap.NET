namespace MSR.CVE.BackMaker
{
    public class MonolithicMapPositionsSchema : MashupXMLSchemaVersion
    {
        public const string monolithicMapPositionTag = "MapPosition";
        public const string monolithicLatAttr = "lat";
        public const string monolithicLonAttr = "lon";
        public const string monolithicZoomAttr = "zoom";
        public const string monolithicStyleAttr = "style";
        public static MonolithicMapPositionsSchema _schema;

        public static MonolithicMapPositionsSchema schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new MonolithicMapPositionsSchema();
                }

                return _schema;
            }
        }

        private MonolithicMapPositionsSchema() : base("1.0")
        {
        }
    }
}
