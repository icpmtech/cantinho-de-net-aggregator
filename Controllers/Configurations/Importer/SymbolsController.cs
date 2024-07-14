using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  public class SymbolsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SymbolsController> _logger;

    public SymbolsController(ApplicationDbContext context, ILogger<SymbolsController> logger)
    {
      _context = context;
      _logger = logger;
    }

    public IActionResult Index()
    {
      var symbols = _context.Symbols.ToList();
      return View(symbols);
    }

    [HttpPost]
    public async Task<IActionResult> ImportCsv(IFormFile csvFile)
    {
      if (csvFile == null || csvFile.Length == 0)
      {
        ViewBag.ErrorMessages = new List<string> { "Please upload a valid CSV file." };
        return RedirectToAction("Index");
      }

      var symbols = new List<SymbolItem>();
      var errorMessages = new List<string>();

      using (var reader = new StreamReader(csvFile.OpenReadStream()))
      {
        bool isFirstRow = true;
        while (!reader.EndOfStream)
        {
          var line = await reader.ReadLineAsync();

          // Skip header row
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

          if (values.Length != 5)
          {
            errorMessages.Add($"Incorrect format in line: {line}");
            continue;
          }

          try
          {
            DateTime dateTime = DateTime.ParseExact(values[4], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var symbol = new SymbolItem
            {
              Category = values[0],
              Title = values[1],
              Description = values[2],
              Link = values[3],
              Date = dateTime.ToString()
            };

            symbols.Add(symbol);
          }
          catch (Exception ex)
          {
            errorMessages.Add($"Error processing line: {line}. Error: {ex.Message}");
            _logger.LogError(ex, $"Error processing line: {line}");
          }
        }
      }

      if (symbols.Count > 0)
      {
        _context.Symbols.AddRange(symbols);
        await _context.SaveChangesAsync();
        ViewBag.SuccessMessage = "CSV file successfully imported.";
      }

      if (errorMessages.Count > 0)
      {
        ViewBag.ErrorMessages = errorMessages;
      }

      return View("Index", _context.Symbols.ToList());
    }
    [HttpGet]
    public IActionResult ExportCsv()
    {
      var symbols = _context.Symbols.ToList();
      var csv = new StringBuilder();

      // Add header
      csv.AppendLine("Category,Title,Description,Link,Date");

      // Add rows
      foreach (var symbol in symbols)
      {
        csv.AppendLine($"{symbol.Category},{symbol.Title},{symbol.Description},{symbol.Link},{symbol.Date}");
      }

      return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "symbols.csv");
    }

    // Other action methods...
  }
}
