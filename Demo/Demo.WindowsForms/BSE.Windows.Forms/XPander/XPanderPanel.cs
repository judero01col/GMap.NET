using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms
{
   #region Class XPanderPanel
   /// <summary>
   /// Used to group collections of controls. 
   /// </summary>
   /// <remarks>
   /// XPanderPanel controls represent the expandable and collapsable panels in XPanderPanelList.
   /// The XpanderPanel is a control that contains other controls.
   /// You can use a XPanderPanel to group collections of controls such as the XPanderPanelList.
   /// The order of xpanderpanels in the XPanderPanelList.XPanderPanels collection reflects the order
   /// of xpanderpanels controls. To change the order of tabs in the control, you must change
   /// their positions in the collection by removing them and inserting them at new indexes.
   /// You can change the xpanderpanel's appearance. For example, to make it appear flat,
   /// set the CaptionStyle property to CaptionStyle.Flat.
   /// On top of the XPanderPanel there is the captionbar.
   /// This captionbar may contain an image and text. According to it's properties the panel is closable.
   /// </remarks>
   /// <copyright>Copyright © 2006-2008 Uwe Eichkorn
   /// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
   /// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
   /// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
   /// PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
   /// REMAINS UNCHANGED.
   /// </copyright>
   [Designer(typeof(XPanderPanelDesigner))]
   [DesignTimeVisible(false)]
   public partial class XPanderPanel : BasePanel
   {
      #region EventsPublic
      /// <summary>
      /// The CaptionStyleChanged event occurs when CaptionStyle flags have been changed.
      /// </summary>
      [Description("The CaptionStyleChanged event occurs when CaptionStyle flags have been changed.")]
      public event EventHandler<EventArgs> CaptionStyleChanged;
      #endregion

      #region Constants
      #endregion

      #region FieldsPrivate

      private Image m_imageChevron;
      private Image m_imageChevronUp;
      private Image m_imageChevronDown;
      private CustomXPanderPanelColors m_customColors;
      private Image m_imageClosePanel;
      private bool m_bIsClosable = true;
      private CaptionStyle m_captionStyle;

      #endregion

      #region Properties
      /// <summary>
      /// Gets or sets a value indicating whether the expand icon in a XPanderPanel is visible.
      /// </summary>
      [Description("Gets or sets a value indicating whether the expand icon in a XPanderPanel is visible.")]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [DefaultValue(false)]
      [Browsable(false)]
      [Category("Appearance")]
      public override bool ShowExpandIcon
      {
         get
         {
            return base.ShowExpandIcon;
         }
         set
         {
            base.ShowExpandIcon = value;
         }
      }
      /// <summary>
      /// Gets or sets a value indicating whether the close icon in a XPanderPanel is visible.
      /// </summary>
      [Description("Gets or sets a value indicating whether the close icon in a XPanderPanel is visible.")]
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [DefaultValue(false)]
      [Browsable(false)]
      [Category("Appearance")]
      public override bool ShowCloseIcon
      {
         get
         {
            return base.ShowCloseIcon;
         }
         set
         {
            base.ShowCloseIcon = value;
         }
      }
      /// <summary>
      /// Gets the custom colors which are used for the XPanderPanel.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
      [Description("The custom colors which are used for the XPanderPanel.")]
      [Category("Appearance")]
      public CustomXPanderPanelColors CustomColors
      {
         get
         {
            return m_customColors;
         }
      }
      /// <summary>
      /// Gets or sets the style of the caption (not for PanelStyle.Aqua).
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      [Browsable(false)]
      public CaptionStyle CaptionStyle
      {
         get
         {
            return m_captionStyle;
         }
         set
         {
            if(value.Equals(m_captionStyle) == false)
            {
               m_captionStyle = value;
               OnCaptionStyleChanged(this, EventArgs.Empty);
            }
         }
      }
      /// <summary>
      /// Gets or sets a value indicating whether this XPanderPanel is closable.
      /// </summary>
      [Description("Gets or sets a value indicating whether this XPanderPanel is closable.")]
      [DefaultValue(true)]
      [Category("Appearance")]
      public bool IsClosable
      {
         get
         {
            return m_bIsClosable;
         }
         set
         {
            if(value.Equals(m_bIsClosable) == false)
            {
               m_bIsClosable = value;
               Invalidate(false);
            }
         }
      }
      /// <summary>
      /// Gets or sets the height and width of the XPanderPanel.
      /// </summary>
      [Browsable(false)]
      public new Size Size
      {
         get
         {
            return base.Size;
         }
         set
         {
            base.Size = value;
         }
      }
      #endregion

      #region MethodsPublic
      /// <summary>
      /// Initializes a new instance of the XPanderPanel class.
      /// </summary>
      public XPanderPanel()
      {
         InitializeComponent();

         BackColor = Color.Transparent;
         CaptionStyle = CaptionStyle.Normal;
         ForeColor = SystemColors.ControlText;
         Height = CaptionHeight;
         ShowBorder = true;
         m_customColors = new CustomXPanderPanelColors();
         m_customColors.CustomColorsChanged += OnCustomColorsChanged;
      }

      /// <summary>
      /// Gets the rectangle that represents the display area of the XPanderPanel.
      /// </summary>
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
      public override Rectangle DisplayRectangle
      {
         get
         {
            Padding padding = Padding;

            Rectangle displayRectangle = new Rectangle(
                padding.Left + Constants.BorderThickness,
                padding.Top + CaptionHeight,
                ClientRectangle.Width - padding.Left - padding.Right - (2 * Constants.BorderThickness),
                ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom);

            if(Controls.Count > 0)
            {
               XPanderPanelList xpanderPanelList = Controls[0] as XPanderPanelList;
               if((xpanderPanelList != null) && (xpanderPanelList.Dock == DockStyle.Fill))
               {
                  displayRectangle = new Rectangle(
                      padding.Left,
                      padding.Top + CaptionHeight,
                      ClientRectangle.Width - padding.Left - padding.Right,
                      ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom - Constants.BorderThickness);
               }
            }
            return displayRectangle;
         }
      }
      #endregion

      #region MethodsProtected
      /// <summary>
      /// Paints the background of the control.
      /// </summary>
      /// <param name="pevent">A PaintEventArgs that contains information about the control to paint.</param>
      protected override void OnPaintBackground(PaintEventArgs pevent)
      {
         base.OnPaintBackground(pevent);
         base.BackColor = Color.Transparent;
         Color backColor = PanelColors.XPanderPanelBackColor;

         if((backColor != Color.Empty) && backColor != Color.Transparent)
         {
            Rectangle rectangle = new Rectangle(
                0,
                CaptionHeight,
                ClientRectangle.Width,
                ClientRectangle.Height - CaptionHeight);

            using(SolidBrush backgroundBrush = new SolidBrush(backColor))
            {
               pevent.Graphics.FillRectangle(backgroundBrush, rectangle);
            }
         }
      }
      /// <summary>
      /// Raises the Paint event.
      /// </summary>
      /// <param name="e">A PaintEventArgs that contains the event data.</param>
      protected override void OnPaint(PaintEventArgs e)
      {
         if(IsZeroWidthOrHeight(CaptionRectangle))
         {
            return;
         }

         using(UseAntiAlias antiAlias = new UseAntiAlias(e.Graphics))
         {
            Graphics graphics = e.Graphics;

            using(UseClearTypeGridFit clearTypeGridFit = new UseClearTypeGridFit(graphics))
            {
               bool bExpand = Expand;
               bool bShowBorder = ShowBorder;
               Color borderColor = PanelColors.BorderColor;
               Rectangle borderRectangle = ClientRectangle;

               switch(PanelStyle)
               {
                  case PanelStyle.Default:
                  case PanelStyle.Office2007:
                  DrawCaptionbar(graphics, bExpand, bShowBorder, PanelStyle);
                  CalculatePanelHeights();
                  DrawBorders(graphics, this);
                  break;
               }
            }
         }
      }
      /// <summary>
      /// Raises the PanelExpanding event.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">A XPanderStateChangeEventArgs that contains the event data.</param>
      protected override void OnPanelExpanding(object sender, XPanderStateChangeEventArgs e)
      {
         bool bExpand = e.Expand;

         if(bExpand)
         {
            Expand = bExpand;
            Invalidate(false);
         }

         base.OnPanelExpanding(sender, e);
      }
      /// <summary>
      /// Raises the CaptionStyleChanged event.
      /// </summary>
      /// <param name="sender">The source of the event.</param>
      /// <param name="e">An EventArgs that contains the event data.</param>
      protected virtual void OnCaptionStyleChanged(object sender, EventArgs e)
      {
         Invalidate(CaptionRectangle);
         if(CaptionStyleChanged != null)
         {
            CaptionStyleChanged(sender, e);
         }
      }
      /// <summary>
      /// Raises the MouseUp event.
      /// </summary>
      /// <param name="e">A MouseEventArgs that contains data about the OnMouseUp event.</param>
      protected override void OnMouseUp(MouseEventArgs e)
      {
         if(CaptionRectangle.Contains(e.X, e.Y))
         {
            if((ShowCloseIcon == false) && (ShowExpandIcon == false))
            {
               OnExpandClick(this, EventArgs.Empty);
            }
            else if(ShowCloseIcon && (ShowExpandIcon == false))
            {
               if(RectangleCloseIcon.Contains(e.X, e.Y) == false)
               {
                  OnExpandClick(this, EventArgs.Empty);
               }
            }
            if(ShowExpandIcon)
            {
               if(RectangleExpandIcon.Contains(e.X, e.Y))
               {
                  OnExpandClick(this, EventArgs.Empty);
               }
            }
            if(ShowCloseIcon && m_bIsClosable)
            {
               if(RectangleCloseIcon.Contains(e.X, e.Y))
               {
                  OnCloseClick(this, EventArgs.Empty);
               }
            }
         }
      }
      /// <summary>
      /// Raises the VisibleChanged event.
      /// </summary>
      /// <param name="e">An EventArgs that contains the event data.</param>
      protected override void OnVisibleChanged(EventArgs e)
      {
         base.OnVisibleChanged(e);

         if(DesignMode)
         {
            return;
         }
         if(Visible == false)
         {
            if(Expand)
            {
               Expand = false;
               foreach(Control control in Parent.Controls)
               {
                  XPanderPanel xpanderPanel =
                            control as XPanderPanel;

                  if(xpanderPanel != null)
                  {
                     if(xpanderPanel.Visible)
                     {
                        xpanderPanel.Expand = true;
                        return;
                     }
                  }
               }
            }
         }
#if DEBUG
         //System.Diagnostics.Trace.WriteLine("Visibility: " + this.Name + this.Visible);
#endif
         CalculatePanelHeights();
      }

      #endregion

      #region MethodsPrivate

      private void DrawCaptionbar(Graphics graphics, bool bExpand, bool bShowBorder, PanelStyle panelStyle)
      {
         Rectangle captionRectangle = CaptionRectangle;
         Color colorGradientBegin = PanelColors.XPanderPanelCaptionGradientBegin;
         Color colorGradientEnd = PanelColors.XPanderPanelCaptionGradientEnd;
         Color colorGradientMiddle = PanelColors.XPanderPanelCaptionGradientMiddle;
         Color colorText = PanelColors.XPanderPanelCaptionText;
         Color foreColorCloseIcon = PanelColors.XPanderPanelCaptionCloseIcon;
         Color foreColorExpandIcon = PanelColors.XPanderPanelCaptionExpandIcon;
         bool bHover = HoverStateCaptionBar == HoverState.Hover ? true : false;

         if(m_imageClosePanel == null)
         {
            m_imageClosePanel = Resources.closePanel;
         }
         if(m_imageChevronUp == null)
         {
            m_imageChevronUp = Resources.ChevronUp;
         }
         if(m_imageChevronDown == null)
         {
            m_imageChevronDown = Resources.ChevronDown;
         }

         m_imageChevron = m_imageChevronDown;
         if(bExpand)
         {
            m_imageChevron = m_imageChevronUp;
         }

         if(m_captionStyle == CaptionStyle.Normal)
         {
            if(bHover)
            {
               colorGradientBegin = PanelColors.XPanderPanelSelectedCaptionBegin;
               colorGradientEnd = PanelColors.XPanderPanelSelectedCaptionEnd;
               colorGradientMiddle = PanelColors.XPanderPanelSelectedCaptionMiddle;
               if(bExpand)
               {
                  colorGradientBegin = PanelColors.XPanderPanelPressedCaptionBegin;
                  colorGradientEnd = PanelColors.XPanderPanelPressedCaptionEnd;
                  colorGradientMiddle = PanelColors.XPanderPanelPressedCaptionMiddle;
               }
               colorText = PanelColors.XPanderPanelSelectedCaptionText;
               foreColorCloseIcon = colorText;
               foreColorExpandIcon = colorText;
            }
            else
            {
               if(bExpand)
               {
                  colorGradientBegin = PanelColors.XPanderPanelCheckedCaptionBegin;
                  colorGradientEnd = PanelColors.XPanderPanelCheckedCaptionEnd;
                  colorGradientMiddle = PanelColors.XPanderPanelCheckedCaptionMiddle;
                  colorText = PanelColors.XPanderPanelSelectedCaptionText;
                  foreColorCloseIcon = colorText;
                  foreColorExpandIcon = colorText;
               }
            }
            if(panelStyle != PanelStyle.Office2007)
            {
               RenderDoubleBackgroundGradient(
               graphics,
               captionRectangle,
               colorGradientBegin,
               colorGradientMiddle,
               colorGradientEnd,
               LinearGradientMode.Vertical,
               false);
            }
            else
            {
               RenderButtonBackground(
                   graphics,
                   captionRectangle,
                   colorGradientBegin,
                   colorGradientMiddle,
                   colorGradientEnd);
            }
         }
         else
         {
            Color colorFlatGradientBegin = PanelColors.XPanderPanelFlatCaptionGradientBegin;
            Color colorFlatGradientEnd = PanelColors.XPanderPanelFlatCaptionGradientEnd;
            Color colorInnerBorder = PanelColors.InnerBorderColor;
            colorText = PanelColors.XPanderPanelCaptionText;
            foreColorExpandIcon = colorText;

            RenderFlatButtonBackground(graphics, captionRectangle, colorFlatGradientBegin, colorFlatGradientEnd, bHover);
            DrawInnerBorders(graphics, this);
         }

         DrawImagesAndText(
             graphics,
             captionRectangle,
             CaptionSpacing,
             ImageRectangle,
             Image,
             RightToLeft,
             m_bIsClosable,
             ShowCloseIcon,
             m_imageClosePanel,
             foreColorCloseIcon,
             ref RectangleCloseIcon,
             ShowExpandIcon,
             m_imageChevron,
             foreColorExpandIcon,
             ref RectangleExpandIcon,
             CaptionFont,
             colorText,
             Text);
      }

      private static void DrawBorders(Graphics graphics, XPanderPanel xpanderPanel)
      {
         if(xpanderPanel.ShowBorder)
         {
            using(GraphicsPath graphicsPath = new GraphicsPath())
            {
               using(Pen borderPen = new Pen(xpanderPanel.PanelColors.BorderColor, Constants.BorderThickness))
               {
                  Rectangle captionRectangle = xpanderPanel.CaptionRectangle;
                  Rectangle borderRectangle = captionRectangle;

                  if(xpanderPanel.Expand)
                  {
                     borderRectangle = xpanderPanel.ClientRectangle;

                     graphics.DrawLine(
                         borderPen,
                         captionRectangle.Left,
                         captionRectangle.Top + captionRectangle.Height - Constants.BorderThickness,
                         captionRectangle.Left + captionRectangle.Width,
                         captionRectangle.Top + captionRectangle.Height - Constants.BorderThickness);
                  }

                  XPanderPanelList xpanderPanelList = xpanderPanel.Parent as XPanderPanelList;
                  if((xpanderPanelList != null) && (xpanderPanelList.Dock == DockStyle.Fill))
                  {
                     Panel panel = xpanderPanelList.Parent as Panel;
                     XPanderPanel parentXPanderPanel = xpanderPanelList.Parent as XPanderPanel;
                     if(((panel != null) && (panel.Padding == new Padding(0))) ||
                                ((parentXPanderPanel != null) && (parentXPanderPanel.Padding == new Padding(0))))
                     {
                        if(xpanderPanel.Top != 0)
                        {
                           graphicsPath.AddLine(
                               borderRectangle.Left,
                               borderRectangle.Top,
                               borderRectangle.Left + captionRectangle.Width,
                               borderRectangle.Top);
                        }

                        // Left vertical borderline
                        graphics.DrawLine(borderPen,
                            borderRectangle.Left,
                            borderRectangle.Top,
                            borderRectangle.Left,
                            borderRectangle.Top + borderRectangle.Height);

                        // Right vertical borderline
                        graphics.DrawLine(borderPen,
                            borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                            borderRectangle.Top,
                            borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                            borderRectangle.Top + borderRectangle.Height);
                     }
                     else
                     {
                        // Upper horizontal borderline only at the top xpanderPanel
                        if(xpanderPanel.Top == 0)
                        {
                           graphicsPath.AddLine(
                               borderRectangle.Left,
                               borderRectangle.Top,
                               borderRectangle.Left + borderRectangle.Width,
                               borderRectangle.Top);
                        }

                        // Left vertical borderline
                        graphicsPath.AddLine(
                            borderRectangle.Left,
                            borderRectangle.Top,
                            borderRectangle.Left,
                            borderRectangle.Top + borderRectangle.Height);

                        //Lower horizontal borderline
                        graphicsPath.AddLine(
                            borderRectangle.Left,
                            borderRectangle.Top + borderRectangle.Height - Constants.BorderThickness,
                            borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                            borderRectangle.Top + borderRectangle.Height - Constants.BorderThickness);

                        // Right vertical borderline
                        graphicsPath.AddLine(
                            borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                            borderRectangle.Top,
                            borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                            borderRectangle.Top + borderRectangle.Height);
                     }
                  }
                  else
                  {
                     // Upper horizontal borderline only at the top xpanderPanel
                     if(xpanderPanel.Top == 0)
                     {
                        graphicsPath.AddLine(
                            borderRectangle.Left,
                            borderRectangle.Top,
                            borderRectangle.Left + borderRectangle.Width,
                            borderRectangle.Top);
                     }

                     // Left vertical borderline
                     graphicsPath.AddLine(
                         borderRectangle.Left,
                         borderRectangle.Top,
                         borderRectangle.Left,
                         borderRectangle.Top + borderRectangle.Height);

                     //Lower horizontal borderline
                     graphicsPath.AddLine(
                         borderRectangle.Left,
                         borderRectangle.Top + borderRectangle.Height - Constants.BorderThickness,
                         borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                         borderRectangle.Top + borderRectangle.Height - Constants.BorderThickness);

                     // Right vertical borderline
                     graphicsPath.AddLine(
                         borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                         borderRectangle.Top,
                         borderRectangle.Left + borderRectangle.Width - Constants.BorderThickness,
                         borderRectangle.Top + borderRectangle.Height);
                  }
               }
               using(Pen borderPen = new Pen(xpanderPanel.PanelColors.BorderColor, Constants.BorderThickness))
               {
                  graphics.DrawPath(borderPen, graphicsPath);
               }
            }
         }
      }


      private static void DrawInnerBorders(Graphics graphics, XPanderPanel xpanderPanel)
      {
         if(xpanderPanel.ShowBorder)
         {
            using(GraphicsPath graphicsPath = new GraphicsPath())
            {
               Rectangle captionRectangle = xpanderPanel.CaptionRectangle;
               XPanderPanelList xpanderPanelList = xpanderPanel.Parent as XPanderPanelList;
               if((xpanderPanelList != null) && (xpanderPanelList.Dock == DockStyle.Fill))
               {
                  Panel panel = xpanderPanelList.Parent as Panel;
                  XPanderPanel parentXPanderPanel = xpanderPanelList.Parent as XPanderPanel;
                  if(((panel != null) && (panel.Padding == new Padding(0))) ||
                            ((parentXPanderPanel != null) && (parentXPanderPanel.Padding == new Padding(0))))
                  {
                     //Left vertical borderline
                     graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y + captionRectangle.Height, captionRectangle.X, captionRectangle.Y + Constants.BorderThickness);
                     if(xpanderPanel.Top == 0)
                     {
                        //Upper horizontal borderline
                        graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y, captionRectangle.X + captionRectangle.Width, captionRectangle.Y);
                     }
                     else
                     {
                        //Upper horizontal borderline
                        graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y + Constants.BorderThickness, captionRectangle.X + captionRectangle.Width, captionRectangle.Y + Constants.BorderThickness);
                     }
                  }
               }
               else
               {
                  //Left vertical borderline
                  graphicsPath.AddLine(captionRectangle.X + Constants.BorderThickness, captionRectangle.Y + captionRectangle.Height, captionRectangle.X + Constants.BorderThickness, captionRectangle.Y);
                  if(xpanderPanel.Top == 0)
                  {
                     //Upper horizontal borderline
                     graphicsPath.AddLine(captionRectangle.X + Constants.BorderThickness, captionRectangle.Y + Constants.BorderThickness, captionRectangle.X + captionRectangle.Width - Constants.BorderThickness, captionRectangle.Y + Constants.BorderThickness);
                  }
                  else
                  {
                     //Upper horizontal borderline
                     graphicsPath.AddLine(captionRectangle.X + Constants.BorderThickness, captionRectangle.Y, captionRectangle.X + captionRectangle.Width - Constants.BorderThickness, captionRectangle.Y);
                  }
               }

               using(Pen borderPen = new Pen(xpanderPanel.PanelColors.InnerBorderColor))
               {
                  graphics.DrawPath(borderPen, graphicsPath);
               }
            }
         }
      }

      private void CalculatePanelHeights()
      {
         if(Parent == null)
         {
            return;
         }

         int iPanelHeight = Parent.Padding.Top;

         foreach(Control control in Parent.Controls)
         {
            XPanderPanel xpanderPanel =
					control as XPanderPanel;

            if((xpanderPanel != null) && xpanderPanel.Visible)
            {
               iPanelHeight += xpanderPanel.CaptionHeight;
            }
         }

         iPanelHeight += Parent.Padding.Bottom;

         foreach(Control control in Parent.Controls)
         {
            XPanderPanel xpanderPanel =
					control as XPanderPanel;

            if(xpanderPanel != null)
            {
               if(xpanderPanel.Expand)
               {
                  xpanderPanel.Height = Parent.Height
                            + xpanderPanel.CaptionHeight
                            - iPanelHeight;
               }
               else
               {
                  xpanderPanel.Height = xpanderPanel.CaptionHeight;
               }
            }
         }

         int iTop = Parent.Padding.Top;
         foreach(Control control in Parent.Controls)
         {
            XPanderPanel xpanderPanel =
					control as XPanderPanel;

            if((xpanderPanel != null) && xpanderPanel.Visible)
            {
               xpanderPanel.Top = iTop;
               iTop += xpanderPanel.Height;
            }
         }
      }

      #endregion
   }

   #endregion

   #region Class XPanderPanelDesigner
   /// <summary>
   /// Designer class for extending the design mode behavior of a XPanderPanel control
   /// </summary>
   internal class XPanderPanelDesigner : System.Windows.Forms.Design.ScrollableControlDesigner
   {
      #region FieldsPrivate

      private Pen m_borderPen = new Pen(Color.FromKnownColor(KnownColor.ControlDarkDark));
      private System.Windows.Forms.Design.Behavior.Adorner m_adorner;

      #endregion

      #region MethodsPublic
      /// <summary>
      /// Initializes a new instance of the XPanderPanelDesigner class.
      /// </summary>
      public XPanderPanelDesigner()
      {
         m_borderPen.DashStyle = DashStyle.Dash;
      }
      /// <summary>
      /// Initializes the designer with the specified component.
      /// </summary>
      /// <param name="component">The IComponent to associate with the designer.</param>
      public override void Initialize(IComponent component)
      {
         base.Initialize(component);
         XPanderPanel xpanderPanel = Control as XPanderPanel;
         if(xpanderPanel != null)
         {
            m_adorner = new System.Windows.Forms.Design.Behavior.Adorner();
            BehaviorService.Adorners.Add(m_adorner);
            m_adorner.Glyphs.Add(new XPanderPanelCaptionGlyph(BehaviorService, xpanderPanel));
         }
      }
      #endregion

      #region MethodsProtected
      /// <summary>
      /// Releases the unmanaged resources used by the XPanderPanelDesigner,
      /// and optionally releases the managed resources. 
      /// </summary>
      /// <param name="disposing">true to release both managed and unmanaged resources;
      /// false to release only unmanaged resources.</param>
      protected override void Dispose(bool disposing)
      {
         try
         {
            if(disposing)
            {
               if(m_borderPen != null)
               {
                  m_borderPen.Dispose();
               }
               if(m_adorner != null)
               {
                  if(BehaviorService != null)
                  {
                     BehaviorService.Adorners.Remove(m_adorner);
                  }
               }
            }
         }
         finally
         {
            base.Dispose(disposing);
         }
      }
      /// <summary>
      /// Receives a call when the control that the designer is managing has painted its surface so the designer can
      /// paint any additional adornments on top of the xpanderpanel.
      /// </summary>
      /// <param name="e">A PaintEventArgs the designer can use to draw on the xpanderpanel.</param>
      protected override void OnPaintAdornments(PaintEventArgs e)
      {
         base.OnPaintAdornments(e);
         e.Graphics.DrawRectangle(
            m_borderPen,
            0,
            0,
            Control.Width - 2,
            Control.Height - 2);
      }
      /// <summary>
      /// Allows a designer to change or remove items from the set of properties that it exposes through a <see cref="TypeDescriptor">TypeDescriptor</see>. 
      /// </summary>
      /// <param name="properties">The properties for the class of the component.</param>
      protected override void PostFilterProperties(IDictionary properties)
      {
         base.PostFilterProperties(properties);
         properties.Remove("AccessibilityObject");
         properties.Remove("AccessibleDefaultActionDescription");
         properties.Remove("AccessibleDescription");
         properties.Remove("AccessibleName");
         properties.Remove("AccessibleRole");
         properties.Remove("AllowDrop");
         properties.Remove("Anchor");
         properties.Remove("AntiAliasing");
         properties.Remove("AutoScroll");
         properties.Remove("AutoScrollMargin");
         properties.Remove("AutoScrollMinSize");
         properties.Remove("BackColor");
         properties.Remove("BackgroundImage");
         properties.Remove("BackgroundImageLayout");
         properties.Remove("CausesValidation");
         properties.Remove("ContextMenuStrip");
         properties.Remove("Dock");
         properties.Remove("GenerateMember");
         properties.Remove("ImeMode");
         properties.Remove("Location");
         properties.Remove("MaximumSize");
         properties.Remove("MinimumSize");
      }

      #endregion
   }
   #endregion

   #region Class XPanderPanelCaptionGlyph
   /// <summary>
   /// Represents a single user interface (UI) entity managed by an Adorner.
   /// </summary>
   internal class XPanderPanelCaptionGlyph : System.Windows.Forms.Design.Behavior.Glyph
   {
      #region FieldsPrivate

      private XPanderPanel m_xpanderPanel;
      private System.Windows.Forms.Design.Behavior.BehaviorService m_behaviorService;

      #endregion

      #region Properties
      /// <summary>
      /// Gets the bounds of the Glyph.
      /// </summary>
      public override Rectangle Bounds
      {
         get
         {
            Point edge = m_behaviorService.ControlToAdornerWindow(m_xpanderPanel);
            Rectangle bounds = new Rectangle(
                edge.X,
                edge.Y,
                m_xpanderPanel.Width,
                m_xpanderPanel.CaptionHeight);

            return bounds;
         }
      }
      #endregion

      #region MethodsPublic
      /// <summary>
      /// Initializes a new instance of the CaptionGlyph class.
      /// </summary>
      /// <param name="behaviorService"></param>
      /// <param name="xpanderPanel"></param>
      public XPanderPanelCaptionGlyph(System.Windows.Forms.Design.Behavior.BehaviorService behaviorService, XPanderPanel xpanderPanel)
         :
         base(new XPanderPanelCaptionClickBehavior(xpanderPanel))
      {
         m_behaviorService = behaviorService;
         m_xpanderPanel = xpanderPanel;
      }
      /// <summary>
      /// Provides hit test logic.
      /// </summary>
      /// <param name="p">A point to hit-test.</param>
      /// <returns>A Cursor if the Glyph is associated with p; otherwise, a null reference</returns>
      public override Cursor GetHitTest(Point p)
      {
         // GetHitTest is called to see if the point is
         // within this glyph.  This gives us a chance to decide
         // what cursor to show.  Returning null from here means
         // the mouse pointer is not currently inside of the glyph.
         // Returning a valid cursor here indicates the pointer is
         // inside the glyph, and also enables our Behavior property
         // as the active behavior.
         if((m_xpanderPanel != null) && (m_xpanderPanel.Expand == false) && (Bounds.Contains(p)))
         {
            return Cursors.Hand;
         }

         return null;
      }
      /// <summary>
      /// Provides paint logic.
      /// </summary>
      /// <param name="pe">A PaintEventArgs that contains the event data.</param>
      public override void Paint(PaintEventArgs pe)
      {
      }

      #endregion
   }

   #endregion

   #region Class XPanderPanelCaptionClickBehavior
   /// <summary>
   /// Designer behaviour when the user clicks in the glyph on the XPanderPanel caption
   /// </summary>
   internal class XPanderPanelCaptionClickBehavior : System.Windows.Forms.Design.Behavior.Behavior
   {
      #region FieldsPrivate
      private XPanderPanel m_xpanderPanel;
      #endregion

      #region MethodsPublic
      /// <summary>
      /// Initializes a new instance of the Behavior class.
      /// </summary>
      /// <param name="xpanderPanel">XPanderPanel for this behaviour</param>
      public XPanderPanelCaptionClickBehavior(XPanderPanel xpanderPanel)
      {
         m_xpanderPanel = xpanderPanel;
      }
      /// <summary>
      /// Called when any mouse-down message enters the adorner window of the BehaviorService. 
      /// </summary>
      /// <param name="g">A Glyph.</param>
      /// <param name="button">A MouseButtons value indicating which button was clicked.</param>
      /// <param name="mouseLoc">The location at which the click occurred.</param>
      /// <returns>true if the message was handled; otherwise, false. </returns>
      public override bool OnMouseDown(System.Windows.Forms.Design.Behavior.Glyph g, MouseButtons button, Point mouseLoc)
      {
         if((m_xpanderPanel != null) && (m_xpanderPanel.Expand == false))
         {
            XPanderPanelList xpanderPanelList = m_xpanderPanel.Parent as XPanderPanelList;
            if(xpanderPanelList != null)
            {
               xpanderPanelList.Expand(m_xpanderPanel);
               m_xpanderPanel.Invalidate(false);
            }
         }
         return true; // indicating we processed this event.
      }
      #endregion
   }

   #endregion
}
