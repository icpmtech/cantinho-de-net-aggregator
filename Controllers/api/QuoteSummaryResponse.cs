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

    [JsonPropertyName("address1")]
    public string Address1 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("zip")]
    public string Zip { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("website")]
    public string Website { get; set; }

    [JsonPropertyName("sector")]
    public string Sector { get; set; }

    [JsonPropertyName("longBusinessSummary")]
    public string LongBusinessSummary { get; set; }

    [JsonPropertyName("fullTimeEmployees")]
    public int FullTimeEmployees { get; set; }
    [JsonPropertyName("companyOfficers")]
    public List<CompanyOfficer> CompanyOfficers { get; set; }
  }
  public class CompanyOfficer
  {
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
  }
}
