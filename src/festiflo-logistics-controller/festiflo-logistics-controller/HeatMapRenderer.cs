// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Media;

// From https://community.esri.com/community/developers/native-app-developers/arcgis-runtime-sdk-for-net/blog/2018/03/29/heat-map-renderer-map-it-like-its-hot

namespace HeatMapRendererJson
{
  /// <summary>
  /// A JSON-serializable class that defines heat renderer information to apply to a feature layer.
  /// </summary>
  public partial class HeatMapRenderer
  {
    /// <summary>
    /// Serialize the heat map renderer to a Json string.
    /// </summary>
    /// <returns>Json-formatted string.</returns>
    public string ToJson()
    {
      return JsonConvert.SerializeObject(this);
    }

    /// <summary>
    /// Add a color stop to the renderer.
    /// </summary>
    /// <param name="ratio">Position of the color in the renderer from 0 (start) to 1 (end).</param>
    /// <param name="color">Color for the stop.</param>
    public void AddColorStop(double ratio, Color color)
    {
      if (ratio > 1.0 || ratio < 0.0) { throw new Exception("Argument 'ratio' must be a value between 0 and 1."); };

      ColorStop stop = new ColorStop(ratio, color);
      ColorStops.Add(stop);
    }

    /// <summary>
    /// Remove all color stops from the renderer.
    /// </summary>
    public void ClearColorStops()
    {
      ColorStops.Clear();
    }

    /// <summary>
    /// Specifies the type of renderer used (must be 'heatmap').
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = "heatmap";

    /// <summary>
    /// Represents the radius (in pixels) of the circle over which the majority of each point's value is spread.
    /// </summary>
    [JsonProperty("blurRadius")]
    public long BlurRadius { get; set; }

    /// <summary>
    /// Represents the list of colors displayed in the heat map color ramp.
    /// </summary>
    [JsonProperty("colorStops")]
    public List<ColorStop> ColorStops { get; set; } = new List<ColorStop>();

    /// <summary>
    /// Each feature gets the same value/importance/weight or with a field where each feature is weighted by the field's value.
    /// (Optional - the renderer can be created if no field is specified).
    /// </summary>
    [JsonProperty("field")]
    public string Field { get; set; }

    /// <summary>
    /// Pixel intensity value which is assigned the final color in the color ramp.
    /// </summary>
    [JsonProperty("maxPixelIntensity")]
    public double MaxPixelIntensity { get; set; }

    /// <summary>
    /// Pixel intensity value which is assigned the initial color in the color ramp.
    /// </summary>
    [JsonProperty("minPixelIntensity")]
    public double MinPixelIntensity { get; set; }
  }

  /// <summary>
  /// A class that defines a color stop used in a heat map renderer color ramp.
  /// </summary>
  public partial class ColorStop
  {
    /// <summary>
    /// The position along the color ramp where this color begins (from 0=start, to 1=end).
    /// </summary>
    [JsonProperty("ratio")]
    public double Ratio { get; set; }

    /// <summary>
    /// Color to use at this stop on the ramp. This is a four value array: R,G,B,A
    /// </summary>
    [JsonProperty("color")]
    public int[] Color { get; set; }

    /// <summary>
    /// A class that defines a color and a position along the color ramp.
    /// </summary>
    public ColorStop(double ratio, Color color)
    {
      Ratio = ratio;
      Color = new int[] { color.R, color.G, color.B, color.A };
    }
  }
}