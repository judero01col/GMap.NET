using System.Collections.Generic;

namespace MSR.CVE.BackMaker.MCDebug
{
    internal class MakeObjectID
    {
        private Dictionary<WeakHashableObject, int> objectIDDict = new Dictionary<WeakHashableObject, int>();
        private int nextID;
        public static MakeObjectID Maker = new MakeObjectID();

        public int make(object o)
        {
            WeakHashableObject key = new WeakHashableObject(o);
            if (objectIDDict.ContainsKey(key))
            {
                return objectIDDict[key];
            }

            int num = nextID;
            nextID++;
            objectIDDict[key] = num;
            return num;
        }
    }
}
