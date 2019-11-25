namespace MSR.CVE.BackMaker
{
    internal abstract class Cfg<T> : ParseableCfg
    {
        public T value;

        public string name
        {
            get;
            set;
        }

        public Cfg(string name, T defaultValue)
        {
            this.name = name;
            value = defaultValue;
        }

        public abstract void ParseFrom(string str);
    }
}
