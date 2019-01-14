using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using HeatMapRendererJson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace festiflo_logistics_controller
{
  public class DataUtils
  {
    #region ColorStops
    public enum ColorPalletteType
    {
      Heat = 0,
      Blues = 1,
      Vivid  = 2
    }

    public static IList<(double ratio, Color color)> GetColorStops(ColorPalletteType pallette = ColorPalletteType.Heat)
    {
      var colorStops = new List<(double, Color)>(){
        (0.0, Colors.Transparent)
      };

      List<Color> colorList = null;

      if (pallette == ColorPalletteType.Heat)
        colorList = _pallette_Heat;
      if (pallette == ColorPalletteType.Blues)
        colorList = _pallette_Blues;
      if (pallette == ColorPalletteType.Vivid)
        colorList = _pallette_Vivid;

      for (int i = 0; i < colorList.Count ; ++i)
        colorStops.Add((_stopValues[i], colorList[i]));

      return colorStops;
    }

    private static readonly List<double> _stopValues = new List<double>()
    {
      0.05, 0.3, 0.4, 0.5, 0.7, 0.85, 1.0
    };

    private static readonly List<Color> _pallette_Heat = new List<Color>()
    {
      //Red/Orange/Yellow
      Color.FromRgb(122, 0, 45),
      Color.FromRgb(169, 62, 60),
      Color.FromRgb(207, 112, 71),
      Color.FromRgb(232, 155, 83),
      Color.FromRgb(243, 191, 94),
      Color.FromRgb(237, 217, 110), 
      Color.FromRgb(219, 226, 175),
    };

    private static readonly List<Color> _pallette_Vivid = new List<Color>()
    {
      //Red/Orange/Yellow
      Color.FromRgb(255, 0, 0),
      Color.FromRgb(255, 85, 0),
      Color.FromRgb(255, 170, 0),
      Color.FromRgb(255, 255, 0),
      Color.FromRgb(170, 255, 0),
      Color.FromRgb(85, 255, 0),
      Color.FromRgb(219, 255, 0),
    };

    private static readonly List<Color> _pallette_Blues = new List<Color>()
    {
      //DarkBlue/LightBlue
      Color.FromRgb(11, 50, 129),
      Color.FromRgb(28, 91, 166),
      Color.FromRgb(53, 126, 185),
      Color.FromRgb(90, 158, 204),
      Color.FromRgb(142, 190, 218),
      Color.FromRgb(186, 210, 235),
      Color.FromRgb(235, 240, 255),
    };
    #endregion

    public static async Task AddOperationalLayerAsync(Map map, string url, Renderer renderer = null)
    {
      var operationaLayer = await GetLayer(url, renderer);
      map.OperationalLayers.Add(operationaLayer);
    }

    public static async void AddOrReplaceOperationalLayerAsync(Map map, string url, Renderer renderer = null)
    {
      var heatMapRenderer = renderer.ToJson();
      var operationaLayer = await GetLayer(url, renderer);
      var layer = map.OperationalLayers.FirstOrDefault((x) => x.Name == operationaLayer.Name);
      if (layer != null)
      {
        var index = map.OperationalLayers.IndexOf(layer);
        map.OperationalLayers.Insert(index + 1, operationaLayer);
        Task.Delay(1000).ContinueWith(t => map.OperationalLayers.Remove(layer));
      }
      else
        map.OperationalLayers.Add(operationaLayer);
    }


    public static async Task<FeatureLayer> GetLayer(string url, Renderer renderer = null)
    {
      FeatureLayer _operationalLayer = null;
      try
      {
        _operationalLayer = new FeatureLayer(new Uri(url));
        await _operationalLayer.LoadAsync();
      }
      catch (Exception e)
      {
        Console.Write(e);
        return _operationalLayer;
      }

      // Apply the renderer to a point layer in the map.
      if (renderer != null)
        _operationalLayer.Renderer = renderer;

      return _operationalLayer;
    }

    public static async Task<ArcGISVectorTiledLayer> GetVectorTileLayer(string url)
    {
      ArcGISVectorTiledLayer _vectorTiledLayer = null;
      try
      {
        _vectorTiledLayer = new ArcGISVectorTiledLayer(new Uri(url));
        await _vectorTiledLayer.LoadAsync();
      }
      catch (Exception)
      {
      }

      return _vectorTiledLayer;
    }


    // Heatmap renderer credit: https://community.esri.com/thread/188830-heatmap-presenting-data-in-runtime-10000
    public static Renderer GetHeatmapRenderer(byte opacity = 255, long blurRadius = 14, long minPixelIntensity = 0, long maxPixelIntensity = 100, DataUtils.ColorPalletteType type = DataUtils.ColorPalletteType.Heat)
    {
      // Create a new HeatMapRenderer with info provided by the user.
      HeatMapRenderer heatMapRendererInfo = new HeatMapRenderer
      {
        BlurRadius = blurRadius,
        MinPixelIntensity = minPixelIntensity,
        MaxPixelIntensity = maxPixelIntensity,
      };

      // Add the chosen color stops (plus transparent for empty areas).
      var colorStops = GetColorStops(type); //defaultColorStops;
      foreach (var (ratio, color) in colorStops)
      {
        var colorWithAlpha = color;
        if (ratio != 0)
          colorWithAlpha.A = opacity;
        heatMapRendererInfo.AddColorStop(ratio, colorWithAlpha);
      }

      // Get the JSON representation of the renderer class.
      string heatMapJson = heatMapRendererInfo.ToJson();

      // Use the static Renderer.FromJson method to create a new renderer from the JSON string.
      return Renderer.FromJson(heatMapJson);
    }

    public static async Task<Esri.ArcGISRuntime.Geometry.Geometry> GetGeometry(string url, string whereClause = "objectid > 0")
    {
      var table = new ServiceFeatureTable(new Uri(url));
      await table.LoadAsync();
      if (table.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon)
        return null;

      var queryParams = new QueryParameters();
      queryParams.WhereClause = "objectid > 0";

      var count = await table.QueryFeatureCountAsync(queryParams);
      var queryFeatureResults = await table.QueryFeaturesAsync(queryParams);
      var resultingFeature = queryFeatureResults.FirstOrDefault();
      return resultingFeature.Geometry;
    }

    public static async Task<List<Esri.ArcGISRuntime.Geometry.Geometry>> GetGeometries(string url, string whereClause = "objectid > 0")
    {
      var geometries = new List<Esri.ArcGISRuntime.Geometry.Geometry>();

      try
      {
        var table = new ServiceFeatureTable(new Uri(url));
        await table.LoadAsync();

        var queryParams = new QueryParameters();
        queryParams.WhereClause = whereClause;
        var count = await table.QueryFeatureCountAsync(queryParams);
        var queryFeatureResults = await table.QueryFeaturesAsync(queryParams);

        foreach (var result in queryFeatureResults)
          geometries.Add(result.Geometry);
      }
      catch (Exception)
      {
      }

      return geometries;
    }


    public static async Task<object> GetAttribute(string url, string whereClause = "", string fieldName = "")
    {
      var table = new ServiceFeatureTable(new Uri(url));
      await table.LoadAsync();
      if (table.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon)
        return null;

      var queryParams = new QueryParameters();
      queryParams.WhereClause = whereClause;
      var count = await table.QueryFeatureCountAsync(queryParams);
      var queryFeatureResults = await table.QueryFeaturesAsync(queryParams);
      var resultingFeature = queryFeatureResults.FirstOrDefault();
      return resultingFeature.GetAttributeValue(fieldName);
    }

    public static async Task<List<object>> GetAttributes(string url, string whereClause = "", string[] fieldNames = null)
    {
      if (fieldNames == null)
        return null;

      var attributes = new List<object>();
      var table = new ServiceFeatureTable(new Uri(url));
      await table.LoadAsync();
      if (table.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon)
        return null;

      var queryParams = new QueryParameters();
      queryParams.WhereClause = whereClause;
      var count = await table.QueryFeatureCountAsync(queryParams);
      var queryFeatureResults = await table.QueryFeaturesAsync(queryParams);
      var resultingFeature = queryFeatureResults.FirstOrDefault();

      foreach (var fieldName in fieldNames)
        attributes.Add(resultingFeature.GetAttributeValue(fieldName));

      return attributes;
    }


    public static int GetContainedCount(List<Esri.ArcGISRuntime.Geometry.Geometry> countableGeometries, Esri.ArcGISRuntime.Geometry.Geometry containingPolygon, int bufferInMeters = 0)
    {
      var count = 0;
      if (countableGeometries == null)
        return count;

      var polyCorrected = GeometryEngine.RemoveZAndM(containingPolygon);
      polyCorrected = Esri.ArcGISRuntime.Geometry.Geometry.FromJson(polyCorrected.ToJson());

      polyCorrected = GeometryEngine.Buffer(polyCorrected, bufferInMeters);

      foreach (var geom in countableGeometries)
      {
        if (polyCorrected.SpatialReference != geom.SpatialReference)
          polyCorrected = GeometryEngine.Project(polyCorrected, geom.SpatialReference);

        var geomCorrected = GeometryEngine.RemoveZAndM(geom);
        geomCorrected = Esri.ArcGISRuntime.Geometry.Geometry.FromJson(geomCorrected.ToJson());

        var isContained = Esri.ArcGISRuntime.Geometry.GeometryEngine.Contains(polyCorrected, geomCorrected);
        count += isContained ? 1 : 0;
      }

      return count;
    }

  }
}
