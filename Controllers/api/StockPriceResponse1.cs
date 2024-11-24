using MarketAnalyticHub.Services.ApiDataApp.Services;
using System.Text.Json.Serialization;

namespace MarketAnalyticHub.Services
{
  public class StockPriceResponse
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("price")]
    public PriceDetails Price { get; set; }
  }
}
