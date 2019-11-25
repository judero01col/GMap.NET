using System.Drawing.Imaging;

namespace MSR.CVE.BackMaker
{
    public class ImageTypeMapping
    {
        public string extension
        {
            get;
        }

        public string mimeType
        {
            get;
        }

        public ImageFormat imageFormat
        {
            get;
        }

        public ImageTypeMapping(string extension, string mimeType, ImageFormat imageFormat)
        {
            this.extension = extension;
            this.mimeType = mimeType;
            this.imageFormat = imageFormat;
        }

        public bool ImageFormatEquals(ImageFormat otherFormat)
        {
            return imageFormat != null && otherFormat != null && imageFormat.Guid == otherFormat.Guid;
        }
    }
}
