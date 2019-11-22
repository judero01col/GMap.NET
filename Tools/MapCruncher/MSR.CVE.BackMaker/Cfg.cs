namespace MSR.CVE.BackMaker
{
    internal abstract class Cfg<T> : ParseableCfg
    {
        private string _name;
        public T value;

        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Cfg(string name, T defaultValue)
        {
            this.name = name;
            value = defaultValue;
        }

        public abstract void ParseFrom(string str);
    }
}
