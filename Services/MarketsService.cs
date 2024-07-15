using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationDbContext = MarketAnalyticHub.Models.SetupDb.ApplicationDbContext;

namespace MarketAnalyticHub.Services
{
  public class MarketsService
  {
    private readonly ApplicationDbContext _context;

    public MarketsService(ApplicationDbContext context)
    {
      _context = context;
    }
    public async Task<SymbolItem> GetSymbolsByIdAsync(int id)
    {
      return await _context.Symbols.FindAsync(id);
    }

    public async Task<bool> UpdateSymbolsAsync(SymbolItem updatedSymbols)
    {
      var SymbolItem = await _context.Symbols.FindAsync(updatedSymbols.Id);
      if (SymbolItem == null)
      {
        return false;
      }

      SymbolItem.Category = updatedSymbols.Category;
      SymbolItem.Title = updatedSymbols.Title;
      SymbolItem.Description = updatedSymbols.Description;
      SymbolItem.Link = updatedSymbols.Link;
      SymbolItem.Date = updatedSymbols.Date;

      _context.Symbols.Update(SymbolItem);
      await _context.SaveChangesAsync();
      return true;
    }

    public async Task<PaginatedList<SymbolItem>> GetPaginatedSymbolsAsync(string category, string sortOrder, int pageNumber, int pageSize, string searchQuery)
    {
      var query = _context.Symbols.AsQueryable();

      if (!string.IsNullOrEmpty(category))
      {
        query = query.Where(n => n.Category == category);
      }

      if (!string.IsNullOrEmpty(searchQuery))
      {
        query = query.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
      }

      query = sortOrder switch
      {
        "asc" => query.OrderBy(n => n.Category),
        "desc" => query.OrderByDescending(n => n.Category),
        _ => query.OrderBy(n => n.Date)
      };

      return await PaginatedList<SymbolItem>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);
    }
  }
}

