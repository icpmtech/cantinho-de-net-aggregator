namespace MarketAnalyticHub.Services
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json.Linq;

  public class AlphaVantageService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AlphaVantageService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["ApiKeys:AlphaVantage"];
    }

    public async Task<decimal> GetRealTimePriceAsync(string symbol)
    {
      string url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";

      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(content);

      decimal latestPrice = json["Global Quote"]["05. price"].Value<decimal>();

      return latestPrice;
    }

  }





}
