using System;
using System.Collections.Generic;

namespace MSR.CVE.BackMaker
{
    public class LayerList
    {
        private const string LayerListTag = "LayerList";
        private List<Layer> layers = new List<Layer>();
        private DirtyEvent dirtyEvent;

        public int Count
        {
            get
            {
                return layers.Count;
            }
        }

        public Layer First
        {
            get
            {
                return layers[0];
            }
        }

        public LayerList(DirtyEvent parentDirty)
        {
            dirtyEvent = parentDirty;
        }

        public LayerList(MashupParseContext context, SourceMap.GetFilenameContext filenameContextDelegate,
            DirtyEvent parentDirty, DirtyEvent parentReadyToLockEvent)
        {
            dirtyEvent = parentDirty;
            XMLTagReader xMLTagReader = context.NewTagReader("LayerList");
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(Layer.GetXMLTag()))
                {
                    Add(new Layer(context, filenameContextDelegate, dirtyEvent, parentReadyToLockEvent));
                }
            }
        }

        public void WriteXML(MashupWriteContext wc)
        {
            wc.writer.WriteStartElement("LayerList");
            foreach (Layer current in this)
            {
                current.WriteXML(wc);
            }

            wc.writer.WriteEndElement();
        }

        internal static string GetXMLTag()
        {
            return "LayerList";
        }

        internal void AddNewLayer()
        {
            Add(new Layer(this, dirtyEvent));
        }

        public List<Layer>.Enumerator GetEnumerator()
        {
            return layers.GetEnumerator();
        }

        public void Add(Layer layer)
        {
            layers.Add(layer);
            dirtyEvent.SetDirty();
        }

        public void AddAfter(Layer newLayer, Layer refLayer)
        {
            int num = layers.FindIndex((Layer l) => l == refLayer);
            if (num < 0)
            {
                throw new IndexOutOfRangeException();
            }

            layers.Insert(num + 1, newLayer);
        }

        public void Remove(Layer layer)
        {
            layers.Remove(layer);
            dirtyEvent.SetDirty();
        }

        internal bool HasLayerNamed(string proposedLayerName)
        {
            return layers.Find((Layer layer) => layer.displayName == proposedLayerName) != null;
        }

        internal void RemoveSourceMap(SourceMap sourceMap)
        {
            Layer layer2 = layers.Find((Layer layer) => layer.Contains(sourceMap));
            layer2.Remove(sourceMap);
        }

        internal void AutoSelectMaxZooms(MapTileSourceFactory mapTileSourceFactory)
        {
            foreach (Layer current in layers)
            {
                current.AutoSelectMaxZooms(mapTileSourceFactory);
            }
        }

        internal bool SomeSourceMapIsReadyToLock()
        {
            foreach (Layer current in layers)
            {
                if (current.SomeSourceMapIsReadyToLock())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
