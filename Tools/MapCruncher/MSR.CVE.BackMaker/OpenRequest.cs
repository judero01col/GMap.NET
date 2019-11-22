using System;
using System.Runtime.Serialization;

namespace MSR.CVE.BackMaker
{
    [Serializable]
    public class OpenRequest : ISerializable
    {
        public string filename;
        public int pageNumber;

        public OpenRequest(string filename, int pageNumber)
        {
            this.filename = filename;
            this.pageNumber = pageNumber;
        }

        public OpenRequest(SerializationInfo info, StreamingContext context)
        {
            filename = (string)info.GetValue("Filename", typeof(string));
            pageNumber = (int)info.GetValue("PageNumber", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Filename", filename);
            info.AddValue("PageNumber", pageNumber);
        }
    }
}
