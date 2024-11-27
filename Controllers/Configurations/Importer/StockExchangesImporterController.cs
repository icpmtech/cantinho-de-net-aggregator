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
              var market = new StockExchange();

              try
              {
                market.Year = int.Parse(values[0].Trim());
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing Year: {ex.Message}");
              }

              try
              {
                market.StockExchangeName = values[1].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing StockExchangeName: {ex.Message}");
              }

              try
              {
                market.MIC = values[2].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing MIC: {ex.Message}");
              }

              try
              {
                market.Region = values[3].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing Region: {ex.Message}");
              }

              try
              {
                market.City = values[4].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing City: {ex.Message}");
              }

              try
              {
                market.MarketCapUsdTrillion = string.IsNullOrEmpty(values[5].Trim()) ?
                    (double?)null : double.Parse(values[5].Trim(), CultureInfo.InvariantCulture);
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing MarketCapUsdTrillion: {ex.Message}");
              }

              try
              {
                market.MonthlyTradeVolumeUsdBillion = string.IsNullOrEmpty(values[6].Trim()) ?
                    (double?)null : double.Parse(values[6].Trim(), CultureInfo.InvariantCulture);
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing MonthlyTradeVolumeUsdBillion: {ex.Message}");
              }

              try
              {
                market.TimeZone = values[7].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing TimeZone: {ex.Message}");
              }

              try
              {
                market.UtcOffset = double.Parse(values[8].Trim(), CultureInfo.InvariantCulture);
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing UtcOffset: {ex.Message}");
              }

              try
              {
                market.DST = values[9].Trim();
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing DST: {ex.Message}");
              }

              try
              {
                market.OpenHoursLocal = JsonSerializer.Deserialize<OpenHours>(values[10].Trim());
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing OpenHoursLocal: {ex.Message}");
              }

              try
              {
                market.OpenHoursUTC = JsonSerializer.Deserialize<OpenHours>(values[11].Trim());
              }
              catch (Exception ex)
              {
                _logger.LogError(ex,$"Error parsing OpenHoursUTC: {ex.Message}");
              }

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
    [HttpPost("create")]
    public async Task<IActionResult> CreateMarket([FromBody] StockExchange market)
    {
      if (market == null || string.IsNullOrWhiteSpace(market.StockExchangeName))
      {
        return BadRequest("Invalid market data. StockExchangeName is required.");
      }

      try
      {
        var existingMarket = _context.StockExchanges
            .FirstOrDefault(m => m.StockExchangeName == market.StockExchangeName);

        if (existingMarket != null)
        {
          return BadRequest("A market with the same StockExchangeName already exists.");
        }

        await _context.StockExchanges.AddAsync(market);
        await _context.SaveChangesAsync();

        return Ok("Market created successfully.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating market.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the market.");
      }
    }
    [HttpPut("edit/{stockExchangeName}")]
    public async Task<IActionResult> EditMarket(string stockExchangeName, [FromBody] StockExchange updatedMarket)
    {
      if (string.IsNullOrWhiteSpace(stockExchangeName))
      {
        return BadRequest("StockExchangeName is required.");
      }

      if (updatedMarket == null || string.IsNullOrWhiteSpace(updatedMarket.StockExchangeName))
      {
        return BadRequest("Invalid market data.");
      }

      try
      {
        var existingMarket = _context.StockExchanges
            .FirstOrDefault(m => m.StockExchangeName.Equals(stockExchangeName));

        if (existingMarket == null)
        {
          return NotFound($"Market with StockExchangeName '{stockExchangeName}' not found.");
        }

        // Update fields
        existingMarket.Year = updatedMarket.Year;
        existingMarket.MIC = updatedMarket.MIC;
        existingMarket.Region = updatedMarket.Region;
        existingMarket.City = updatedMarket.City;
        existingMarket.MarketCapUsdTrillion = updatedMarket.MarketCapUsdTrillion;
        existingMarket.MonthlyTradeVolumeUsdBillion = updatedMarket.MonthlyTradeVolumeUsdBillion;
        existingMarket.TimeZone = updatedMarket.TimeZone;
        existingMarket.UtcOffset = updatedMarket.UtcOffset;
        existingMarket.DST = updatedMarket.DST;
        existingMarket.OpenHoursLocal = updatedMarket.OpenHoursLocal;
        existingMarket.OpenHoursUTC = updatedMarket.OpenHoursUTC;
        existingMarket.Description = updatedMarket.Description;

        _context.StockExchanges.Update(existingMarket);
        await _context.SaveChangesAsync();

        return Ok(new { SuccessMessage = "Market updated successfully.", Market = existingMarket });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error editing market with StockExchangeName '{stockExchangeName}'.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while editing the market.");
      }
    }
    [HttpGet("details/{stockExchangeName}")]
    public IActionResult GetMarketDetails(string stockExchangeName)
    {
      if (string.IsNullOrWhiteSpace(stockExchangeName))
      {
        return BadRequest("StockExchangeName is required.");
      }

      try
      {
        var market = _context.StockExchanges
            .FirstOrDefault(m => m.StockExchangeName.Equals(stockExchangeName));

        if (market == null)
        {
          return NotFound($"Market with StockExchangeName '{stockExchangeName}' not found.");
        }

        return Ok(market);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving details for StockExchangeName '{stockExchangeName}'.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the market details.");
      }
    }

    [HttpDelete("delete/{stockExchangeName}")]
    public async Task<IActionResult> DeleteMarket(string stockExchangeName)
    {
      if (string.IsNullOrWhiteSpace(stockExchangeName))
      {
        return BadRequest("StockExchangeName is required.");
      }

      try
      {
        var market = _context.StockExchanges
            .FirstOrDefault(m => m.StockExchangeName.Equals(stockExchangeName));

        if (market == null)
        {
          return NotFound($"Market with StockExchangeName '{stockExchangeName}' not found.");
        }

        _context.StockExchanges.Remove(market);
        await _context.SaveChangesAsync();

        return Ok($"Market with StockExchangeName '{stockExchangeName}' deleted successfully.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error deleting market with StockExchangeName '{stockExchangeName}'.");
        return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the market.");
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
