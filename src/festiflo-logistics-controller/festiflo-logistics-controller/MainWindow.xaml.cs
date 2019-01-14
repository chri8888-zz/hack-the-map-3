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

namespace festiflo_logistics_controller
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private MapView _mapView;
    private GraphicsOverlay _clickEventOverlay;
    private MapViewModel _mapVM;
    private Popup _testPopup;

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
        _mapVM = _mapView.DataContext as MapViewModel;

        _mapView.GeoViewTapped += MapView_GeoViewTapped;
      }
    }

    private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
    {
      
    }

    private void MapView_LeftMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (sender is MapView mapView)
      {
        var mousePos = e.GetPosition(mapView);
        var mapRelativeLoc = mapView.ScreenToLocation(mousePos);
        var markerSym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 12);
        var textSym = new TextSymbol("stuff",
                        System.Drawing.Color.Red,
                        12,
                        Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                        Esri.ArcGISRuntime.Symbology.VerticalAlignment.Bottom);

        var markerGraphic = new Graphic(mapRelativeLoc, markerSym);
        var textGraphic = new Graphic(mapRelativeLoc, textSym);
        if (_clickEventOverlay != null)
        {
          _clickEventOverlay.Graphics.Add(markerGraphic);
          _clickEventOverlay.Graphics.Add(textGraphic);
        }

      }
    }

    // Map initialization logic is contained in MapViewModel.cs
  }
}
