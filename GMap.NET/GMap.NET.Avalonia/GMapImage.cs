using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GMap.NET.MapProviders;

namespace GMap.NET.Avalonia
{
    /// <summary>
    ///     image abstraction
    /// </summary>
    public class GMapImage : PureImage
    {
        public IImage? Img;

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

        public override PureImage? FromStream(Stream stream)
        {
            if (stream != null)
            {
                try
                {
                    var m = new Bitmap(stream);

                    var ret = new GMapImage { Img = m };

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
                    (ret.Img as Bitmap)?.Save(stream);
                    return true;
                }
                catch
                {
                    // ignore
                }
            }

            return false;
        }
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
