using System;

namespace MSR.CVE.BackMaker
{
    internal class CfgBool : Cfg<bool>
    {
        public CfgBool(string name, bool defaultValue) : base(name, defaultValue)
        {
        }

        public override void ParseFrom(string str)
        {
            value = Convert.ToBoolean(str);
        }
    }
}
