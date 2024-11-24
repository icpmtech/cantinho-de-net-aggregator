using System.Text.Json.Serialization;

namespace MarketAnalyticHub.Services.ApiDataApp.Services
{
  public class PriceDetails
  {
    [JsonPropertyName("regularMarketPrice")]
    public double RegularMarketPrice { get; set; }

    [JsonPropertyName("regularMarketChange")]
    public double RegularMarketChange { get; set; }

    [JsonPropertyName("regularMarketChangePercent")]
    public double RegularMarketChangePercent { get; set; }

    [JsonPropertyName("regularMarketVolume")]
    public long RegularMarketVolume { get; set; }

    // Add other price-related properties as needed
  }
}
