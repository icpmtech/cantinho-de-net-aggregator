namespace MarketAnalyticHub.Services
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json.Linq;

  public class BarchartService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public BarchartService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["ApiKeys:Barchart"];
    }

    public async Task<decimal> GetRealTimePriceAsync(string symbol)
    {
      string url = $"https://marketdata.websol.barchart.com/getQuote.json?apikey={_apiKey}&symbols={symbol}";

      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(content);

      decimal latestPrice = json["results"][0]["lastPrice"].Value<decimal>();

      return latestPrice;
    }
  }
}
