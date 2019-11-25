using System.Drawing.Drawing2D;

namespace MSR.CVE.BackMaker
{
    public class RenderQualityStyle
    {
        private string _styleName;

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
            get;
        }

        public InterpolationMode warpInterpolationMode
        {
            get;
        }

        public double hackyWarperAntialiasFactor
        {
            get;
        }

        private RenderQualityStyle(string _styleName, InterpolationMode invokeImageInterpolationMode,
            InterpolationMode warpInterpolationMode, double hackyWarperAntialiasFactor)
        {
            this._styleName = _styleName;
            this.invokeImageInterpolationMode = invokeImageInterpolationMode;
            this.warpInterpolationMode = warpInterpolationMode;
            this.hackyWarperAntialiasFactor = hackyWarperAntialiasFactor;
        }
    }
}
