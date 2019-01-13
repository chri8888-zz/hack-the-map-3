using Esri.ArcGISRuntime.Data;
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
    #region ColorStops
    internal enum ColorPalletteType
    {
      Heat = 0,
      Blues = 1
    }

    public static IList<(double ratio, Color color)> GetColorStops(ColorPalletteType pallette = ColorPalletteType.Heat)
    {
      var colorStops = new List<(double, Color)>(){
        (0.0, Colors.Transparent)
      };

      List<Color> colorList = (pallette == ColorPalletteType.Heat) ? _pallette_Heat : _pallette_Blues;
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

    private static readonly IList<(double ratio, Color color)> defaultColorStops = new ReadOnlyCollection<(double, Color)>
      (new List<(double, Color)> {
        { (0.0, Colors.Transparent) },
        { (0.02, Color.FromArgb(51, 238, 17, 17)) },
        { (0.02, Color.FromArgb(51, 238, 17, 17)) },
        { (0.02, Color.FromArgb(51, 238, 17, 17)) },
        { (0.02, Color.FromArgb(51, 238, 17, 17)) },
        { (0.02, Color.FromArgb(51, 238, 62, 17)) },
        { (0.02, Color.FromArgb(51, 122, 0, 45)) },
        //{ (0.1, Color.FromArgb(255, 238, 17, 17)) },
        //{ (1.0, Color.FromArgb(255, 238, 17, 17)) }
      });
    #endregion

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
        BlurRadius = blurRadius,
        MinPixelIntensity = minPixelIntensity,
        MaxPixelIntensity = maxPixelIntensity,
      };

      // Add the chosen color stops (plus transparent for empty areas).
      colorStops = colorStops ?? GetColorStops(); //defaultColorStops;
      foreach (var (ratio, color) in colorStops)
      {
        heatMapRendererInfo.AddColorStop(ratio, color);
      }


      // Get the JSON representation of the renderer class.
      string heatMapJson = heatMapRendererInfo.ToJson();

      // Use the static Renderer.FromJson method to create a new renderer from the JSON string.
      return Renderer.FromJson(heatMapJson);
    }

    public static async Task<Esri.ArcGISRuntime.Geometry.Geometry> GetGeometry(string url)
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
  }
}
