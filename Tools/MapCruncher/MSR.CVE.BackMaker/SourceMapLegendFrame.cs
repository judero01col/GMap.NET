using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml;

namespace MSR.CVE.BackMaker
{
    internal class SourceMapLegendFrame
    {
        public delegate ThumbnailRecord ThumbnailDelegate();

        private const string filenameAttr = "Filename";
        private const string urlAttr = "URL";
        private const string widthAttr = "Width";
        private const string heightAttr = "Height";
        private bool useLoadedSize;
        private Size loadedSize;
        private string filename;
        private string displayName;
        private SourceMapInfo sourceMapInfo;
        private List<LegendRecord> legendRecords;
        private ThumbnailDelegate thumbnailDelegate;

        public Size size
        {
            get
            {
                if (useLoadedSize)
                {
                    return loadedSize;
                }

                Size result = new Size(250, 50);
                foreach (LegendRecord current in legendRecords)
                {
                    result.Width = Math.Max(result.Width, current.imageDimensions.Width);
                    result.Height += current.imageDimensions.Height;
                }

                ThumbnailRecord thumbnailRecord = thumbnailDelegate();
                if (thumbnailRecord != null)
                {
                    result.Width = Math.Max(result.Width, thumbnailRecord.size.Width);
                    result.Height += thumbnailRecord.size.Height;
                }

                return result;
            }
        }

        public SourceMapLegendFrame(Layer layer, SourceMap sourceMap, List<LegendRecord> legendRecords,
            ThumbnailDelegate thumbnailDelegate)
        {
            filename = RenderState.ForceValidFilename(string.Format("SourceMap_{0}_{1}.html",
                layer.displayName,
                sourceMap.displayName));
            displayName = sourceMap.displayName;
            sourceMapInfo = sourceMap.sourceMapInfo;
            this.legendRecords = legendRecords;
            this.thumbnailDelegate = thumbnailDelegate;
        }

        internal static string GetXMLTag()
        {
            return "SourceMapLegendFrame";
        }

        public SourceMapLegendFrame(MashupParseContext context)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            filename = context.GetRequiredAttribute("Filename");
            loadedSize.Width = context.GetRequiredAttributeInt("Width");
            loadedSize.Height = context.GetRequiredAttributeInt("Height");
            useLoadedSize = true;
            xMLTagReader.SkipAllSubTags();
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(GetXMLTag());
            writer.WriteAttributeString("Filename", filename);
            writer.WriteAttributeString("URL", GetURL());
            writer.WriteAttributeString("Width", size.Width.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Height", size.Height.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        private string GetURL()
        {
            return string.Format("{0}/{1}", "legends", filename);
        }

        public void WriteSourceMapLegendFrame(RenderOutputMethod renderOutput)
        {
            Stream stream = renderOutput.MakeChildMethod("legends").CreateFile(filename, "text/html");
            StreamWriter streamWriter = new StreamWriter(stream);
            streamWriter.WriteLine("<html>");
            streamWriter.WriteLine(string.Format("<head><title>{0}</title></head>", displayName));
            streamWriter.WriteLine("<body>");
            streamWriter.WriteLine(string.Format("<h3>{0}</h3>", displayName));
            ThumbnailRecord thumbnailRecord = thumbnailDelegate();
            if (thumbnailRecord != null)
            {
                streamWriter.WriteLine(thumbnailRecord.WriteImgTag("../"));
            }

            if (sourceMapInfo.mapFileURL != "")
            {
                streamWriter.WriteLine(string.Format("<br>Map URL: <a href=\"{0}\">{0}</a>",
                    sourceMapInfo.mapFileURL));
            }

            if (sourceMapInfo.mapHomePage != "")
            {
                streamWriter.WriteLine(string.Format("<br>Map Home Page: <a href=\"{0}\">{0}</a>",
                    sourceMapInfo.mapHomePage));
            }

            if (sourceMapInfo.mapDescription != "")
            {
                streamWriter.WriteLine(string.Format("<p>{0}</p>", sourceMapInfo.mapDescription));
            }

            foreach (LegendRecord current in legendRecords)
            {
                streamWriter.WriteLine(string.Format("<br><img src=\"{0}\" width=\"{1}\" height=\"{2}\">",
                    current.urlSuffix,
                    current.imageDimensions.Width,
                    current.imageDimensions.Height));
            }

            streamWriter.WriteLine("</body>");
            streamWriter.WriteLine("</html>");
            streamWriter.Close();
        }
    }
}
