using System.Windows.Forms;

namespace MSR.CVE.BackMaker
{
    public partial class AboutForm : Form
    {
        public AboutForm(string versionNumber)
        {
            InitializeComponent();

            string text = string.Concat(new[]
            {
                "\r\n<html>\r\n<center>\r\n<br /><br /><i>", BuildConfig.theConfig.editionName,
                "</i>\r\n<br>Version ", versionNumber, "\r\n<p>\r\n<a target=\"new\" href=\"",
                BuildConfig.theConfig.mapCruncherHomeSite,
                "\">MapCruncher</a> was\r\ncreated at <a target=\"new\" href=\"http://research.microsoft.com/\">Microsoft Research</a>,<br>by the\r\nComposable Virtual Earth team"
            });
            if (BuildConfig.theConfig.buildConfiguration != "VE")
            {
                text += ":<br>Jeremy Elson, Jon Howell, John Douceur, and Danyel Fisher";
            }

            text +=
                ".\r\n</center>\r\n<p>\r\n<center>&copy; 2007 Microsoft Corporation.  All rights reserved.</center>\r\n<p>\r\n<center>\r\nFoxit PDF Reader SDK\r\n<br>Copyright &copy; 2002-2006, Foxit Software Company\r\n<br><a target=\"new\" href=\"http://www.foxitsoftware.com/\">www.foxitsoftware.com</a>, All rights reserved.\r\n</center>";
            if (BuildConfig.theConfig.buildConfiguration != "VE")
            {
                text +=
                    "\r\n<p>\r\n<center>\r\nSpecial thanks to:\r\nJim Blinn,\r\nRich Draves,\r\nSteve Lombardi,\r\nKaren Luecking,\r\nJoe Schwartz,\r\nMarvin Theimer,\r\nChandu Thota,\r\nand everyone who has tested MapCruncher!\r\n</center>";
            }

            //text += "\r\n<p>\r\n<center>Feedback is welcome.  Write to<br>\r\n<tt>cruncher</tt> at <tt>microsoft</tt> dot <tt>com</tt></center>\r\n\r\n<p>\r\n<b>Warning:</b>\r\nThis computer program is protected by copyright law and international treaties.\r\nUnauthorized reproduction or distribution of this program, or any portion of it,\r\nmay result in severe civil and criminal penalties, and will be prosecuted to the\r\nmaximum extent possible under the law.\r\n</html>\r\n";
            aboutContentsBrowser.DocumentText = text;
        }
    }
}
