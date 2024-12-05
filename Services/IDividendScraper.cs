using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DividendApi.Services
{
  public class DividendRecord
  {
    public DateTime Date { get; set; }
    public double Amount { get; set; }
  }

  public interface IDividendScraper
  {
    Task<List<DividendRecord>> GetDividendHistoryAsync(string symbol, DateTime startDate, DateTime endDate);
  }

  public class DividendScraper : IDividendScraper
  {
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://finance.yahoo.com/quote/";

    public DividendScraper(HttpClient httpClient)
    {
      _httpClient = httpClient;
    }

    /// <summary>
    /// Converts a DateTime object to Unix timestamp string.
    /// </summary>
    /// <param name="date">DateTime object</param>
    /// <returns>Unix timestamp as string</returns>
    private string FormatDate(DateTime date)
    {
      DateTimeOffset dto = new DateTimeOffset(date);
      long unixTime = dto.ToUnixTimeSeconds();
      return unixTime.ToString();
    }

    public async Task<List<DividendRecord>> GetDividendHistoryAsync(string symbol, DateTime startDate, DateTime endDate)
    {
      string start = FormatDate(startDate);
      string end = FormatDate(endDate);
      string subdomain = $"{symbol}/history?period1={start}&period2={end}&interval=div%7Csplit&filter=div&frequency=1d";
      string url = _baseUrl + subdomain;

      var request = new HttpRequestMessage(HttpMethod.Get, url);
      // Set necessary headers
      request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
      request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

      HttpResponseMessage response = await _httpClient.SendAsync(request);
      if (!response.IsSuccessStatusCode)
      {
        throw new Exception($"Failed to fetch data. Status Code: {response.StatusCode}");
      }

      string pageContent = await response.Content.ReadAsStringAsync();

      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(pageContent);

      // XPath to locate the dividends table
      var table = doc.DocumentNode.SelectSingleNode("//table[contains(@class, 'W(100%) M(0)')]");
      if (table == null)
      {
        throw new Exception("Dividend table not found.");
      }

      var rows = table.SelectNodes(".//tr");
      if (rows == null || rows.Count == 0)
      {
        throw new Exception("No data found in the dividend table.");
      }

      List<DividendRecord> dividends = new List<DividendRecord>();

      foreach (var row in rows)
      {
        var cells = row.SelectNodes(".//td");
        if (cells != null && cells.Count >= 1)
        {
          string dateText = cells[0].InnerText.Trim();
          string dividendText = cells[1]?.InnerText.Trim();

          if (!string.IsNullOrEmpty(dividendText) && dividendText != "Dividend")
          {
            // Parse the date
            if (DateTime.TryParseExact(dateText, "MMM dd, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
              // Clean the dividend amount
              string cleanedDividend = new string(dividendText.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
              if (double.TryParse(cleanedDividend, out double amount))
              {
                dividends.Add(new DividendRecord
                {
                  Date = date,
                  Amount = amount
                });
              }
            }
          }
        }
      }

      // Optionally, remove the last row if it's a summary or unwanted entry
      if (dividends.Count > 0)
      {
        dividends.RemoveAt(dividends.Count - 1);
      }

      // Order by date ascending
      dividends = dividends.OrderBy(d => d.Date).ToList();

      return dividends;
    }
  }
}
