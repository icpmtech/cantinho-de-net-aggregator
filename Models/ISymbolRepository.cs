
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Models
{
  public interface ISymbolRepository
  {
    Task<IEnumerable<SymbolItem>> GetSymbolsAsync();
  }

  public class SymbolRepository : ISymbolRepository
  {
    private readonly ApplicationDbContext _context;

    public SymbolRepository(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<SymbolItem>> GetSymbolsAsync()
    {
      return await _context.Symbols.ToListAsync();
    }
  }
}
