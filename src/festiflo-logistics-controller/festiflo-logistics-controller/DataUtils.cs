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
  class DataUtils
  {
    private static readonly IList<(double ratio, Color color)> defaultColorStops = new ReadOnlyCollection<(double, Color)>
      (new List<(double, Color)> {
        { (0.0, Colors.Transparent) },
        { (0.10, Colors.Red) },
        { (1.0, Colors.Yellow) }});

    public static async Task AddOperationalLayerAsync(Map map, string url, Renderer renderer = null)
    {
      var operationaLayer = await GetLayer(url, renderer);
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
      catch (Exception)
      {
        return _operationalLayer;
      }

      // Apply the renderer to a point layer in the map.
      if (renderer != null)
        _operationalLayer.Renderer = renderer;

      return _operationalLayer;
    }

    public static Renderer GetHeatmapRenderer(long blurRadius = 14, long minPixelIntensity = 0, long maxPixelIntensity = 100, IList<(double ratio, Color color)> colorStops = null)
    {
      // Create a new HeatMapRenderer with info provided by the user.
      HeatMapRenderer heatMapRendererInfo = new HeatMapRenderer
      {
        BlurRadius = 14,
        MinPixelIntensity = 0,
        MaxPixelIntensity = 100,
      };

      // Add the chosen color stops (plus transparent for empty areas).
      colorStops = colorStops ?? defaultColorStops;
      foreach (var ColorStop in colorStops)
      {
        heatMapRendererInfo.AddColorStop(ColorStop.ratio, ColorStop.color);
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

    public static int GetContainedCount(List<Esri.ArcGISRuntime.Geometry.Geometry> countableGeometries, Esri.ArcGISRuntime.Geometry.Geometry containingPolygon)
    {
      var count = 0;
      if (countableGeometries == null || containingPolygon.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon)
        return count;

      var polyCorrected = GeometryEngine.RemoveZAndM(containingPolygon);
      polyCorrected = Esri.ArcGISRuntime.Geometry.Geometry.FromJson(polyCorrected.ToJson());

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
