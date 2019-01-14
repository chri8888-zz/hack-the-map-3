using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace festiflo_logistics_controller
{
  internal class StaffMember
  {
    public int OID { get; set; }
    public int ID { get; set; }
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public StafflocationsViewModel.LocationType Location { get; set; }

    public static StaffMember fromJSON(JObject json)
    {
      if (json == null)
        return null;

      var attributes = json.GetValue("attributes") as JObject;
      int? oid = (int?) attributes.GetValue("oid");
      if (oid == null)
        return null;

      int? id = (int?)attributes.GetValue("id");
      if (id == null)
        return null;

      string name = (string) attributes.GetValue("name");
      if (string.IsNullOrEmpty(name))
        return null;

      int? locationInt = (int?)attributes.GetValue("location");
      if (locationInt == null)
        return null;

      if (locationInt < 0 || locationInt > Enum.GetValues(typeof(StafflocationsViewModel.LocationType)).Cast<int>().Max())
        return null;

      StafflocationsViewModel.LocationType location = (StafflocationsViewModel.LocationType)locationInt;

      var staffMember = new StaffMember();

      var geometry = json.GetValue("geometry") as JObject;
      if (geometry == null)
        return null;

      double? x = (double?) geometry.GetValue("x");
      if (x == null)
        return null;

      double? y = (double?)geometry.GetValue("y");
      if (y == null)
        return null;

      return new StaffMember()
      {
        OID = oid.Value,
        ID = id.Value,
        Name = name,
        Location = location,
        X = x.Value,
        Y = y.Value,
      };
    }

    public string ToJSON()
    {
      var dictionary = new Dictionary<string, object>()
      {
        {
          "attributes", new Dictionary<string, object>()
          {
            { "oid", OID },
            { "id", ID },
            { "name", Name },
            { "location", Convert.ToInt32(Location) }
          }
        },
        {
          "geometry", new Dictionary<string, object>()
          {
            { "x", X },
            { "y", Y },
          }
        }
      };

      return JsonConvert.SerializeObject(dictionary);
    }
  }

  internal class EventData
  {
    public int OID { get; set; }
    public int ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public static EventData fromJSON(JObject json)
    {
      if (json == null)
        return null;

      var attributes = json.GetValue("attributes") as JObject;
      int? oid = (int?)attributes.GetValue("oid");
      if (oid == null)
        return null;

      int? id = (int?)attributes.GetValue("id");
      if (id == null)
        return null;

      string name = (string)attributes.GetValue("name");
      if (string.IsNullOrEmpty(name))
        return null;

      string description = (string)attributes.GetValue("description");

      var staffMember = new EventData();

      var geometry = json.GetValue("geometry") as JObject;
      if (geometry == null)
        return null;

      double? x = (double?)geometry.GetValue("x");
      if (x == null)
        return null;

      double? y = (double?)geometry.GetValue("y");
      if (y == null)
        return null;

      return new EventData()
      {
        OID = oid.Value,
        ID = id.Value,
        Name = name,
        Description = description,
        X = x.Value,
        Y = y.Value,
      };
    }

    public string ToJSON()
    {
      var dictionary = new Dictionary<string, object>()
      {
        {
          "attributes", new Dictionary<string, object>()
          {
            { "oid", OID },
            { "id", ID },
            { "name", Name },
            { "description", Description }
          }
        },
        {
          "geometry", new Dictionary<string, object>()
          {
            { "x", X },
            { "y", Y },
          }
        }
      };

      return JsonConvert.SerializeObject(dictionary);
    }
  }

  internal class RestRequest
  {
    private static string _cardiffServer = "https://cardiffportal.esri.com";
    private static string _staffLayer = "/server/rest/services/Hosted/StaffServiceWebMercator/FeatureServer/0/";
    private static string _eventLayer = "/server/rest/services/Hosted/EventServiceWebMercator/FeatureServer/0/";

    private HttpClient httpClient = null;

    private async Task<string> GetToken()
    {
      var loginFile = "login.txt";
      if (!File.Exists(loginFile))
      {
        System.Windows.MessageBox.Show("Failed to get login details, please add login.txt to your build path");
        return "";
      }

      string[] lines = File.ReadAllLines(loginFile);
      if (lines == null || lines.Length != 2)
        return "";

      var postParams = new Dictionary<string, string>()
      {
        { "username", lines[0] },
        { "password", lines[1] },
        { "client", "referer" },
        { "referer", "https://cardiffportal.esri.com:6443/arcgis/admin" },
        { "f", "pjson" },
        { "expiration", "10000" },
      };

      var content = new FormUrlEncodedContent(postParams);
      var result = await httpClient.PostAsync(_cardiffServer + "/portal/sharing/rest/generateToken", content);
      if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Content == null)
        return "";

      string json = await result.Content.ReadAsStringAsync();
      if (string.IsNullOrEmpty(json))
        return "";

      var tokenResult = JObject.Parse(json);
      return tokenResult.GetValue("token")?.ToString();
    }

    public async Task<List<StaffMember>> GetStaff()
    {
      var staffList = new List<StaffMember>();

      var token = await GetToken();
      if (string.IsNullOrEmpty(token))
        return staffList;

      var postParams = new Dictionary<string, string>()
      {
        { "token", token },
        { "f", "pjson" },
        { "where", "oid > 0" },
        { "outfields", "*" },
      };

      var content = new FormUrlEncodedContent(postParams);
      var result = await httpClient.PostAsync(_cardiffServer + _staffLayer + "query", content);
      if (result.StatusCode != System.Net.HttpStatusCode.OK || result.Content == null)
        return staffList;

      string json = await result.Content.ReadAsStringAsync();
      if (string.IsNullOrEmpty(json))
        return staffList;

      var tokenResult = JObject.Parse(json);
      if (tokenResult == null)
        return staffList;

      var users = tokenResult.GetValue("features") as JArray;
      if (users == null || users.Count == 0)
        return staffList;
      
      foreach (var user in users)
      {
        var staffMember = StaffMember.fromJSON(user as JObject);
        if (staffMember == null)
          continue;

        staffList.Add(staffMember);
      }
      
      return staffList;
    }

    public async Task CreateStaffMembers(List<StaffMember> members)
    {
      if (members == null)
        return;

      StringBuilder sb = new StringBuilder();
      sb.Append("[");
      foreach (var member in members)
        sb.Append(member.ToJSON());
      sb.Append("]");

      var token = await GetToken();
      if (string.IsNullOrEmpty(token))
        return;

      var postParams = new Dictionary<string, string>()
      {
        { "token", token },
        { "f", "pjson" },
        { "features", sb.ToString() }
      };

      var content = new FormUrlEncodedContent(postParams);
      await httpClient.PostAsync(_cardiffServer + _staffLayer + "addFeatures", content);
    }

    public RestRequest()
    {
      httpClient = new HttpClient();
    }


  }
}
