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
    private static string _userDataUrl = "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";

    public MapViewModel()
    {
      LoadHeatMap();
    }

    private Map _map = new Map(BasemapType.ImageryWithLabels, 51.155, -2.584, 16);

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
      await AddOperationalLayerAsync(_map, _userDataUrl, GetHeatmapRenderer());
    }

    public async void ReloadLayers()
    {
      await AddOperationalLayerAsync(_map, _userDataUrl, GetHeatmapRenderer());
      await AddOperationalLayerAsync(_map, _stagesURL);
    }

    public void ReloadHeatMap()
    {
      _map.OperationalLayers = new LayerCollection();
      ReloadLayers();
    }

    #region Commands
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
    #endregion


    #region Utils
    private async Task AddOperationalLayerAsync(Map map, string url, Renderer renderer = null)
    {
      var operationaLayer = await GetLayer(url, renderer);
      map.OperationalLayers.Add(operationaLayer);
    }

    private async Task<FeatureLayer> GetLayer(string url, Renderer renderer = null)
    {
      FeatureLayer _opertionalLayer = new FeatureLayer(new Uri(url));
      await _opertionalLayer.LoadAsync();

      // Apply the renderer to a point layer in the map.
      if (renderer != null)
        _opertionalLayer.Renderer = renderer;

      return _opertionalLayer;
    }

    private Renderer GetHeatmapRenderer()
    {
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
      return Renderer.FromJson(heatMapJson);
    }
    #endregion

  }
}
