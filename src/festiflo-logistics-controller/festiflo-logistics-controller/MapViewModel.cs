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
using System.Collections.ObjectModel;

namespace festiflo_logistics_controller
{
  /// <summary>
  /// Provides map data to an application
  /// </summary>
  public class MapViewModel : INotifyPropertyChanged
  {
    private static string _userDataUrl = "https://cardiffportal.esri.com/server/rest/services/Hosted/FestiFlowUserService/FeatureServer/0";// "https://cardiffportal.esri.com/server/rest/services/Hosted/UserServiceWebMercator/FeatureServer/0";//"https://cardiffportal.esri.com/server/rest/services/Hosted/FestiFeatureService/FeatureServer/0";// "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    //    private static string _userDataUrl = "http://cardiffportal.esri.com/server/rest/services/Hosted/FestivalTestPolys/FeatureServer/0";
    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages_WebMercator/FeatureServer/3";// "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";
    private static string _toiletsURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/ToiletsWGS/FeatureServer/0";
    private static string _carParksURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/CarParks/FeatureServer/5";
    private static string _entrancesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/EntrancesStar/FeatureServer/1"; // "http://cardiffportal.esri.com/server/rest/services/Hosted/Entrances/FeatureServer/1";
    private static string _campsitesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Campsites/FeatureServer/3";

    private StafflocationsViewModel _stafflocationsViewModel = new StafflocationsViewModel();
    public StafflocationsViewModel StafflocationsViewModel
    {
      get { return _stafflocationsViewModel; }
    }
    public MapViewModel()
    {
      LoadHeatMap();
      updateStaffUserCounts();
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

    private string geomCount;

    public string GeomCount
    {
      get { return geomCount; }
      set { geomCount = value; }
    }

    private DataUtils.ColorPalletteType _heatMapColor = DataUtils.ColorPalletteType.Blues;
    public DataUtils.ColorPalletteType HeatMapColorPallette
    {
      get => _heatMapColor;
      set
      {
        _heatMapColor = value;
        OnPropertyChanged(nameof(HeatMapColorPallette));
      }
    }


    /// <summary>
    /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;


    public async void LoadHeatMap()
    {
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer(colorStops: DataUtils.GetColorStops(_heatMapColor)));
    }

    public async void ReloadLayers()
    {
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer(colorStops: DataUtils.GetColorStops(_heatMapColor)));
      await DataUtils.AddOperationalLayerAsync(_map, _campsitesURL);
      await DataUtils.AddOperationalLayerAsync(_map, _carParksURL);
      await DataUtils.AddOperationalLayerAsync(_map, _toiletsURL);
      await DataUtils.AddOperationalLayerAsync(_map, _entrancesURL);
      await DataUtils.AddOperationalLayerAsync(_map, _stagesURL);

      var vectorLayer = await DataUtils.GetVectorTileLayer("https://tiles.arcgis.com/tiles/cjTkfDK7oY4dk5Cd/arcgis/rest/services/Glasto/VectorTileServer");
      _map.OperationalLayers.Add(vectorLayer);
      updateStaffUserCounts();
    }

    public void loadfullMap()
    {
      _map.OperationalLayers = new LayerCollection();
      ReloadLayers();
      updateStaffUserCounts();
    }

