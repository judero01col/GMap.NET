using System.Diagnostics;
#if NETFRAMEWORK && !NET5_0_OR_GREATER
using System.Drawing;
using System.Drawing.Imaging;
using GMap.NET.Internals;
#endif
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GMap.NET.MapProviders;

namespace GMap.NET.WindowsPresentation
{
    /// <summary>
    ///     image abstraction
    /// </summary>
    public class GMapImage : PureImage
    {
        public ImageSource Img;

        public override void Dispose()
        {
            if (Img != null)
            {
                Img = null;
            }

            if (Data != null)
            {
                Data.Dispose();
                Data = null;
            }
        }
    }

    /// <summary>
    ///     image abstraction proxy
    /// </summary>
    public class GMapImageProxy : PureImageProxy
    {
        private GMapImageProxy()
        {
        }

        public static void Enable()
        {
            GMapProvider.TileImageProxy = Instance;
        }

        public static readonly GMapImageProxy Instance = new GMapImageProxy();

#if NETFRAMEWORK && !NET5_0_OR_GREATER
        internal ColorMatrix ColorMatrix;

        static readonly bool Win7OrLater = Stuff.IsRunningOnWin7OrLater();
#endif

        public override PureImage FromStream(Stream stream)
        {
            if (stream != null)
            {
                try
                {
                    GMapImage ret = null;
#if NETFRAMEWORK && !NET5_0_OR_GREATER
                    if (ColorMatrix == null)
                    {
#endif
                    var m = BitmapFrame.Create(stream,
                        BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.OnLoad);
                    ret = new GMapImage { Img = m };
#if NETFRAMEWORK && !NET5_0_OR_GREATER
                    }
                    else
                    {
                        var m = Image.FromStream(stream, true, !Win7OrLater);
                        var bitmap = ApplyColorMatrix(m, ColorMatrix);
                        using (var memory = new MemoryStream())
                        {
                            bitmap.Save(memory, ImageFormat.Png);
                            memory.Position = 0;

                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            ret = new GMapImage { Img = bitmapImage };
                        }
                    }
#endif
                    if (ret.Img.CanFreeze)
                    {
                        ret.Img.Freeze();
                    }
                    return ret;
                }
                catch
                {
                    stream.Position = 0;

                    int type = stream.Length > 0 ? stream.ReadByte() : 0;
                    Debug.WriteLine("WindowsPresentationImageProxy: unknown image format: " + type);
                }
            }

            return null;
        }

        public override bool Save(Stream stream, PureImage image)
        {
            var ret = (GMapImage)image;
            if (ret.Img != null)
            {
                try
                {
                    var e = new PngBitmapEncoder();
                    e.Frames.Add(BitmapFrame.Create(ret.Img as BitmapSource));
                    e.Save(stream);

                    return true;
                }
                catch
                {
                    // ignore
                }
            }

            return false;
        }

#if NETFRAMEWORK && !NET5_0_OR_GREATER
        Bitmap ApplyColorMatrix(Image original, ColorMatrix matrix)
        {
            // create a blank bitmap the same size as original
            var newBitmap = new Bitmap(original.Width, original.Height);

            using (original) // destroy original
            {
                // get a graphics object from the new image
                using (var g = Graphics.FromImage(newBitmap))
                {
                    // set the color matrix attribute
                    using (var attributes = new ImageAttributes())
                    {
                        attributes.SetColorMatrix(matrix);
                        g.DrawImage(original,
                            new Rectangle(0, 0, original.Width, original.Height),
                            0,
                            0,
                            original.Width,
                            original.Height,
                            GraphicsUnit.Pixel,
                            attributes);
                    }
                }
            }
            return newBitmap;
        }
#endif
    }

    //internal class TileVisual : FrameworkElement
    //{
    //    public readonly ObservableCollection<ImageSource> Source;
    //    public readonly RawTile Tile;

    //    public TileVisual(IEnumerable<ImageSource> src, RawTile tile)
    //    {
    //        Opacity = 0;
    //        Tile = tile;

    //        Source = new ObservableCollection<ImageSource>(src);
    //        Source.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Source_CollectionChanged);

    //        this.Loaded += new RoutedEventHandler(ImageVisual_Loaded);
    //        this.Unloaded += new RoutedEventHandler(ImageVisual_Unloaded);
    //    }

    //    void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //    {
    //        if (IsLoaded)
    //        {
    //            switch (e.Action)
    //            {
    //                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
    //                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
    //                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
    //                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
    //                {
    //                    UpdateVisual();
    //                }
    //                break;

    //                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
    //                {
    //                    Child = null;
    //                }
    //                break;
    //            }
    //        }
    //    }

    //    void ImageVisual_Unloaded(object sender, RoutedEventArgs e)
    //    {
    //        Child = null;
    //    }

    //    void ImageVisual_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        UpdateVisual();
    //    }

    //    Visual _child;
    //    public virtual Visual Child
    //    {
    //        get
    //        {
    //            return _child;
    //        }
    //        set
    //        {
    //            if (_child != value)
    //            {
    //                if (_child != null)
    //                {
    //                    RemoveLogicalChild(_child);
    //                    RemoveVisualChild(_child);
    //                }

    //                if (value != null)
    //                {
    //                    AddVisualChild(value);
    //                    AddLogicalChild(value);
    //                }

    //                // cache the new child
    //                _child = value;

    //                InvalidateVisual();
    //            }
    //        }
    //    }

    //    public void UpdateVisual()
    //    {
    //        Child = Create();
    //    }

    //    static readonly Pen gridPen = new Pen(Brushes.White, 2.0);

    //    private DrawingVisual Create()
    //    {
    //        var dv = new DrawingVisual();

    //        using (DrawingContext dc = dv.RenderOpen())
    //        {
    //            foreach (var img in Source)
    //            {
    //                var rect = new Rect(0, 0, img.Width + 0.6, img.Height + 0.6);

    //                dc.DrawImage(img, rect);
    //                dc.DrawRectangle(null, gridPen, rect);
    //            }
    //        }

    //        return dv;
    //    }

    //    #region Necessary Overrides -- Needed by WPF to maintain bookkeeping of our hosted visuals
    //    protected override int VisualChildrenCount
    //    {
    //        get
    //        {
    //            return (Child == null ? 0 : 1);
    //        }
    //    }

    //    protected override Visual GetVisualChild(int index)
    //    {
    //        Debug.Assert(index == 0);
    //        return Child;
    //    }
    //    #endregion
    //}
}
