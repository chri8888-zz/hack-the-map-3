using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace festiflo_logistics_controller
{
  internal class RestRequest
  {
    private static string _cardiffServer = "";
    private static string _staffLayer = "";
    private static string _eventLayer = "";

    private HttpClient httpClient = null;

    public RestRequest()
    {
      httpClient = new HttpClient();
    }
  }
}
