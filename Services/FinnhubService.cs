namespace AspnetCoreMvcFull.Services
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json.Linq;

  public class FinnhubService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public FinnhubService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["MarketRealTimeAPIs:Finnhub"];
    }

    public async Task<decimal> GetRealTimePriceAsync(string symbol)
    {
      string url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";

      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(content);

      decimal latestPrice = json["c"].Value<decimal>();

      return latestPrice;
    }
  }
}
