using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.Popups;
using System.Runtime.InteropServices;

namespace festiflo_logistics_controller
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private MapView _mapView;
    private GraphicsOverlay _clickEventOverlay;
    private static MapViewModel _mapVM;

    private Window _dragAdorner = null;
    private bool _eventPlacementMode = false;

    private SimpleMarkerSymbol _infoSymbol = 
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.LightBlue, 12);

    private SimpleMarkerSymbol _warningSymbol =
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Yellow, 12);

    private SimpleMarkerSymbol _closureSymbol =
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 12);

    public MainWindow()
    {
      InitializeComponent();
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      _clickEventOverlay = new GraphicsOverlay();
      if (EsriMapView is MapView mapView)
      {
        _mapView = mapView;
        _mapView.GraphicsOverlays.Add(_clickEventOverlay);
        _mapVM = _border.DataContext as MapViewModel;
      }
    }

    private void MapView_LeftMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is MapView mapView)
      {
        var mousePos = e.GetPosition(mapView);
        var mapRelativeLoc = mapView.ScreenToLocation(mousePos);

        if (_eventPlacementMode)
        {
          _mapVM.EventManagerViewModel.EventLocation = mapRelativeLoc;
          var type = _mapVM.EventManagerViewModel.SelectedEventType;

          var textColor = System.Drawing.Color.Red;
          SimpleMarkerSymbol markerSym = null;
          if (type == EventsManagerViewModel.EventType.Information)
          {
            markerSym = _infoSymbol;
            textColor = System.Drawing.Color.DarkBlue;
          }
          else if (type == EventsManagerViewModel.EventType.Warning)
          {
            markerSym = _warningSymbol;
            textColor = System.Drawing.Color.LightYellow;
          }
          else if (type == EventsManagerViewModel.EventType.Closure)
          {
            markerSym = _closureSymbol;
            textColor = System.Drawing.Color.Red;
          }

          var textSym = new TextSymbol()
          {
            Text = _mapVM.EventManagerViewModel.EventTitle,
            Color = textColor,
            HaloColor = System.Drawing.Color.White,
            HaloWidth = 2,
            Size = 14,
            HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
            VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Bottom
          };

          var markerGraphic = new Graphic(mapRelativeLoc, markerSym);
          var textGraphic = new Graphic(mapRelativeLoc, textSym);
          if (_clickEventOverlay != null)
          {
            _clickEventOverlay.Graphics.Add(markerGraphic);
            _clickEventOverlay.Graphics.Add(textGraphic);
          }

          // remove the visual feedback
          if (_dragAdorner != null)
          {
            _dragAdorner.Close();
            _dragAdorner = null;
          }

          _eventPlacementMode = false;
        } 
      }
    }

    private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (_eventPlacementMode)
      {
        // update the visual feedback
        var mouse = e.GetPosition(null);

        _dragAdorner.Left = mouse.X;
        _dragAdorner.Top = mouse.Y;
      }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
      public Int32 X;
      public Int32 Y;
    };

    public static Point GetMousePosition()
    {
      Win32Point w32Mouse = new Win32Point();
      GetCursorPos(ref w32Mouse);
      return new Point(w32Mouse.X, w32Mouse.Y);
    }

    private void LocationButton_Click(object sender, RoutedEventArgs e)
    {
      if (!_eventPlacementMode)
      {
        var mouse = GetMousePosition();
        //var location = Application.Current.MainWindow.PointFromScreen(mouse);

        StackPanel visualStackPanel = new StackPanel { Orientation = Orientation.Vertical };
        var textElement = TitleField;
        var rect = new Rectangle
        {
          Width = TitleField.ActualWidth,
          Height = TitleField.ActualHeight,
          Fill = new VisualBrush(TitleField as Visual),
        };

        visualStackPanel.Children.Add(rect);

        _dragAdorner = new Window
        {
          WindowStyle = WindowStyle.None,
          AllowsTransparency = true,
          AllowDrop = false,
          Background = null,
          IsHitTestVisible = false,
          SizeToContent = SizeToContent.WidthAndHeight,
          Topmost = true,
          ShowInTaskbar = false,
          Content = visualStackPanel as Visual,
          Left = mouse.X,
          Top = mouse.Y
        };

        _dragAdorner.Show();
        _eventPlacementMode = true;
      }
      else
      {
        // remove the visual feedback
        if (_dragAdorner != null)
        {
          _dragAdorner.Close();
          _dragAdorner = null;
        }

        _eventPlacementMode = false;
      }
    }

    // Map initialization logic is contained in MapViewModel.cs
  }
}
