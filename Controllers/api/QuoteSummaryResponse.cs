namespace MarketAnalyticHub.YaooServive.Models
{
  using System.Text.Json.Serialization;

  public class QuoteSummaryResponse
  {
    [JsonPropertyName("quoteSummary")]
    public QuoteSummary QuoteSummary { get; set; }
  }

  public class QuoteSummary
  {
    [JsonPropertyName("result")]
    public List<Result> Result { get; set; }

    [JsonPropertyName("error")]
    public object Error { get; set; }
  }

  public class Result
  {
    [JsonPropertyName("assetProfile")]
    public AssetProfile AssetProfile { get; set; }
  }

  public class AssetProfile
  {
    [JsonPropertyName("industry")]
    public string Industry { get; set; }

  }
}
