using System.Drawing.Imaging;

namespace MSR.CVE.BackMaker
{
    public class OutputTileType
    {
        private string _extn;
        private ImageFormat _imageFormat;
        public static OutputTileType PNG = new OutputTileType("png", ImageFormat.Png);
        public static OutputTileType JPG = new OutputTileType("jpg", ImageFormat.Jpeg);
        public static OutputTileType IPIC = new OutputTileType("ipic", null);

        public string extn
        {
            get
            {
                return _extn;
            }
        }

        public ImageFormat imageFormat
        {
            get
            {
                D.Assert(_imageFormat != null);
                return _imageFormat;
            }
        }

        private OutputTileType(string extn, ImageFormat imageFormat)
        {
            _extn = extn;
            _imageFormat = imageFormat;
        }

        public static OutputTileType Parse(string extn)
        {
            if (extn == "png")
            {
                return PNG;
            }

            if (extn == "jpg")
            {
                return JPG;
            }

            if (extn == "ipic")
            {
                return IPIC;
            }

            throw new UnknownImageTypeException(string.Format("Unrecognized output type {0}", extn));
        }
    }
}
