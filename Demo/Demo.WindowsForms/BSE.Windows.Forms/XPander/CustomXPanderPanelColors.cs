using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace BSE.Windows.Forms
{
    /// <summary>
    /// Class for the custom colors at a XPanderPanel control. 
    /// </summary>
    /// <copyright>Copyright © 2008 Uwe Eichkorn
    /// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    /// PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
    /// REMAINS UNCHANGED.
    /// </copyright>
    public class CustomXPanderPanelColors : CustomColors
    {
        #region FieldsPrivate
        private Color _backColor = SystemColors.Control;
        private Color _flatCaptionGradientBegin = System.Windows.Forms.ProfessionalColors.ToolStripGradientMiddle;
        private Color _flatCaptionGradientEnd = System.Windows.Forms.ProfessionalColors.ToolStripGradientBegin;
        private Color _captionPressedGradientBegin = System.Windows.Forms.ProfessionalColors.ButtonPressedGradientBegin;
        private Color _captionPressedGradientEnd = System.Windows.Forms.ProfessionalColors.ButtonPressedGradientEnd;
        private Color _captionPressedGradientMiddle = System.Windows.Forms.ProfessionalColors.ButtonPressedGradientMiddle;
        private Color _captionCheckedGradientBegin = System.Windows.Forms.ProfessionalColors.ButtonCheckedGradientBegin;
        private Color _captionCheckedGradientEnd = System.Windows.Forms.ProfessionalColors.ButtonCheckedGradientEnd;
        private Color _captionCheckedGradientMiddle = System.Windows.Forms.ProfessionalColors.ButtonCheckedGradientMiddle;
        private Color _captionSelectedGradientBegin = System.Windows.Forms.ProfessionalColors.ButtonSelectedGradientBegin;
        private Color _captionSelectedGradientEnd = System.Windows.Forms.ProfessionalColors.ButtonSelectedGradientEnd;
        private Color _captionSelectedGradientMiddle = System.Windows.Forms.ProfessionalColors.ButtonSelectedGradientMiddle;
        private Color _captionSelectedText = SystemColors.ControlText;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the backcolor of a XPanderPanel.
        /// </summary>
        [Description("The backcolor of a XPanderPanel.")]
        public virtual Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (value.Equals(_backColor) == false)
                {
                    _backColor = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient on a flat XPanderPanel captionbar.
        /// </summary>
        [Description("The starting color of the gradient on a flat XPanderPanel captionbar.")]
        public virtual Color FlatCaptionGradientBegin
        {
            get { return _flatCaptionGradientBegin; }
            set
            {
                if (value.Equals(_flatCaptionGradientBegin) == false)
                {
                    _flatCaptionGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient on a flat XPanderPanel captionbar.
        /// </summary>
        [Description("The end color of the gradient on a flat XPanderPanel captionbar.")]
        public virtual Color FlatCaptionGradientEnd
        {
            get { return _flatCaptionGradientEnd; }
            set
            {
                if (value.Equals(_flatCaptionGradientEnd) == false)
                {
                    _flatCaptionGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient used when the XPanderPanel is pressed down.
        /// </summary>
        [Description("The starting color of the gradient used when the XPanderPanel is pressed down.")]
        public virtual Color CaptionPressedGradientBegin
        {
            get { return _captionPressedGradientBegin; }
            set
            {
                if (value.Equals(_captionPressedGradientBegin) == false)
                {
                    _captionPressedGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient used when the XPanderPanel is pressed down.
        /// </summary>
        [Description("The end color of the gradient used when the XPanderPanel is pressed down.")]
        public virtual Color CaptionPressedGradientEnd
        {
            get { return _captionPressedGradientEnd; }
            set
            {
                if (value.Equals(_captionPressedGradientEnd) == false)
                {
                    _captionPressedGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the middle color of the gradient used when the XPanderPanel is pressed down.
        /// </summary>
        [Description("The middle color of the gradient used when the XPanderPanel is pressed down.")]
        public virtual Color CaptionPressedGradientMiddle
        {
            get { return _captionPressedGradientMiddle; }
            set
            {
                if (value.Equals(_captionPressedGradientMiddle) == false)
                {
                    _captionPressedGradientMiddle = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient used when the XPanderPanel is checked.
        /// </summary>
        [Description("The starting color of the gradient used when the XPanderPanel is checked.")]
        public virtual Color CaptionCheckedGradientBegin
        {
            get { return _captionCheckedGradientBegin; }
            set
            {
                if (value.Equals(_captionCheckedGradientBegin) == false)
                {
                    _captionCheckedGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient used when the XPanderPanel is checked.
        /// </summary>
        [Description("The end color of the gradient used when the XPanderPanel is checked.")]
        public virtual Color CaptionCheckedGradientEnd
        {
            get { return _captionCheckedGradientEnd; }
            set
            {
                if (value.Equals(_captionCheckedGradientEnd) == false)
                {
                    _captionCheckedGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the middle color of the gradient used when the XPanderPanel is checked.
        /// </summary>
        [Description("The middle color of the gradient used when the XPanderPanel is checked.")]
        public virtual Color CaptionCheckedGradientMiddle
        {
            get { return _captionCheckedGradientMiddle; }
            set
            {
                if (value.Equals(_captionCheckedGradientMiddle) == false)
                {
                    _captionCheckedGradientMiddle = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient used when the XPanderPanel is selected.
        /// </summary>
        [Description("The starting color of the gradient used when the XPanderPanel is selected.")]
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
        /// Gets or sets the end color of the gradient used when the XPanderPanel is selected.
        /// </summary>
        [Description("The end color of the gradient used when the XPanderPanel is selected.")]
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
        /// Gets or sets the middle color of the gradient used when the XPanderPanel is selected.
        /// </summary>
        [Description("The middle color of the gradient used when the XPanderPanel is selected.")]
        public virtual Color CaptionSelectedGradientMiddle
        {
            get { return _captionSelectedGradientMiddle; }
            set
            {
                if (value.Equals(_captionSelectedGradientMiddle) == false)
                {
                    _captionSelectedGradientMiddle = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the text color used when the XPanderPanel is selected.
        /// </summary>
        [Description("The text color used when the XPanderPanel is selected.")]
        public virtual Color CaptionSelectedText
        {
            get { return _captionSelectedText; }
            set
            {
                if (value.Equals(_captionSelectedText) == false)
                {
                    _captionSelectedText = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion
    }
}
