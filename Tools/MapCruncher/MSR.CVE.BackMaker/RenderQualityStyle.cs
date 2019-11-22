using System.Drawing.Drawing2D;

namespace MSR.CVE.BackMaker
{
    public class RenderQualityStyle
    {
        private string _styleName;
        private InterpolationMode _invokeImageInterpolationMode;
        private InterpolationMode _warpInterpolationMode;
        private double _hackyWarperAntialiasFactor;

        public static RenderQualityStyle highQuality = new RenderQualityStyle("highQuality",
            InterpolationMode.HighQualityBicubic,
            InterpolationMode.Bilinear,
            1.0);

        public static RenderQualityStyle exactColors = new RenderQualityStyle("exactColors",
            InterpolationMode.NearestNeighbor,
            InterpolationMode.NearestNeighbor,
            1.0);

        public static RenderQualityStyle foxit = new RenderQualityStyle("FoxIT",
            InterpolationMode.HighQualityBicubic,
            InterpolationMode.Bilinear,
            2.0);

        public static RenderQualityStyle theStyle = foxit;

        public InterpolationMode invokeImageInterpolationMode
        {
            get
            {
                return _invokeImageInterpolationMode;
            }
        }

        public InterpolationMode warpInterpolationMode
        {
            get
            {
                return _warpInterpolationMode;
            }
        }

        public double hackyWarperAntialiasFactor
        {
            get
            {
                return _hackyWarperAntialiasFactor;
            }
        }

        private RenderQualityStyle(string _styleName, InterpolationMode invokeImageInterpolationMode,
            InterpolationMode warpInterpolationMode, double hackyWarperAntialiasFactor)
        {
            this._styleName = _styleName;
            _invokeImageInterpolationMode = invokeImageInterpolationMode;
            _warpInterpolationMode = warpInterpolationMode;
            _hackyWarperAntialiasFactor = hackyWarperAntialiasFactor;
        }
    }
}
