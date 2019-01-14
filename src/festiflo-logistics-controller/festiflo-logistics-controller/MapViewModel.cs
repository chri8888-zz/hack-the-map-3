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
using System.Threading;
using System.Diagnostics;

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

    private EventsManagerViewModel _eventManagerVM = new EventsManagerViewModel();
    public EventsManagerViewModel EventManagerViewModel
    {
      get => _eventManagerVM;
    }

    public MapViewModel()
    {
      LoadHeatMap();
      updateStaffUserCounts();

      Thread mapRefresher = new Thread(new ThreadStart(MapRefreshTimer));
      mapRefresher.Start();
    }

    private bool _autoRefresh = false;

    public bool AutoRefresh
    {
      get { return _autoRefresh; }
      set { _autoRefresh = value; }
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

    void MapRefreshTimer()
    {
      while (true)
      {
        if (AutoRefresh)
          reloadHeatMap();
        Thread.Sleep(1000 * 5); // 5 Minutes
                                 //Have a break condition
      }
    }

    private string geomCount;

    public string GeomCount
    {
      get { return geomCount; }
      set { geomCount = value; }
    }

    private DataUtils.ColorPalletteType _heatMapColor = DataUtils.ColorPalletteType.Heat;
    public DataUtils.ColorPalletteType HeatMapColorPallette
    {
      get => _heatMapColor;
      set
      {
        _heatMapColor = value;
        OnPropertyChanged(nameof(HeatMapColorPallette));
        reloadHeatMap();
      }
    }

    private byte _opacity = 255;
    public byte HeatMapOpacity
    {
      get => _opacity;
      set => _opacity = value;
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
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer(opacity: HeatMapOpacity, type:_heatMapColor));
    }

    public async void ReloadLayers()
    {
      await DataUtils.AddOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer(opacity: HeatMapOpacity, type:_heatMapColor));
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
      DataUtils.AddOrReplaceOperationalLayerAsync(_map, _userDataUrl, DataUtils.GetHeatmapRenderer(opacity: HeatMapOpacity, type: _heatMapColor));
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
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private static string _stagesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages_WebMercator/FeatureServer/3";// "http://cardiffportal.esri.com/server/rest/services/Hosted/Stages/FeatureServer/3";
    private static string _carParksURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/CarParks/FeatureServer/5";
    private static string _entrancesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/EntrancesStar/FeatureServer/1"; // "http://cardiffportal.esri.com/server/rest/services/Hosted/Entrances/FeatureServer/1";
    private static string _campsitesURL = "http://cardiffportal.esri.com/server/rest/services/Hosted/Campsites/FeatureServer/3";

    private int totalStaff = 45;
    private int freeStaff
    {
      get => _locations.Where(location => location.LocationType == LocationType.StaffRoom).Sum(location => location.CurrentStaffing);
    }

    private int workingStaffCount
    {
      get => (_locations.Sum(location => location.CurrentStaffing));
    }

    private ObservableCollection<Location> _locations = new ObservableCollection<Location>();
    public ObservableCollection<Location> Locations { get => _locations; }

    private ObservableCollection<StaffMember> _staffMembers = new ObservableCollection<StaffMember>();

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
      GetFreeStaff();
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
          var loc = new Location(locationType, 0, 2, result.GetAttributeValue("objectid").ToString(), result.Geometry);
          loc.Name = result.GetAttributeValue("Name").ToString();

          _locations.Add(loc);
        }
      }
      catch (Exception)
      {
      }
    }

    private void GetFreeStaff()
    {
      var geom = new Esri.ArcGISRuntime.Geometry.MapPoint(0, 0, SpatialReferences.WebMercator);
      var loc = new Location(LocationType.StaffRoom, totalStaff - workingStaffCount, 0, "n/a", geom);
      loc.Name = "Staff Room";

      _locations.Add(loc);
    }

    private ObservableCollection<string> staffMoveHistory = new ObservableCollection<string>();

    public ObservableCollection<string> StaffMoveHistory
    {
      get { return staffMoveHistory; }
      set { staffMoveHistory = value; OnPropertyChanged(nameof(StaffMoveHistory)); }
    }


    public void MoveStaff()
    {
      var staffRoom = Locations.Where(location => location.LocationType == LocationType.StaffRoom).FirstOrDefault();

      foreach (var loc in Locations.Where(location => location.Understaffed).ToList().OrderBy(loc => loc.StaffNeeded / (loc.CurrentStaffing == 0 ? 0.00001 : loc.CurrentStaffing)))
      {
        var movingStaff = ((freeStaff > loc.StaffNeeded) ? loc.StaffNeeded : freeStaff);
        if (movingStaff > 0)
        {
          StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + movingStaff + " staff from staff room.");
          loc.CurrentStaffing += movingStaff;
          staffRoom.CurrentStaffing -= movingStaff;
          //Debug.Assert(staffRoom.CurrentStaffing == staffRoom.StaffMembers.Count());
          //Debug.Assert(loc.CurrentStaffing == loc.StaffMembers.Count());
        }
        else if (loc.CurrentStaffing == 0)
        {
          var bestRatio = Locations.Select(x => x.CurrentStaffing / (x.StaffNeeded == 0 ? 1000 : x.StaffNeeded)).Min();
          var bestLoc = Locations.Where(x => x.CurrentStaffing / (x.StaffNeeded == 0 ? 1000 : x.StaffNeeded) == bestRatio).FirstOrDefault();
          StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + Math.Abs(loc.StaffNeeded) + " staff removed.");
          bestLoc.CurrentStaffing--;
          StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + 1 + " staff added.");
          loc.CurrentStaffing++;
          //Debug.Assert(bestLoc.CurrentStaffing == bestLoc.StaffMembers.Count());
          //Debug.Assert(loc.CurrentStaffing == loc.StaffMembers.Count());
        }
        if (freeStaff == 0 && StaffMoveHistory.Count != 0 && !StaffMoveHistory[0].Contains("No more free staff"))
          StaffMoveHistory.Insert(0, DateTime.Now + " - No more free staff");
      }
    }

    private void FreeStaff()
    {
      foreach (var loc in Locations.Where(location => location.StaffNeeded < 0).ToList())
      {
        StaffMoveHistory.Insert(0, DateTime.Now + " - " + loc.Name + ": " + Math.Abs(loc.StaffNeeded) + " staff now on break.");
        loc.CurrentStaffing += loc.StaffNeeded;
        var staffRoom = Locations.Where(location => location.LocationType == LocationType.StaffRoom).FirstOrDefault();
        staffRoom.CurrentStaffing -= loc.StaffNeeded;

        //Debug.Assert(staffRoom.CurrentStaffing == staffRoom.StaffMembers.Count());
        //Debug.Assert(loc.CurrentStaffing == loc.StaffMembers.Count());
      }
    }

    public enum LocationType
    {
      Stage,
      Carpark,
      Entrance,
      Campsite,
      StaffRoom,
    }

    public class Location : INotifyPropertyChanged
    {
      /// <summary>
      /// Raises the <see cref="MapViewModel.PropertyChanged" /> event
      /// </summary>
      /// <param name="propertyName">The name of the property that has changed</param>
      protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }

      public event PropertyChangedEventHandler PropertyChanged;

      public bool Understaffed { get => StaffNeeded > 0; }
      public bool UrgentStaffRequired { get => StaffNeeded > 0 && CurrentStaffing < 1; }

      private LocationType _locationType;

      public LocationType LocationType
      {
        get { return _locationType; }
        set { _locationType = value; }
      }

      int _currentStaffing;
      public int CurrentStaffing
      {
        get => _currentStaffing;
        set
        {
          if (value < 0)
            return;
          _currentStaffing = value;
          OnPropertyChanged(nameof(CurrentStaffing));
          OnPropertyChanged(nameof(StaffNeeded));
          OnPropertyChanged(nameof(Understaffed));
        }
      }

      private List<StaffMember> _staffMembers;
      internal List<StaffMember> StaffMembers
      {
        get { return _staffMembers; }
        set { _staffMembers = value; }
      }

      int _userCount = 0;
      public int UserCount
      {
        get => _userCount;
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

      public int StaffNeeded { get => _locationType == LocationType.StaffRoom? 0 : (int)Math.Ceiling((_userCount / GetUserStaffRatio()) - _currentStaffing); }
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

  public class EventsManagerViewModel : INotifyPropertyChanged
  {
    public EventsManagerViewModel(){ }

    /// <summary>
    /// Raises the <see cref="EventsManagerViewModel.PropertyChanged" /> event
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed</param>
    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public enum EventType
    {
      Information = 0,
      Warning = 1,
      Closure = 2
    }

    #region Event Props

    private int _nextID = 0;

    public List<EventData> ActiveEvents = new List<EventData>();

    private string _eventTitle = "";
    public string EventTitle
    {
      get => _eventTitle;
      set
      {
        _eventTitle = value;
        NotifyPropertyChanged(nameof(EventTitle));
        NotifyPropertyChanged(nameof(CanSendEvent));
      }
    }

    private string _eventDesc = "";
    public string EventDescription
    {
      get => _eventDesc;
      set
      {
        _eventDesc = value;
        NotifyPropertyChanged(nameof(EventDescription));
        NotifyPropertyChanged(nameof(CanSendEvent));
      }
    }

    private MapPoint _eventLocation = null;
    public MapPoint EventLocation
    {
      get => _eventLocation;
      set
      {
        _eventLocation = value;
        NotifyPropertyChanged(nameof(EventLocation));
        NotifyPropertyChanged(nameof(CanSendEvent));
      }
    }

    private EventType _eventType = EventType.Information;
    public EventType SelectedEventType
    {
      get => _eventType;
      set
      {
        _eventType = value;
        NotifyPropertyChanged(nameof(SelectedEventType));
        NotifyPropertyChanged(nameof(CanSendEvent));
      }
    }

    public bool CanSendEvent
    {
      get
      {
        if (_eventTitle != "" && _eventDesc != "" && _eventLocation != null)
          return true;
        return false;
      }
    }

    // Event timer
    // Event symbol
    #endregion

    #region commands
    private DelegateCommand _sendEventCommand;
    public ICommand SendEventCmd
    {
      get
      {
        if (_sendEventCommand == null)
          _sendEventCommand = new DelegateCommand(new Action(SendEvent));
        return _sendEventCommand;
      }
    }

    #endregion
    private async void SendEvent()
    {
      var newEvent = new EventData() { ID = _nextID++, Name = _eventTitle, Description = _eventDesc, X = _eventLocation.X, Y = _eventLocation.Y, EventType = _eventType };

      var rreq = new RestRequest();
      await rreq.CreateEvent(newEvent);

      ActiveEvents = await rreq.QueryEvents();

      EventTitle = "";
      EventDescription = "";
      EventLocation = null;
    }

    public async Task PollEvents()
    {
      var rreq = new RestRequest();
      var eventList = await rreq.QueryEvents();
      if (eventList != null)
        ActiveEvents = eventList;
    }
  }
}
