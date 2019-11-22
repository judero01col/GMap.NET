using System;
using System.Text;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    public class XMLTagReader
    {
        private string tag;
        private string lastStart;
        private XmlTextReader reader;
        private bool empty;
        private bool done;
        private IgnoredTags ignoredTags;
        private StringBuilder content = new StringBuilder();
        private MashupParseContext context;

        public XMLTagReader(XmlTextReader reader, string tag, IgnoredTags ignoredTags, MashupParseContext context)
        {
            this.context = context;
            this.ignoredTags = ignoredTags;
            if (reader.NodeType != XmlNodeType.Element || reader.Name != tag)
            {
                throw new Exception(string.Format(
                    "You found a bug.  Reader for {0} called improperly at start of {1} block.",
                    tag,
                    reader.Name));
            }

            this.reader = reader;
            this.tag = tag;
            if (reader.IsEmptyElement)
            {
                empty = true;
            }
        }

        public bool TagIs(string s)
        {
            if (lastStart != null && lastStart == s)
            {
                context.mostRecentXTRTagIs = lastStart;
                lastStart = null;
                return true;
            }

            return false;
        }

        public void SkipAllSubTags()
        {
            while (FindNextStartTag())
            {
            }
        }

        public string GetContent()
        {
            return content.ToString();
        }

        public bool FindNextStartTag()
        {
            if (done)
            {
                return false;
            }

            if (empty)
            {
                done = true;
                return false;
            }

            if (reader.NodeType == XmlNodeType.Element && lastStart != null &&
                lastStart == reader.Name)
            {
                ignoredTags.Add(lastStart);
                XMLTagReader xMLTagReader =
                    new XMLTagReader(reader, lastStart, ignoredTags, context);
                xMLTagReader.SkipAllSubTags();
            }

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    lastStart = reader.Name;
                    context.mostRecentXTRTagIs = null;
                    return true;
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == tag)
                    {
                        done = true;
                        return false;
                    }

                    throw new InvalidMashupFile(reader,
                        string.Format("Bad Mashup file!  XML tag {0} improperly closed with </{1}> (line {2})",
                            tag,
                            reader.Name,
                            reader.LineNumber));
                }
                else
                {
                    string value = reader.Value;
                    content.Append(value);
                }
            }

            throw new InvalidMashupFile(reader, "Unexpected end of file");
        }
    }
}
