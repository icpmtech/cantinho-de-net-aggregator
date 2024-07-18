using AspnetCoreMvcFull.Models.Portfolio;
using AspnetCoreMvcFull.Services;
using DocumentFormat.OpenXml.InkML;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[Authorize]
public class PortfolioController : Controller
{
  private readonly PortfolioService _portfolioService;
  private readonly UserManager<ApplicationUser> _userManager;
  public PortfolioController(PortfolioService portfolioService, UserManager<ApplicationUser> userManager)
  {
    _portfolioService = portfolioService;
    _userManager = userManager;
  }

  [HttpGet("Export")]
  public async Task<IActionResult> Export([FromQuery] string fileType)
  {
    var userId = _userManager.GetUserId(User);
    var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);

    if (fileType == "csv")
    {
      var csvData = _portfolioService.ExportToCsv(portfolios);
      var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
      return File(bytes, "text/csv", "portfolios.csv");
    }
    else
    {
      using (var workbook = _portfolioService.ExportToExcel(portfolios))
      {
        using (var stream = new MemoryStream())
        {
          workbook.SaveAs(stream);
          var content = stream.ToArray();
          return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "portfolios.xlsx");
        }
      }
    }
  }

  [HttpPost("Import")]
  public async Task<IActionResult> Import([FromForm] IFormFile file)
  {
    if (file == null || file.Length == 0)
      return BadRequest("File is empty");

    var userId = _userManager.GetUserId(User);
    var extension = Path.GetExtension(file.FileName).ToLower();

    using (var stream = new MemoryStream())
    {
      await file.CopyToAsync(stream);
      stream.Position = 0;

      if (extension == ".csv")
      {
        using (var reader = new StreamReader(stream))
        {
          var csvData = await reader.ReadToEndAsync();
          await _portfolioService.ImportFromCsv(csvData, userId);
        }
      }
      else if (extension == ".xlsx")
      {
        await _portfolioService.ImportFromExcel(stream, userId);
      }
      else
      {
        return BadRequest("Unsupported file format");
      }
    }

    return Ok();
  }

  [HttpGet]
  public async Task<IActionResult> GetPortfolios()
  {
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var portfolios = await _portfolioService.GetPortfoliosByUserAsync(userId);
    return Ok(portfolios);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetPortfolio(int id)
  {
    var portfolio = await _portfolioService.GetPortfolioByIdAsync(id);
    if (portfolio == null)
    {
      return NotFound();
    }
    return Ok(portfolio);
  }

  [HttpPost]
  public async Task<IActionResult> AddPortfolio([FromBody] Portfolio portfolio)
  {
    portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    await _portfolioService.AddPortfolioAsync(portfolio);
    return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> UpdatePortfolio(int id, [FromBody] Portfolio portfolio)
  {
    portfolio.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (id != portfolio.Id)
    {
      return BadRequest();
    }
    await _portfolioService.UpdatePortfolioAsync(portfolio);
    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeletePortfolio(int id)
  {
    await _portfolioService.DeletePortfolioAsync(id);
    return NoContent();
  }
}
