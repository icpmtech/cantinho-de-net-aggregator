
namespace MarketAnalyticHub.Services
{
  using System;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Text.Json;
  using System.Threading.Tasks;

  namespace ApiDataApp.Services
  {
    public class ApiService
    {
      private readonly HttpClient _httpClient;
      private const string ApiKey = "O831EHVCJBFEHUKE"; // Replace with your actual API key

      public ApiService(HttpClient httpClient)
      {
        _httpClient = httpClient;
      }

      public async Task<dynamic> GetApiDataAsync(string keyword)
      {
        var queryUrl = $"https://www.alphavantage.co/query?function=SYMBOL_SEARCH&keywords={keyword}&apikey={ApiKey}";
        var response = await _httpClient.GetStringAsync(queryUrl);
        var jsonData = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(response);
        return jsonData;
      }
    }
  }

}
