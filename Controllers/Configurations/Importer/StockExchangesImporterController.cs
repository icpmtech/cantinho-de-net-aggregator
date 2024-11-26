using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/stockextange")]
  public class StockExchangesImporterController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StockExchangesImporterController> _logger;

    public StockExchangesImporterController(ApplicationDbContext context, ILogger<StockExchangesImporterController> logger)
    {
      _context = context;
      _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllMarkets()
    {
      try
      {
        var markets = _context.StockExchanges.ToList();
        return Ok(markets);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving markets.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving data.");
      }
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportCsv(IFormFile csvFile)
    {
      if (csvFile == null || csvFile.Length == 0)
      {
        return BadRequest("Please upload a valid CSV file.");
      }

      var markets = new List<StockExchange>();
      var errorMessages = new List<string>();

      try
      {
        using (var reader = new StreamReader(csvFile.OpenReadStream()))
        {
          bool isFirstRow = true;
          int lineNumber = 0;

          while (!reader.EndOfStream)
          {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            // Skip the header row
            if (isFirstRow)
            {
              isFirstRow = false;
              continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
              continue;
            }

            var values = line.Split(',');

            if (values.Length != 12) // Adjusted for the number of columns in the model
            {
              errorMessages.Add($"Line {lineNumber}: Incorrect format. Expected 12 columns but got {values.Length}.");
              continue;
            }

            try
            {
              var market = new StockExchange
              {
                Year = int.Parse(values[0].Trim()),
                StockExchangeName = values[1].Trim(),
                MIC = values[2].Trim(),
                Region = values[3].Trim(),
                City = values[4].Trim(),
                MarketCapUsdTrillion = string.IsNullOrEmpty(values[5].Trim()) ? (double?)null : double.Parse(values[5].Trim(), CultureInfo.InvariantCulture),
                MonthlyTradeVolumeUsdBillion = string.IsNullOrEmpty(values[6].Trim()) ? (double?)null : double.Parse(values[6].Trim(), CultureInfo.InvariantCulture),
                TimeZone = values[7].Trim(),
                UtcOffset = double.Parse(values[8].Trim(), CultureInfo.InvariantCulture),
                DST = values[9].Trim(),
                OpenHoursLocal = JsonSerializer.Deserialize<OpenHours>(values[10].Trim()),
                OpenHoursUTC = JsonSerializer.Deserialize<OpenHours>(values[11].Trim())
              };

              markets.Add(market);
            }
            catch (Exception ex)
            {
              errorMessages.Add($"Line {lineNumber}: Error processing line. Error: {ex.Message}");
              _logger.LogError(ex, $"Error processing line {lineNumber}: {line}");
            }
          }
        }

        if (markets.Any())
        {
          await _context.StockExchanges.AddRangeAsync(markets);
          await _context.SaveChangesAsync();
        }

        return Ok(new
        {
          SuccessMessage = $"{markets.Count} records successfully imported.",
          Errors = errorMessages
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Unexpected error during CSV import.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred during import.");
      }
    }

    [HttpGet("export")]
    public IActionResult ExportCsv()
    {
      try
      {
        var markets = _context.StockExchanges.ToList();
        var csv = new StringBuilder();

        // Add header
        csv.AppendLine("Year,StockExchangeName,MIC,Region,City,MarketCapUsdTrillion,MonthlyTradeVolumeUsdBillion,TimeZone,UtcOffset,DST,OpenHoursLocal,OpenHoursUTC");

        // Add rows
        foreach (var market in markets)
        {
          csv.AppendLine($"{market.Year},{EscapeCsvValue(market.StockExchangeName)},{EscapeCsvValue(market.MIC)},{EscapeCsvValue(market.Region)},{EscapeCsvValue(market.City)},{market.MarketCapUsdTrillion},{market.MonthlyTradeVolumeUsdBillion},{EscapeCsvValue(market.TimeZone)},{market.UtcOffset},{EscapeCsvValue(market.DST)},{EscapeCsvValue(JsonSerializer.Serialize(market.OpenHoursLocal))},{EscapeCsvValue(JsonSerializer.Serialize(market.OpenHoursUTC))}");
        }

        var fileBytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(fileBytes, "text/csv", "markets.csv");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error exporting CSV.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred during export.");
      }
    }

    private string EscapeCsvValue(string value)
    {
      if (string.IsNullOrEmpty(value)) return "";
      return value.Contains(",") || value.Contains("\"") || value.Contains("\n")
          ? $"\"{value.Replace("\"", "\"\"")}\""
          : value;
    }
  }
}
