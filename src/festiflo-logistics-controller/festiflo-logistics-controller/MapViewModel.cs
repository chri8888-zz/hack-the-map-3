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
    private static string _userDataUrl = "https://cardiffportal.esri.com/server/rest/services/Hosted/FestiFeatureService/FeatureServer/0";// "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    //    private static string _userDataUrl = "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages_WebMercator/FeatureServer/3";// "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";
    private static string _toiletsURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/ToiletsWGS/FeatureServer/0";
    private static string _carParksURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/CarParks/FeatureServer/5";
    private static string _entrancesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/EntrancesStar/FeatureServer/1"; // "http://cardiffportal.esri.com/server/rest/services/Hosted/Entrances/FeatureServer/1";
    private static string _campsitesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Campsites/FeatureServer/3";

    public MapViewModel()
    {
      LoadHeatMap();
    }

    private Map _map = new Map(BasemapType.ImageryWithLabelsVector, 51.155, -2.585, 14);

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

    private string geomCount;

    public string GeomCount
    {
      get { return geomCount; }
      set { geomCount = value; }
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
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer());
    }

    public async void ReloadLayers()
    {
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer());
      await DataUtils.AddOperationalLayerAsync(_map, _campsitesURL);
      await DataUtils.AddOperationalLayerAsync(_map, _carParksURL);
      await DataUtils.AddOperationalLayerAsync(_map, _toiletsURL);
      await DataUtils.AddOperationalLayerAsync(_map, _entrancesURL);
      await DataUtils.AddOperationalLayerAsync(_map, _stagesURL);

      JohnPeelGeometry = await DataUtils.GetGeometry(_stagesURL);

      var users = await DataUtils.GetGeometries(_userDataUrl, "oid > 0");

      GeomCount = DataUtils.GetContainedCount(users, JohnPeelGeometry).ToString();
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