    public void reloadHeatMap()
    {
      DataUtils.AddOrReplaceOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer());
      updateStaffUserCounts();
    }

    public async void updateStaffUserCounts()
    {
      var users = await DataUtils.GetGeometries(_userDataUrl, "oid >= 0");
      _stafflocationsViewModel.UpdateUserCounts(users);
      OnPropertyChanged(nameof(StafflocationsViewModel));
    }

    #region Commands
    private DelegateCommand reloadHeatMapCommand;
    public ICommand ReloadHeatMapCommand
    {
      get
      {
        if (reloadHeatMapCommand == null)
          reloadHeatMapCommand = new DelegateCommand(new Action(reloadHeatMap));
        return reloadHeatMapCommand;
      }
    }

    private DelegateCommand loadFullMapCommand;
    public ICommand LoadFullMapCommand
    {
      get
      {
        if (loadFullMapCommand == null)
          loadFullMapCommand = new DelegateCommand(new Action(loadfullMap));
        return loadFullMapCommand;
      }
    }

    private DelegateCommand moveStaffCommand;
    public ICommand MoveStaffCommand
    {
      get
      {
        if (moveStaffCommand == null)
          moveStaffCommand = new DelegateCommand(new Action(_stafflocationsViewModel.MoveStaff));
        return moveStaffCommand;
      }
    }
    #endregion


    #region Utils

    #endregion

  }

  public class StafflocationsViewModel : INotifyPropertyChanged
  {
    public StafflocationsViewModel()
    {
      InitializeAsync();
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

    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages_WebMercator/FeatureServer/3";// "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";
    private static string _carParksURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/CarParks/FeatureServer/5";
    private static string _entrancesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/EntrancesStar/FeatureServer/1"; // "http://cardiffportal.esri.com/server/rest/services/Hosted/Entrances/FeatureServer/1";
    private static string _campsitesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Campsites/FeatureServer/3";

    private int totalStaff = 40;
    private int freeStaff
    {
      get => totalStaff - (_locations.Sum(location => location.CurrentStaffing));
    }
    private ObservableCollection<Location> _locations = new ObservableCollection<Location>();
    public ObservableCollection<Location> Locations { get => _locations; }

    public void UpdateUserCounts(List<Esri.ArcGISRuntime.Geometry.Geometry> usersGeometries)
    {
      foreach (var loc in _locations)
      {
        loc.UserCount = DataUtils.GetContainedCount(usersGeometries, loc.Geometry, 150);
      }
      OnPropertyChanged(nameof(Locations));
    }


    private async void InitializeAsync()
    {
      ExtractInformation(_stagesURL, "objectid >= 0", LocationType.Stage);
      ExtractInformation(_entrancesURL, "objectid >= 0", LocationType.Entrance);
    }

    private async void ExtractInformation(string url, string whereClause, LocationType locationType)
    {
      try
      {
        var table = new ServiceFeatureTable(new Uri(url));
        await table.LoadAsync();

        var queryParams = new QueryParameters();
        queryParams.WhereClause = whereClause;
        var count = await table.QueryFeatureCountAsync(queryParams);
        var queryFeatureResults = await table.QueryFeaturesAsync(queryParams);

        foreach (var result in queryFeatureResults)
        {
          var loc = new Location(locationType, (freeStaff > 3) ? 3 : freeStaff, 2, result.GetAttributeValue("objectid").ToString(), result.Geometry);
          loc.Name = result.GetAttributeValue("Name").ToString();

          _locations.Add(loc);
        }
      }
      catch (Exception)
      {
      }
    }

    private ObservableCollection<string> staffMoveHistory = new ObservableCollection<string>();

    public ObservableCollection<string> StaffMoveHistory
    {
      get { return staffMoveHistory; }
      set { staffMoveHistory = value; OnPropertyChanged(nameof(StaffMoveHistory)); }
    }


    public void MoveStaff()
    {
      FreeStaff();
      foreach (var loc in Locations.Where(location => location.Understaffed).ToList())
      {
        var movingStaff = ((freeStaff > loc.StaffNeeded) ? loc.StaffNeeded : freeStaff);
        if (movingStaff > 0)
        {
          StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + movingStaff + " staff added.");
          loc.CurrentStaffing += movingStaff;
        }
        if (freeStaff == 0 && StaffMoveHistory.Count != 0 && !StaffMoveHistory[0].Contains("No more free staff"))
          StaffMoveHistory.Insert(0, DateTime.Now + " - No more free staff");
      }
    }

    private void FreeStaff()
    {
      foreach (var loc in Locations.Where(location => location.StaffNeeded < 0).ToList())
      {
        StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + Math.Abs(loc.StaffNeeded) + " staff removed.");
        loc.CurrentStaffing += loc.StaffNeeded;
      }
    }



    public enum LocationType
    {
      Stage,
      Carpark,
      Entrance,
      Campsite,
    }

    public class Location : INotifyPropertyChanged
    {
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

      public bool Understaffed { get => StaffNeeded > 0; }

      LocationType _locationType;
      int _currentStaffing;
      public int CurrentStaffing
      {
        get => _currentStaffing;
        set
        {
          _currentStaffing = value;
          OnPropertyChanged(nameof(CurrentStaffing));
          OnPropertyChanged(nameof(StaffNeeded));
          OnPropertyChanged(nameof(Understaffed));
        }
      }
      int _userCount;
      public int UserCount
      { get => _userCount;
        set
        {
          _userCount = value;
          OnPropertyChanged(nameof(UserCount));
          OnPropertyChanged(nameof(StaffNeeded));
          OnPropertyChanged(nameof(Understaffed));
        }
      }
      string _oid;
      string _name;
      public string Name { get => _name; set => _name = value; }

      Esri.ArcGISRuntime.Geometry.Geometry _geometry;
      public Esri.ArcGISRuntime.Geometry.Geometry Geometry { get => _geometry; set => _geometry = value; }

      public int StaffNeeded { get => (int)Math.Ceiling((_userCount / GetUserStaffRatio()) - _currentStaffing); }
      public Location(LocationType locationType, int currentStaffing, int userCount, string oid, Esri.ArcGISRuntime.Geometry.Geometry geometry)
      {
        _locationType = locationType;
        _currentStaffing = currentStaffing;
        _userCount = userCount;
        _oid = oid;
        _geometry = geometry;
      }

      private double GetUserStaffRatio()
      {
        switch (_locationType)
        {
          case LocationType.Stage:
            return 30;
          case LocationType.Carpark:
            return 50;
          case LocationType.Entrance:
            return 15;
          case LocationType.Campsite:
            return 100;
          default:
            return 0;
        }
      }
    }

  }
}
