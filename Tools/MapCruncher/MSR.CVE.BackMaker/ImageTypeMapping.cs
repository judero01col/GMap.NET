using System.Drawing.Imaging;

namespace MSR.CVE.BackMaker
{
    public class ImageTypeMapping
    {
        private string _extension;
        private string _mimeType;
        private ImageFormat _imageFormat;

        public string extension
        {
            get
            {
                return _extension;
            }
        }

        public string mimeType
        {
            get
            {
                return _mimeType;
            }
        }

        public ImageFormat imageFormat
        {
            get
            {
                return _imageFormat;
            }
        }

        public ImageTypeMapping(string extension, string mimeType, ImageFormat imageFormat)
        {
            _extension = extension;
            _mimeType = mimeType;
            _imageFormat = imageFormat;
        }

        public bool ImageFormatEquals(ImageFormat otherFormat)
        {
            return _imageFormat != null && otherFormat != null && _imageFormat.Guid == otherFormat.Guid;
        }
    }
}
