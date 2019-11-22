using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using MSR.CVE.BackMaker.MCDebug;

namespace MSR.CVE.BackMaker
{
    public class VEUrlFormat
    {
        private const string webGenerationNumberUpdateURL = "http://tiles.virtualearth.net/tiles/gen";
        private const string defaultFormatString = "http://{0}{1}.ortho.tiles.virtualearth.net/tiles/{0}{2}.{3}?g={4}";
        private const int defaultGenerationNumber = 66;
        private const string VEUrlFormatTag = "VEUrlFormat";
        private const string FormatStringAttr = "FormatString";
        private const string GenerationNumberAttr = "GenerationNumber";
        public static VEUrlFormat theFormat = new VEUrlFormat();

        private EventWaitHandle formatReadyEvent =
            new CountedEventWaitHandle(false, EventResetMode.ManualReset, "VEUrlFormat.formatReadyEvent");

        private string formatString;
        private int generationNumber;

        private VEUrlFormat()
        {
            if (BuildConfig.theConfig.veFormatUpdateURL != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    TryOneFetchFormatString();
                    if (formatString != null)
                    {
                        break;
                    }
                }

                if (formatString == null)
                {
                    D.Sayf(0,
                        "Dynamic VEUrlFormat unavailable at {0}",
                        new object[] {BuildConfig.theConfig.veFormatUpdateURL});
                }
            }

            if (formatString == null)
            {
                formatString = "http://{0}{1}.ortho.tiles.virtualearth.net/tiles/{0}{2}.{3}?g={4}";
                generationNumber = 66;
            }

            UpdateGenerationNumber();
            formatReadyEvent.Set();
        }

        private void TryOneFetchFormatString()
        {
            try
            {
                HttpWebRequest httpWebRequest =
                    (HttpWebRequest)WebRequest.Create(BuildConfig.theConfig.veFormatUpdateURL);
                httpWebRequest.Timeout = 5000;
                HttpWebResponse httpWebResponse;
                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                }
                catch (WebException)
                {
                    return;
                }

                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = httpWebResponse.GetResponseStream();
                    XmlTextReader reader = new XmlTextReader(responseStream);
                    MashupParseContext mashupParseContext = new MashupParseContext(reader);
                    using (mashupParseContext)
                    {
                        while (mashupParseContext.reader.Read())
                        {
                            if (mashupParseContext.reader.NodeType == XmlNodeType.Element &&
                                mashupParseContext.reader.Name == "VEUrlFormat")
                            {
                                XMLTagReader xMLTagReader = mashupParseContext.NewTagReader("VEUrlFormat");
                                formatString = mashupParseContext.GetRequiredAttribute("FormatString");
                                generationNumber = mashupParseContext.GetRequiredAttributeInt("GenerationNumber");
                                xMLTagReader.SkipAllSubTags();
                                break;
                            }
                        }

                        mashupParseContext.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                D.Sayf(0, "VEUrlFormat fetch failed with unexpected {0}", new object[] {ex});
            }
        }

        private void UpdateGenerationNumber()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    generationNumber = TryOneFetchGenerationNumber();
                    break;
                }
                catch (Exception ex)
                {
                    D.Sayf(0, "VEUrlFormat fetch failed with unexpected {0}", new object[] {ex});
                }
            }
        }

        private int TryOneFetchGenerationNumber()
        {
            HttpWebRequest httpWebRequest =
                (HttpWebRequest)WebRequest.Create("http://tiles.virtualearth.net/tiles/gen");
            httpWebRequest.Timeout = 5000;
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            if (httpWebResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Status code = " + httpWebResponse.StatusCode.ToString());
            }

            Stream responseStream = httpWebResponse.GetResponseStream();
            byte[] array = new byte[1000];
            responseStream.Read(array, 0, array.Length);
            ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
            string @string = aSCIIEncoding.GetString(array);
            return Convert.ToInt32(@string, CultureInfo.InvariantCulture);
        }

        public string GetFormatString()
        {
            formatReadyEvent.WaitOne();
            return formatString;
        }

        public int GetGenerationNumber()
        {
            return generationNumber;
        }
    }
}
