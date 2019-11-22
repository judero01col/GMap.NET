using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

namespace Demo.WindowsPresentation.CustomMarkers
{
    /// <summary>
    ///     Interaction logic for CustomMarkerDemo.xaml
    /// </summary>
    public partial class CustomMarkerDemo
    {
        private Popup _popup;
        private Label _label;
        private GMapMarker _marker;
        private MainWindow _mainWindow;

        public CustomMarkerDemo(MainWindow window, GMapMarker marker, string title)
        {
            InitializeComponent();

            _mainWindow = window;
            _marker = marker;

            _popup = new Popup();
            _label = new Label();

            Unloaded += CustomMarkerDemo_Unloaded;
            Loaded += CustomMarkerDemo_Loaded;
            SizeChanged += CustomMarkerDemo_SizeChanged;
            MouseEnter += MarkerControl_MouseEnter;
            MouseLeave += MarkerControl_MouseLeave;
            MouseMove += CustomMarkerDemo_MouseMove;
            MouseLeftButtonUp += CustomMarkerDemo_MouseLeftButtonUp;
            MouseLeftButtonDown += CustomMarkerDemo_MouseLeftButtonDown;

            _popup.Placement = PlacementMode.Mouse;
            {
                _label.Background = Brushes.Blue;
                _label.Foreground = Brushes.White;
                _label.BorderBrush = Brushes.WhiteSmoke;
                _label.BorderThickness = new Thickness(2);
                _label.Padding = new Thickness(5);
                _label.FontSize = 22;
                _label.Content = title;
            }
            _popup.Child = _label;
        }

        void CustomMarkerDemo_Loaded(object sender, RoutedEventArgs e)
        {
            if (Icon.Source.CanFreeze)
            {
                Icon.Source.Freeze();
            }
        }

        void CustomMarkerDemo_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= CustomMarkerDemo_Unloaded;
            Loaded -= CustomMarkerDemo_Loaded;
            SizeChanged -= CustomMarkerDemo_SizeChanged;
            MouseEnter -= MarkerControl_MouseEnter;
            MouseLeave -= MarkerControl_MouseLeave;
            MouseMove -= CustomMarkerDemo_MouseMove;
            MouseLeftButtonUp -= CustomMarkerDemo_MouseLeftButtonUp;
            MouseLeftButtonDown -= CustomMarkerDemo_MouseLeftButtonDown;

            _marker.Shape = null;
            Icon.Source = null;
            Icon = null;
            _popup = null;
            _label = null;
        }

        void CustomMarkerDemo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height);
        }

        void CustomMarkerDemo_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var p = e.GetPosition(_mainWindow.MainMap);
                _marker.Position = _mainWindow.MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        void CustomMarkerDemo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void CustomMarkerDemo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            _marker.ZIndex -= 10000;
            _popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _marker.ZIndex += 10000;
            _popup.IsOpen = true;
        }
    }
}
