namespace AspnetCoreMvcFull.Services
{
  using System.Net.Http;
  using System.Threading.Tasks;
  using AspnetCoreMvcFull.Models;
  using Microsoft.Extensions.Configuration;
  using Newtonsoft.Json.Linq;

  public partial class FinnhubService
  {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public FinnhubService(HttpClient httpClient, IConfiguration configuration)
    {
      _httpClient = httpClient;
      _apiKey = configuration["MarketRealTimeAPIs:Finnhub"];
    }

  

    public async Task<StockData> GetRealTimePriceAsync(string symbol)
    {
      string url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";

      var response = await _httpClient.GetAsync(url);
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      var json = JObject.Parse(content);

      var stockData = new StockData
      {
        CurrentPrice = decimal.TryParse(json["c"]?.ToString(), out decimal currentPrice) ? currentPrice : 0m,
        Change = decimal.TryParse(json["d"]?.ToString(), out decimal change) ? change : 0m,
        PercentChange = decimal.TryParse(json["dp"]?.ToString(), out decimal percentChange) ? percentChange : 0m,
        HighPrice = decimal.TryParse(json["h"]?.ToString(), out decimal highPrice) ? highPrice : 0m,
        LowPrice = decimal.TryParse(json["l"]?.ToString(), out decimal lowPrice) ? lowPrice : 0m,
        OpenPrice = decimal.TryParse(json["o"]?.ToString(), out decimal openPrice) ? openPrice : 0m,
        PreviousClosePrice = decimal.TryParse(json["pc"]?.ToString(), out decimal previousClosePrice) ? previousClosePrice : 0m,


      };

      return stockData;
    }
  }

}
