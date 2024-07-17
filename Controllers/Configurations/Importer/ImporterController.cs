using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ImporterController : ControllerBase
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImporterController> _logger;

    public ImporterController(ApplicationDbContext context, ILogger<ImporterController> logger)
    {
      _context = context;
      _logger = logger;
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv()
    {
      var companies = await _context.Companies.ToListAsync();
      var csvBuilder = new StringBuilder();
      csvBuilder.AppendLine("Name,Description");

      foreach (var company in companies)
      {
        csvBuilder.AppendLine($"{company.Name},{company.Description}");
      }

      var fileName = "companies.csv";
      var mimeType = "text/csv";
      return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), mimeType, fileName);
    }


    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
      if (file == null || file.Length == 0)
      {
        return BadRequest("File is empty");
      }

      using (var stream = new StreamReader(file.OpenReadStream()))
      {
        var companies = new List<Company>();
        while (!stream.EndOfStream)
        {
          var line = await stream.ReadLineAsync();
          var values = line.Split(',');
          if (values.Length >= 2)
          {
            var company = new Company
            {
              Name = values[0],
              Description = values[1]
            };
            companies.Add(company);
          }
        }

        // Save companies to the database
        _context.Companies.AddRange(companies);
         await _context.SaveChangesAsync();
      }

      return Ok("File imported successfully");
    }

    [HttpGet("export-sectors-csv")]
    public async Task<IActionResult> ExportSectorsCsv()
    {
      var sectors = await _context.Sectors.ToListAsync();
      var csvBuilder = new StringBuilder();
      csvBuilder.AppendLine("Name,Description");

      foreach (var sector in sectors)
      {
        csvBuilder.AppendLine($"{sector.Name},{sector.Description}");
      }

      var fileName = "sectors.csv";
      var mimeType = "text/csv";
      return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), mimeType, fileName);
    }


    [HttpPost("import-sectors-csv")]
    public async Task<IActionResult> ImportSectorsCsv(IFormFile file)
    {
      if (file == null || file.Length == 0)
      {
        return BadRequest("File is empty");
      }

      using (var stream = new StreamReader(file.OpenReadStream()))
      {
        var sectors = new List<Sector>();
        while (!stream.EndOfStream)
        {
          var line = await stream.ReadLineAsync();
          var values = line.Split(',');
          if (values.Length >= 2)
          {
            var sector = new Sector
            {
              Name = values[0],
              Description = values[1]
            };
            sectors.Add(sector);
          }
        }

        // Save sectors to the database
        _context.Sectors.AddRange(sectors);
        await _context.SaveChangesAsync();
      }

      return Ok("File imported successfully");
    }

  }
}
