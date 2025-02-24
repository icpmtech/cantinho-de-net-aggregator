using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MarketAnalyticHub.Controllers.api
{
  [ApiController]
  [Route("api/sec")]
  public partial class SecFilingsController : ControllerBase
  {
    private readonly HttpClient _httpClient;

    public SecFilingsController(IHttpClientFactory httpClientFactory)
    {
      _httpClient = httpClientFactory.CreateClient();
      _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("marketanalyticshub.app/1.0 (mourao.martins@gmail.com)");
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetFilingsBySymbol(string symbol)
    {
      if (string.IsNullOrWhiteSpace(symbol))
      {
        return BadRequest("Invalid symbol.");
      }

      string companyTickersUrl = "https://www.sec.gov/files/company_tickers.json";
      var tickersResponse = await _httpClient.GetAsync(companyTickersUrl);
      if (!tickersResponse.IsSuccessStatusCode)
      {
        return StatusCode((int)tickersResponse.StatusCode, "Error retrieving CIK data.");
      }

      var tickersJson = await tickersResponse.Content.ReadAsStringAsync();
      var companyTickers = JsonSerializer.Deserialize<Dictionary<string, CompanyTicker>>(tickersJson);
      if (companyTickers == null || companyTickers.Count == 0)
      {
        return NotFound("No data found for the provided symbol.");
      }

      var company = companyTickers.Values
          .FirstOrDefault(x => x.ticker.Equals(symbol, System.StringComparison.OrdinalIgnoreCase));
      if (company == null)
      {
        return NotFound("CIK not found for the provided symbol.");
      }
      // Format CIK as a 10-digit string with leading zeros.
      string cikFormatted = company.cik_str.ToString("D10");
      string filingsUrl = $"https://data.sec.gov/submissions/CIK{cikFormatted}.json";
      var filingsResponse = await _httpClient.GetAsync(filingsUrl);
      if (!filingsResponse.IsSuccessStatusCode)
      {
        return StatusCode((int)filingsResponse.StatusCode, "Error retrieving SEC filings.");
      }

      var filingsJson = await filingsResponse.Content.ReadAsStringAsync();
      return Content(filingsJson, "application/json");
    }

    // GET api/sec/download/{fileName}
    // Downloads a file from SEC using the provided file name.
    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
      if (string.IsNullOrWhiteSpace(fileName))
        return BadRequest("Invalid file name.");

      // Construct the SEC file URL. (Make sure fileName is safe!)
      string fileUrl = $"https://data.sec.gov/submissions/{fileName}";
      var response = await _httpClient.GetAsync(fileUrl);
      if (!response.IsSuccessStatusCode)
        return StatusCode((int)response.StatusCode, "Error retrieving file.");

      // Get the content type from the response; fallback to application/octet-stream.
      var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
      var fileBytes = await response.Content.ReadAsByteArrayAsync();

      // Return the file for download with a content-disposition header.
      return File(fileBytes, contentType, fileName);
    }



  }



}


