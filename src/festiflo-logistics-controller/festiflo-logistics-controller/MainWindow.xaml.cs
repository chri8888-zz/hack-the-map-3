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
    private Graphic _previewTextGraphic = null;
    private Graphic _previewMarkerGraphic = null;

    private List<Tuple<Graphic, Graphic>> _activeGraphics = new List<Tuple<Graphic, Graphic>>();

    private SimpleMarkerSymbol _infoSymbol = 
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.LightBlue, 12);

    private SimpleMarkerSymbol _warningSymbol =
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Gold, 12);

    private SimpleMarkerSymbol _closureSymbol =
      new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 12);

    public MainWindow()
    {
      InitializeComponent();
      Loaded += OnLoaded;

      var dir = System.AppDomain.CurrentDomain.BaseDirectory + "..\\..\\img\\widelogosmall.png";
      LogoImage.Source = new BitmapImage(new Uri(dir));
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
          if (_previewMarkerGraphic != null && _previewTextGraphic != null)
          {
            _previewMarkerGraphic.Geometry = mapRelativeLoc;
            _previewTextGraphic.Geometry = mapRelativeLoc;
            _eventPlacementMode = false;

            // remove the visual feedback
            if (_dragAdorner != null)
            {
              _dragAdorner.Close();
              _dragAdorner = null;
            }

            return;
          }

          _mapVM.EventManagerViewModel.EventLocation = mapRelativeLoc;
          var type = _mapVM.EventManagerViewModel.SelectedEventType;

          if (_clickEventOverlay != null)
          {
            var graphics = CreateEventGraphics(type, mapRelativeLoc, _mapVM.EventManagerViewModel.EventTitle);
            var markerGraphic = graphics.Item1;
            var textGraphic = graphics.Item2;

            _clickEventOverlay.Graphics.Add(markerGraphic);
            _clickEventOverlay.Graphics.Add(textGraphic);

            _previewMarkerGraphic = markerGraphic;
            _previewTextGraphic = textGraphic;

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

    private Tuple<Graphic, Graphic> CreateEventGraphics(EventsManagerViewModel.EventType type, MapPoint location, string title)
    {
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
        textColor = System.Drawing.Color.DarkOrange;
      }
      else if (type == EventsManagerViewModel.EventType.Closure)
      {
        markerSym = _closureSymbol;
        textColor = System.Drawing.Color.Red;
      }

      var textSym = new TextSymbol()
      {
        Text = title,
        Color = textColor,
        HaloColor = System.Drawing.Color.White,
        HaloWidth = 2,
        Size = 14,
        HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
        VerticalAlignment = Esri.ArcGISRuntime.Symbology.VerticalAlignment.Bottom
      };

      var markerGraphic = new Graphic(location, markerSym);
      var textGraphic = new Graphic(location, textSym);

      return new Tuple<Graphic, Graphic>(markerGraphic, textGraphic);
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

    private void RadioButtonInfo_Click(object sender, RoutedEventArgs e)
    {
      // remove other radio button 
      if (_border.DataContext is MapViewModel mapVM && mapVM.EventManagerViewModel != null)
        mapVM.EventManagerViewModel.SelectedEventType = EventsManagerViewModel.EventType.Information;

      RadioButtonWarning.IsChecked = false;
      RadioButtonClosure.IsChecked = false;
    }

    private void RadioButtonWarning_Click(object sender, RoutedEventArgs e)
    {
      if (_border.DataContext is MapViewModel mapVM && mapVM.EventManagerViewModel != null)
        mapVM.EventManagerViewModel.SelectedEventType = EventsManagerViewModel.EventType.Warning;

      RadioButtonInfo.IsChecked = false;
      RadioButtonClosure.IsChecked = false;
    }

    private void RadioButtonClosure_Click(object sender, RoutedEventArgs e)
    {
      if (_border.DataContext is MapViewModel mapVM && mapVM.EventManagerViewModel != null)
        mapVM.EventManagerViewModel.SelectedEventType = EventsManagerViewModel.EventType.Closure;

      RadioButtonWarning.IsChecked = false;
      RadioButtonInfo.IsChecked = false;
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_border.DataContext is MapViewModel mapVM)
      {
        string color = (HeatMapColor.SelectedItem as ComboBoxItem).Name;
        if (color == "Heat")
          mapVM.HeatMapColorPallette = DataUtils.ColorPalletteType.Heat;
        else if (color == "Blues")
          mapVM.HeatMapColorPallette = DataUtils.ColorPalletteType.Blues;
      }
    }

    private void SendEventButton_Click(object sender, RoutedEventArgs e)
    {
      _previewMarkerGraphic = null;
      _previewTextGraphic = null;
    }

    private void LoadEvents()
    {
      if (_border.DataContext is MapViewModel mapVM)
      {
        foreach (var graphicSet in _activeGraphics)
        {
          _clickEventOverlay.Graphics.Remove(graphicSet.Item1);
          _clickEventOverlay.Graphics.Remove(graphicSet.Item2);
        }

        var eventList = mapVM.EventManagerViewModel.ActiveEvents;
        foreach (var e in eventList)
        {
          var graphics = CreateEventGraphics(e.EventType, new MapPoint(e.X, e.Y), e.Name);
          _activeGraphics.Add(graphics);

          _clickEventOverlay.Graphics.Add(graphics.Item1);
          _clickEventOverlay.Graphics.Add(graphics.Item2);
        }
      }
    }

    private void LoadData_Click(object sender, RoutedEventArgs e)
    {
      LoadEvents();
    }

    // Map initialization logic is contained in MapViewModel.cs
  }
}
