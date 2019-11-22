using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace BSE.Windows.Forms
{
    /// <summary>
    /// Class for the custom colors at a Panel control. 
    /// </summary>
    /// <copyright>Copyright © 2008 Uwe Eichkorn
    /// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    /// PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
    /// REMAINS UNCHANGED.
    /// </copyright>
    public class CustomPanelColors : CustomColors
    {
        #region FieldsPrivate
        private Color _captionSelectedGradientBegin = System.Windows.Forms.ProfessionalColors.ButtonSelectedGradientBegin;
        private Color _captionSelectedGradientEnd = System.Windows.Forms.ProfessionalColors.ButtonSelectedGradientEnd;
        private Color _collapsedCaptionText = SystemColors.ControlText;
        private Color _contentGradientBegin = System.Windows.Forms.ProfessionalColors.ToolStripContentPanelGradientBegin;
        private Color _contentGradientEnd = System.Windows.Forms.ProfessionalColors.ToolStripContentPanelGradientEnd;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the starting color of the gradient used when the hover icon in the captionbar on the Panel is selected.
        /// </summary>
        [Description("The starting color of the hover icon in the captionbar on the Panel.")]
        public virtual Color CaptionSelectedGradientBegin
        {
            get { return _captionSelectedGradientBegin; }
            set
            {
                if (value.Equals(_captionSelectedGradientBegin) == false)
                {
                    _captionSelectedGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient used when the hover icon in the captionbar on the Panel is selected.
        /// </summary>
        [Description("The end color of the hover icon in the captionbar on the Panel.")]
        public virtual Color CaptionSelectedGradientEnd
        {
            get { return _captionSelectedGradientEnd; }
            set
            {
                if (value.Equals(_captionSelectedGradientEnd) == false)
                {
                    _captionSelectedGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the text color of a Panel when it's collapsed.
        /// </summary>
        [Description("The text color of a Panel when it's collapsed.")]
        public virtual Color CollapsedCaptionText
        {
            get { return _collapsedCaptionText; }
            set
            {
                if (value.Equals(_collapsedCaptionText) == false)
                {
                    _collapsedCaptionText = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient used in the Panel.
        /// </summary>
        [Description("The starting color of the gradient used in the Panel.")]
        public virtual Color ContentGradientBegin
        {
            get { return _contentGradientBegin; }
            set
            {
                if (value.Equals(_contentGradientBegin) == false)
                {
                    _contentGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient used in the Panel.
        /// </summary>
        [Description("The end color of the gradient used in the Panel.")]
        public virtual Color ContentGradientEnd
        {
            get { return _contentGradientEnd; }
            set
            {
                if (value.Equals(_contentGradientEnd) == false)
                {
                    _contentGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion
    }
}
