using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public class LegendList
    {
        private SourceMap _sourceMap;
        private List<Legend> list = new List<Legend>();
        public DirtyEvent dirtyEvent;
        private DirtyEvent parentBoundsChangedEvent;

        //[CompilerGenerated]
        //private static Converter<Legend, string> <>9__CachedAnonymousMethodDelegate1;

        public LegendList(SourceMap sourceMap, DirtyEvent parentEvent, DirtyEvent parentBoundsChangedEvent)
        {
            _sourceMap = sourceMap;
            dirtyEvent = new DirtyEvent(parentEvent);
            this.parentBoundsChangedEvent = parentBoundsChangedEvent;
        }

        public LegendList(SourceMap sourceMap, MashupParseContext context, DirtyEvent parentEvent)
        {
            _sourceMap = sourceMap;
            dirtyEvent = new DirtyEvent(parentEvent);
            parentBoundsChangedEvent = parentEvent;
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(Legend.GetXMLTag()))
                {
                    list.Add(new Legend(_sourceMap, context, dirtyEvent, parentBoundsChangedEvent));
                }
            }
        }

        public static string GetXMLTag()
        {
            return "LegendList";
        }

        public void WriteXML(MashupWriteContext context)
        {
            context.writer.WriteStartElement(GetXMLTag());
            foreach (Legend current in this)
            {
                current.WriteXML(context);
            }

            context.writer.WriteEndElement();
        }

        internal Legend AddNewLegend()
        {
            Legend legend = new Legend(_sourceMap, dirtyEvent, parentBoundsChangedEvent);
            string displayName = legend.displayName;
            int num = 1;
            List<string> list = this.list.ConvertAll((Legend l) => l.displayName);
            while (list.Contains(legend.displayName))
            {
                num++;
                legend.displayName = string.Format("{0} {1}", displayName, num);
            }

            this.list.Add(legend);
            dirtyEvent.SetDirty();
            return legend;
        }

        public List<Legend>.Enumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void RemoveLegend(Legend legend)
        {
            list.Remove(legend);
            dirtyEvent.SetDirty();
        }
    }
}
