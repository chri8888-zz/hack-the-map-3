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


namespace festiflo_logistics_controller
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void MapView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is MapViewModel mapView)
      {
        var map = mapView.Map;
        var mousePos = e.GetPosition(null);
        
      }
    }

    // Map initialization logic is contained in MapViewModel.cs
  }
}
