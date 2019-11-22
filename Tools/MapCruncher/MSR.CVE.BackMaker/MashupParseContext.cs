using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class MashupParseContext : IDisposable
    {
        public XmlTextReader reader;
        public MashupXMLSchemaVersion version;
        public MashupFileWarningList warnings = new MashupFileWarningList();
        public IgnoredTags ignoredTags = new IgnoredTags();
        public string mostRecentXTRTagIs;
        private string mostRecentTag;
        private Dictionary<string, object> identityMap = new Dictionary<string, object>();

        public MashupParseContext(XmlTextReader reader)
        {
            this.reader = reader;
        }

        public XMLTagReader NewTagReader(string mashupFileTag)
        {
            mostRecentTag = mashupFileTag;
            return new XMLTagReader(reader, mashupFileTag, ignoredTags, this);
        }

        public void Dispose()
        {
            if (ignoredTags.Count > 0)
            {
                warnings.Add(new MashupFileWarning("Ignored tags: " + ignoredTags.ToString()));
            }

            reader.Close();
        }

        public string GetRequiredAttribute(string AttrName)
        {
            string attribute = reader.GetAttribute(AttrName);
            if (attribute == null)
            {
                throw new InvalidMashupFile(this,
                    string.Format("Missing attribute {0} in {1} tag.", AttrName, reader.Name));
            }

            return attribute;
        }

        internal object FetchObjectByIdentity(string id)
        {
            if (identityMap.ContainsKey(id))
            {
                return identityMap[id];
            }

            return null;
        }

        internal void ExpectIdentity(object target)
        {
            string attribute = reader.GetAttribute("id");
            if (attribute == null)
            {
                return;
            }

            if (identityMap.ContainsKey(attribute))
            {
                throw new InvalidMashupFile(this, string.Format("Id attribute {0} reused", attribute));
            }

            identityMap.Add(attribute, target);
        }

        internal void AssertUnique(object obj)
        {
            D.Assert(obj == null || !obj.GetType().IsValueType);
            if (obj != null)
            {
                ThrowUnique();
            }
        }

        internal void ThrowUnique()
        {
            throw new InvalidMashupFile(this,
                string.Format("Expected only one {0} tag here.", mostRecentXTRTagIs));
        }

        internal void AssertPresent(object obj, string tagName)
        {
            if (obj == null)
            {
                throw new InvalidMashupFile(this, "Missing " + tagName);
            }
        }

        internal bool GetRequiredAttributeBoolean(string attrName)
        {
            bool result;
            try
            {
                result = Convert.ToBoolean(GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new InvalidMashupFile(this, ex.Message);
            }

            return result;
        }

        internal void GetAttributeBoolean(string attrName, ref bool target)
        {
            try
            {
                string attribute = reader.GetAttribute(attrName);
                if (attribute != null)
                {
                    target = Convert.ToBoolean(attribute, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                warnings.Add(new MashupFileWarning(string.Format("Ignored invalid boolean value at {0}",
                    FilePosition())));
            }
        }

        public string FilePosition()
        {
            return FilePosition(reader);
        }

        public static string FilePosition(XmlTextReader reader)
        {
            return string.Format("line {0}, character {1}", reader.LineNumber, reader.LinePosition);
        }

        internal int GetRequiredAttributeInt(string attrName)
        {
            int result;
            try
            {
                result = Convert.ToInt32(GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new InvalidMashupFile(this, ex.Message);
            }

            return result;
        }

        internal long GetRequiredAttributeLong(string attrName)
        {
            long result;
            try
            {
                result = Convert.ToInt64(GetRequiredAttribute(attrName), CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new InvalidMashupFile(this, ex.Message);
            }

            return result;
        }

        internal void GetAttributeInt(string attrName, ref int target)
        {
            try
            {
                string attribute = reader.GetAttribute(attrName);
                if (attribute != null)
                {
                    target = Convert.ToInt32(attribute, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                warnings.Add(new MashupFileWarning(string.Format("Ignored invalid integer value at {0}",
                    FilePosition())));
            }
        }
    }
}
