namespace MSR.CVE.BackMaker
{
    public class ViewsNotAsWellPreservedSchema : MashupXMLSchemaVersion
    {
        public static ViewsNotAsWellPreservedSchema _schema;

        public static ViewsNotAsWellPreservedSchema schema
        {
            get
            {
                if (_schema == null)
                {
                    _schema = new ViewsNotAsWellPreservedSchema();
                }

                return _schema;
            }
        }

        private ViewsNotAsWellPreservedSchema() : base("1.5")
        {
        }
    }
}
