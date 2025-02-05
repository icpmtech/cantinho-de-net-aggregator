namespace MarketAnalyticHub.Services
{
  using MarketAnalyticHub.Controllers.api;
  using System;
  using System.Collections.Generic;
  using System.Net.Http;
  using System.Text.Json;
  using System.Threading.Tasks;

  public class ApiDividend
  {
    public string Date { get; set; }
    public decimal Dividend { get; set; }
  }


  public class ApiDividendResponse
  {
    public List<ApiDividend> Dividends { get; set; }
    public string Symbol { get; set; }
  }



  public class DividendService
  {
    private readonly HttpClient _httpClient;

    // Constructor to inject HttpClient (recommended for reusability and testability)
    public DividendService(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches dividend data for a given stock symbol within a specified date range.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL").</param>
    /// <param name="startDate">Start date for fetching dividends.</param>
    /// <param name="endDate">End date for fetching dividends.</param>
    /// <returns>List of DividendScreenViewModel containing dividend details.</returns>
    public async Task<IEnumerable<DividendScreenViewModel>> GetDividendsAsync(string symbol, DateTime startDate, DateTime endDate)
    {
      // Initialize the list to hold dividend data
      List<DividendScreenViewModel> dividends = new List<DividendScreenViewModel>();

      try
      {
        // Construct the API URL with query parameters
        string apiUrl = $"http://apimarketsanalyticshub.azurewebsites.net/dividends?symbol={Uri.EscapeDataString(symbol)}&start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}";

        // Make the HTTP GET request
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        string jsonResponse = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON response into a list of ApiDividend objects
        var apiDividends = System.Text.Json.JsonSerializer.Deserialize<ApiDividendResponse>(jsonResponse, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        // Check if the deserialization was successful and the list is not null
        var apiResponse = JsonSerializer.Deserialize<ApiDividendResponse>(jsonResponse, new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        });

        // Check if the deserialization was successful and the list is not null
        if (apiResponse?.Dividends != null && apiResponse.Dividends.Count > 0)
        {
          foreach (var dividend in apiResponse.Dividends)
          {
            // Map ApiDividend to DividendScreenViewModel
            dividends.Add(new DividendScreenViewModel
            {
              Amount = dividend.Dividend.ToString("F2"), // Formats to two decimal places
              ExDate = dividend.Date // Assuming Date is in the desired string format
            });
          }
        }
      }
      catch (HttpRequestException httpEx)
      {
        // Handle HTTP-specific exceptions here (e.g., network errors, non-success status codes)
        Console.Error.WriteLine($"HTTP Request Error: {httpEx.Message}");
        // Optionally, log the error or rethrow the exception based on your application's needs
      }
      catch (JsonException jsonEx)
      {
        // Handle JSON parsing errors here
        Console.Error.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
        // Optionally, log the error or rethrow the exception based on your application's needs
      }
      catch (Exception ex)
      {
        // Handle any other exceptions that may occur
        Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
        // Optionally, log the error or rethrow the exception based on your application's needs
      }

      // Return the list of dividends (empty list if an error occurred or no data was found)
      return dividends;
    }
  }



}
