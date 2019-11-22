namespace MSR.CVE.BackMaker
{
    public class NoTagIdentities : MashupXMLSchemaVersion
    {
        public static NoTagIdentities _schema;

        public static NoTagIdentities schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new NoTagIdentities();
                }

                return _schema;
            }
        }

        private NoTagIdentities() : base("1.6")
        {
        }
    }
}
