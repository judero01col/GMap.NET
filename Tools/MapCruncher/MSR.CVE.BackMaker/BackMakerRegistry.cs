using Microsoft.Win32;

namespace MSR.CVE.BackMaker
{
    public class BackMakerRegistry
    {
        private const string BACKMAKER_KEY_ROOT = "Software\\Microsoft Research\\CVE";
        private const string BACKMAKER_KEY_PREFIX = "backmaker_";
        private RegistryKey BackMaker_key;
        private string customPrefix = "";

        private void initialize(string customPrefix)
        {
            this.customPrefix = customPrefix;
            BackMaker_key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft Research\\CVE", true);
            if (BackMaker_key == null)
            {
                BackMaker_key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft Research\\CVE");
            }
        }

        public BackMakerRegistry(string customPrefix)
        {
            initialize(customPrefix);
        }

        public BackMakerRegistry()
        {
            initialize("");
        }

        public void Dispose()
        {
            BackMaker_key.Close();
        }

        private string AddPrefixes(string rawKeyName)
        {
            return "backmaker_" + customPrefix + rawKeyName;
        }

        public string GetValue(string keyName)
        {
            if (BackMaker_key != null)
            {
                return (string)BackMaker_key.GetValue(AddPrefixes(keyName));
            }

            return null;
        }

        public void SetValue(string keyName, string value)
        {
            if (BackMaker_key != null)
            {
                if (value == null)
                {
                    BackMaker_key.DeleteValue(AddPrefixes(keyName), false);
                    return;
                }

                BackMaker_key.SetValue(AddPrefixes(keyName), value);
            }
        }
    }
}
