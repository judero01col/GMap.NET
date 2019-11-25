using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.Text.RegularExpressions;

namespace BSE.Windows.Forms
{
	static class DisplayInformation
	{
		#region FieldsPrivate

		[ThreadStatic]
        private static bool _bIsThemed;
        private const string StrRegExpression = @".*\.msstyles$";

		#endregion

		#region Properties

        internal static bool IsThemed
        {
            get { return _bIsThemed; }
        }
		#endregion

		#region MethodsPrivate

		static DisplayInformation()
		{
			SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
			SetScheme();
		}

		private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
		{
			SetScheme();
		}

		private static void SetScheme()
		{
			if (VisualStyleRenderer.IsSupported)
			{
				if (!VisualStyleInformation.IsEnabledByUser)
				{
					return;
				}

				var stringBuilder = new StringBuilder(0x200);
				int iResult = NativeMethods.GetCurrentThemeName(stringBuilder, stringBuilder.Capacity, null, 0, null, 0);
                if (iResult == 0)
                {
                    var regex = new Regex(StrRegExpression);
                    _bIsThemed = regex.IsMatch(Path.GetFileName(stringBuilder.ToString()));
                }
			}
		}
		#endregion

		static class NativeMethods
		{
			[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
			public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);
		}
	}
}
