using MarketAnalyticHub.Controllers.api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Services
{
  // Models matching the JSON from /api/dividends
  public class ApiDividend
  {
    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
  }

  public class ApiDividendResponse
  {
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("dividends")]
    public List<ApiDividend> Dividends { get; set; }
  }

 

  public class DividendService
  {
    private readonly HttpClient _httpClient;

    public DividendService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches dividend data for a given stock symbol within a specified date range.
    /// </summary>
    public async Task<IEnumerable<DividendScreenViewModel>> GetDividendsAsync(
        string symbol,
        DateTime startDate,
        DateTime endDate)
    {
      var dividends = new List<DividendScreenViewModel>();

      try
      {
        // 1) Convert dates to UNIX timestamps
        long period1 = new DateTimeOffset(startDate).ToUnixTimeSeconds();
        long period2 = new DateTimeOffset(endDate).ToUnixTimeSeconds();

        // 2) Build URL with the correct query parameters
        var apiUrl = new UriBuilder("https://marketanaliticsapp.vercel.app/api/dividends");
        var qs = System.Web.HttpUtility.ParseQueryString(string.Empty);
        qs["symbol"] = symbol;
        qs["period1"] = period1.ToString();
        qs["period2"] = period2.ToString();
        qs["interval"] = "1d";
        apiUrl.Query = qs.ToString();

        // 3) Call the API
        using var response = await _httpClient.GetAsync(apiUrl.Uri);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // 4) Deserialize once
        var apiResponse = JsonSerializer.Deserialize<ApiDividendResponse>(
            json,
            new JsonSerializerOptions
            {
              PropertyNameCaseInsensitive = true
            }
        );

        // 5) Map to your ViewModel
        if (apiResponse?.Dividends != null)
        {
          foreach (var d in apiResponse.Dividends)
          {
            dividends.Add(new DividendScreenViewModel
            {
              ExDate = d.Date,
              Amount = d.Amount.ToString("F2")
            });
          }
        }
      }
      catch (HttpRequestException httpEx)
      {
        Console.Error.WriteLine($"HTTP Error: {httpEx.Message}");
      }
      catch (JsonException jsonEx)
      {
        Console.Error.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Unexpected Error: {ex.Message}");
      }

      return dividends;
    }
  }
}
