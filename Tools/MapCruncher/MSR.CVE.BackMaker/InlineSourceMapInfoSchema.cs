namespace MSR.CVE.BackMaker
{
    public class InlineSourceMapInfoSchema : MashupXMLSchemaVersion
    {
        public static InlineSourceMapInfoSchema _schema;

        public static InlineSourceMapInfoSchema schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new InlineSourceMapInfoSchema();
                }

                return _schema;
            }
        }

        private InlineSourceMapInfoSchema() : base("1.2")
        {
        }
    }
}
