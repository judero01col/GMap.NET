namespace MSR.CVE.BackMaker
{
    public class S3Content
    {
        public byte[] Bytes
        {
            get;
        }

        public S3Content(byte[] bytes)
        {
            this.Bytes = bytes;
        }
    }
}
