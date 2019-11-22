namespace MSR.CVE.BackMaker
{
    public class SourceMapInfoAsCharDataSchema : MashupXMLSchemaVersion
    {
        public static SourceMapInfoAsCharDataSchema _schema;

        public static SourceMapInfoAsCharDataSchema schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new SourceMapInfoAsCharDataSchema();
                }

                return _schema;
            }
        }

        private SourceMapInfoAsCharDataSchema() : base("1.3")
        {
        }
    }
}
