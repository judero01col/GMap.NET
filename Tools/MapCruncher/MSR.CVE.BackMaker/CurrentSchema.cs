namespace MSR.CVE.BackMaker
{
    public class CurrentSchema : MashupXMLSchemaVersion
    {
        public static CurrentSchema _schema;

        public static CurrentSchema schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new CurrentSchema();
                }

                return _schema;
            }
        }

        private CurrentSchema() : base("1.7")
        {
        }
    }
}
