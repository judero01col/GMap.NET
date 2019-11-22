using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace BSE.Windows.Forms
{
    /// <summary>
    /// Base class for the custom colors at a panel or xpanderpanel control. 
    /// </summary>
    /// <remarks>
    /// If you use the <see cref="ColorScheme.Custom"/> ColorScheme, this is the base class for the custom colors.
    /// </remarks>
    /// <copyright>Copyright © 2008 Uwe Eichkorn
    /// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    /// PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
    /// REMAINS UNCHANGED.
    /// </copyright>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("The colors used in a panel")]
    public class CustomColors
    {
        #region Events
        /// <summary>
        /// Occurs when the value of the CustomColors property changes.
        /// </summary>
        [Description("Occurs when the value of the CustomColors property changes.")]
        public event EventHandler<EventArgs> CustomColorsChanged;
        #endregion
        
        #region FieldsPrivate
        private Color _borderColor = System.Windows.Forms.ProfessionalColors.GripDark;
        private Color _captionCloseIcon = SystemColors.ControlText;
        private Color _captionExpandIcon = SystemColors.ControlText;
        private Color _captionGradientBegin = System.Windows.Forms.ProfessionalColors.ToolStripGradientBegin;
        private Color _captionGradientEnd = System.Windows.Forms.ProfessionalColors.ToolStripGradientEnd;
        private Color _captionGradientMiddle = System.Windows.Forms.ProfessionalColors.ToolStripGradientMiddle;
        private Color _captionText = SystemColors.ControlText;
        private Color _innerBorderColor = System.Windows.Forms.ProfessionalColors.GripLight;
        
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the border color of a Panel or XPanderPanel.
        /// </summary>
        [Description("The border color of a Panel or XPanderPanel.")]
        public virtual Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (value.Equals(_borderColor) == false)
                {
                    _borderColor = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the forecolor of a close icon in a Panel or XPanderPanel.
        /// </summary>
        [Description("The forecolor of a close icon in a Panel or XPanderPanel.")]
        public virtual Color CaptionCloseIcon
        {
            get { return _captionCloseIcon; }
            set
            {
                if (value.Equals(_captionCloseIcon) == false)
                {
                    _captionCloseIcon = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the forecolor of an expand icon in a Panel or XPanderPanel.
        /// </summary>
        [Description("The forecolor of an expand icon in a Panel or XPanderPanel.")]
        public virtual Color CaptionExpandIcon
        {
            get { return _captionExpandIcon; }
            set
            {
                if (value.Equals(_captionExpandIcon) == false)
                {
                    _captionExpandIcon = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the starting color of the gradient at the caption on a Panel or XPanderPanel.
        /// </summary>
        [Description("The starting color of the gradient at the caption on a Panel or XPanderPanel.")]
        public virtual Color CaptionGradientBegin
        {
            get { return _captionGradientBegin; }
            set
            {
                if (value.Equals(_captionGradientBegin) == false)
                {
                    _captionGradientBegin = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the end color of the gradient at the caption on a Panel or XPanderPanel.
        /// </summary>
        [Description("The end color of the gradient at the caption on a Panel or XPanderPanel")]
        public virtual Color CaptionGradientEnd
        {
            get { return _captionGradientEnd; }
            set
            {
                if (value.Equals(_captionGradientEnd) == false)
                {
                    _captionGradientEnd = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the middle color of the gradient at the caption on a Panel or XPanderPanel.
        /// </summary>
        [Description("The middle color of the gradient at the caption on a Panel or XPanderPanel.")]
        public virtual Color CaptionGradientMiddle
        {
            get { return _captionGradientMiddle; }
            set
            {
                if (value.Equals(_captionGradientMiddle) == false)
                {
                    _captionGradientMiddle = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the text color at the caption on a Panel or XPanderPanel.
        /// </summary>
        [Description("The text color at the caption on a Panel or XPanderPanel.")]
        public virtual Color CaptionText
        {
            get { return _captionText; }
            set
            {
                if (value.Equals(_captionText) == false)
                {
                    _captionText = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Gets or sets the inner border color of a Panel.
        /// </summary>
        [Description("The inner border color of a Panel.")]
        public virtual Color InnerBorderColor
        {
            get { return _innerBorderColor; }
            set
            {
                if (value.Equals(_innerBorderColor) == false)
                {
                    _innerBorderColor = value;
                    OnCustomColorsChanged(this, EventArgs.Empty);
                }
            }
        }
        #endregion

        #region MethodsProtected
        /// <summary>
        /// Raises the CustomColors changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A EventArgs that contains the event data.</param>
        protected virtual void OnCustomColorsChanged(object sender, EventArgs e)
        {
            if (CustomColorsChanged != null)
            {
                CustomColorsChanged(sender, e);
            }
        }
        #endregion
    }
}
