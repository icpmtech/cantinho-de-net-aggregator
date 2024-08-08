using MarketAnalyticHub.Services;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class SymbolsAPIController : ControllerBase
  {
    private readonly ISymbolService _symbolService;
    private readonly ApplicationDbContext _context;
    private readonly IYahooFinanceService _yahooFinanceService;
    public SymbolsAPIController(ApplicationDbContext context, ISymbolService symbolService, IYahooFinanceService  yahooFinanceService)
    {
      _symbolService = symbolService;
      _yahooFinanceService = yahooFinanceService;
      _context = context;
    }

    // GET: api/symbols
    [HttpGet]
    public async Task<IActionResult> GetSymbols()
    {
      var symbols = await _symbolService.GetSymbolsAsync();
     var symbolsData= symbols.Select(s =>new
     {
       symbol = s,
       currentStockData = _yahooFinanceService.GetRealTimePriceAsync(s)
     });
      return Ok(symbols);
    }
    // GET: api/symbols
    [HttpGet("symbols-current-data")]
    public async Task<IActionResult> GetSymbolsCurrentData(DateTime dateValue)
    {
      var symbols = await _symbolService.GetSymbolsAsync();
      var symbolsData = symbols.Select(s => new
      {
        symbol = s,
        currentStockData = _yahooFinanceService.GetDailyHistoricalDataAsync(s, dateValue)
      });
      return Ok(symbolsData);
    }
    // GET: api/symbols/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSymbol(int id)
    {
      var symbol = await _context.Symbols.FindAsync(id);
      if (symbol == null)
      {
        return NotFound(new { success = false, message = "Symbol not found" });
      }
      return Ok(new { success = true, symbol = symbol });
    }

    // POST: api/symbols
    [HttpPost]
    public async Task<IActionResult> AddSymbol([FromBody] SymbolItem symbol)
    {
      _context.Symbols.Add(symbol);
      await _context.SaveChangesAsync();
      var symbols = await _context.Symbols.ToListAsync();
      return Ok(new { success = true, message = "Symbol added successfully", symbols });
    }

    // PUT: api/symbols/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> EditSymbol(int id, [FromBody] SymbolItem symbol)
    {
      var existingSymbol = await _context.Symbols.FindAsync(id);
      if (existingSymbol == null)
      {
        return NotFound(new { success = false, message = "Symbol not found" });
      }

      existingSymbol.Category = symbol.Category;
      existingSymbol.Title = symbol.Title;
      existingSymbol.Description = symbol.Description;
      existingSymbol.Link = symbol.Link;
      existingSymbol.Date = symbol.Date;

      await _context.SaveChangesAsync();
      var symbols = await _context.Symbols.ToListAsync();
      return Ok(new { success = true, message = "Symbol updated successfully", symbols });
    }

    // DELETE: api/symbols/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSymbol(int id)
    {
      var symbol = await _context.Symbols.FindAsync(id);
      if (symbol == null)
      {
        return NotFound(new { success = false, message = "Symbol not found" });
      }
      _context.Symbols.Remove(symbol);
      await _context.SaveChangesAsync();
      var symbols = await _context.Symbols.ToListAsync();
      return Ok(new { success = true, message = "Symbol deleted successfully", symbols });
    }
  }
}
