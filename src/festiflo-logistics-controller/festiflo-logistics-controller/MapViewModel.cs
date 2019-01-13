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
//    private static string _userDataUrl = "https://cardiffportal.esri.com/server/rest/services/Hosted/FestiFeatureService/FeatureServer/0";// "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    private static string _userDataUrl = "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";

    public MapViewModel()
    {
      LoadHeatMap();
    }

    private Map _map = new Map(BasemapType.ImageryWithLabelsVector, 51.155, -2.584, 16);

    /// <summary>
    /// Gets or sets the map
    /// </summary>
    public Map Map
    {
      get { return _map; }
      set { _map = value; OnPropertyChanged(); }
    }

    private Esri.ArcGISRuntime.Geometry.Geometry _geometry;

    public Esri.ArcGISRuntime.Geometry.Geometry JohnPeelGeometry
    {
      get { return _geometry;; }
      set
      {
        _geometry = value;
        OnPropertyChanged(nameof(GeometryString));
      }
    }

    public string GeometryString { get => _geometry?.ToJson(); }


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
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer());
    }

    public async void ReloadLayers()
    {
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer());
      await DataUtils.AddOperationalLayerAsync(_map, _stagesURL);
      JohnPeelGeometry = await DataUtils.GetGeometry(_stagesURL);
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

    #endregion

  }
}
