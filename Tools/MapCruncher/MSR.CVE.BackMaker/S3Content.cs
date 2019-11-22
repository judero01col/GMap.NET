namespace MSR.CVE.BackMaker
{
    public class S3Content
    {
        private byte[] bytes;

        public byte[] Bytes
        {
            get
            {
                return bytes;
            }
        }

        public S3Content(byte[] bytes)
        {
            this.bytes = bytes;
        }
    }
}
