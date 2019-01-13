using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using HeatMapRendererJson;
using System.Windows.Media;
using System.Windows.Input;

namespace festiflo_logistics_controller
{
  /// <summary>
  /// Provides map data to an application
  /// </summary>
  public class MapViewModel : INotifyPropertyChanged
  {
    private static string _dataUrl = "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";

    public MapViewModel()
    {
      LoadHeatMap();
    }

    private Map _map = new Map(BasemapType.ImageryWithLabels, 51.154, -2.581, 16);

    /// <summary>
    /// Gets or sets the map
    /// </summary>
    public Map Map
    {
      get { return _map; }
      set { _map = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      var propertyChangedHandler = PropertyChanged;
      if (propertyChangedHandler != null)
        propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;


    public async void LoadHeatMap()
    {
      var dataLayer = await GetHeatMapLayer();
      _map.OperationalLayers.Add(dataLayer);
    }

    public async Task<FeatureLayer> GetHeatMapLayer()
    {
      FeatureLayer _dataLayer = new FeatureLayer(new Uri(_dataUrl));
      await _dataLayer.LoadAsync();

      // Create a new HeatMapRenderer with info provided by the user.
      HeatMapRenderer heatMapRendererInfo = new HeatMapRenderer
      {
        BlurRadius = 14,
        MinPixelIntensity = 0,
        MaxPixelIntensity = 100,
      };

      // Add the chosen color stops (plus transparent for empty areas).
      heatMapRendererInfo.AddColorStop(0.0, Colors.Transparent);
      heatMapRendererInfo.AddColorStop(0.10, Colors.Red);
      heatMapRendererInfo.AddColorStop(1.0, Colors.Yellow);

      // Get the JSON representation of the renderer class.
      string heatMapJson = heatMapRendererInfo.ToJson();

      // Use the static Renderer.FromJson method to create a new renderer from the JSON string.
      var heatMapRenderer = Renderer.FromJson(heatMapJson);

      // Apply the renderer to a point layer in the map.
      _dataLayer.Renderer = heatMapRenderer;

      return _dataLayer;
    }
    public void ReloadHeatMap()
    {
      _map.OperationalLayers = new LayerCollection();
      LoadHeatMap();
      Map = new Map(BasemapType.ImageryWithLabels, 51.154, -2.581, 16);
    }

    private DelegateCommand reloadHeatMapCommand;
    public ICommand ReloadHeatMapCommand
    {
      get
      {
        if (reloadHeatMapCommand == null)
          reloadHeatMapCommand = new DelegateCommand(new Action(ReloadHeatMap));
        return reloadHeatMapCommand;
      }
    }

  }
}
