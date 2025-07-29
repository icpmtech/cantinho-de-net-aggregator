namespace MarketAnalyticHub.Controllers.Api
{
  using System.Text.Json.Serialization;

  public class RaciusHit
  {
   
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("fiscal_id")]
    public string FiscalId { get; set; } = default!;

    public string Name { get; set; } = default!;

    [JsonPropertyName("name_short")]
    public string NameShort { get; set; } = default!;

    public string Url { get; set; } = default!;

    public string Description { get; set; } = default!;

    public int Type { get; set; }

    public int[] Years { get; set; } = Array.Empty<int>();

    public Geo Geo { get; set; } = new();
  }

  public class Geo
  {
    public string Address { get; set; } = default!;

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; } = default!;

    public string Location { get; set; } = default!;
  }

}
