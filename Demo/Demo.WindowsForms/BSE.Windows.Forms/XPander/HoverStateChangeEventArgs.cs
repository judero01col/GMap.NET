using System;
using System.Collections.Generic;
using System.Text;

namespace BSE.Windows.Forms
{
    /// <summary>
    ///     Provides data for the HoverStateChange event.
    /// </summary>
    /// <copyright>
    ///     Copyright © 2008 Uwe Eichkorn
    ///     THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    ///     KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    ///     IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    ///     PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER
    ///     REMAINS UNCHANGED.
    /// </copyright>
    public class HoverStateChangeEventArgs : EventArgs
    {
        #region FieldsPrivate

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the HoverState.
        /// </summary>
        public HoverState HoverState { get; }

        #endregion

        #region MethodsPublic

        /// <summary>
        ///     Initializes a new instance of the HoverStateChangeEventArgs class.
        /// </summary>
        /// <param name="hoverState">The <see cref="HoverState" /> values.</param>
        public HoverStateChangeEventArgs(HoverState hoverState)
        {
            HoverState = hoverState;
        }

        #endregion
    }
}
