using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Models
{
  public interface IIndicesDividendsTrackerRepository
  {
    Task<IEnumerable<string>> GetIndicesAsync(string region);
    Task<IndexDividendsTracker> FindAsync(int indexId);
  }

  public interface IDividendsTrackerRepository
  {
    Task<IEnumerable<DividendsTracker>> GetDividendsAsync(string region, string index);
    Task<IEnumerable<string>> GetIndicesAsync(string region);
    Task<DividendsTracker> GetDividendByIdAsync(int id);
    Task AddDividendAsync(DividendsTracker dividend);
    Task UpdateDividendAsync(DividendsTracker dividend);
    Task DeleteDividendAsync(int id);
    Task<IEnumerable<DividendsTracker>> GetAllDividendsAsync();
    Task<IEnumerable<DividendsTracker>> GetDividendsByRegionAndExchangeAsync(string region, string exchange);
  }

  public class IndicesDividendsTrackerRepository : IIndicesDividendsTrackerRepository
  {
    private readonly ApplicationDbContext _context;

    public IndicesDividendsTrackerRepository(ApplicationDbContext context)
    {
      _context = context;
    }
    public async Task<IndexDividendsTracker> FindAsync(int indexId)
    {
      // Find the entity in the database by its ID
      var indicesEntity = await _context.IndexDividendsTrackers.FindAsync(indexId);

      // Return the found entity (or null if not found)
      return indicesEntity;
    }


    public async Task<IEnumerable<string>> GetIndicesAsync(string region)
    {
      var indicesEntity = await _context.IndexDividendsTrackers
                                        .FirstOrDefaultAsync(i => i.Region == region);

      return indicesEntity?.Indices ?? new List<string>();
    }
  }

  public class DividendsTrackerRepository : IDividendsTrackerRepository
  {
    private readonly ApplicationDbContext _context;

    public DividendsTrackerRepository(ApplicationDbContext context)
    {
      _context = context;
    }
    public async Task<IEnumerable<DividendsTracker>> GetDividendsByRegionAndExchangeAsync(string region, string exchange)
    {
      var query = _context.DividendsTrackers.AsQueryable();

      if (!string.IsNullOrEmpty(region))
      {
        query = query.Where(d => d.Region == region);
      }

      if (!string.IsNullOrEmpty(exchange))
      {
        query = query.Where(d => d.Exchange == exchange);
      }

      return await query.Include(d => d.DividendIndices)
                        .ThenInclude(di => di.IndexDividendsTracker)
                        .ToListAsync();
    }

    public async Task<IEnumerable<DividendsTracker>> GetDividendsAsync(string region, string index)
    {
      var query = _context.DividendsTrackers
                          .Include(d => d.DividendIndices)
                          .ThenInclude(di => di.IndexDividendsTracker)
                          .AsQueryable();

      // Ensure that Region is not null or empty
      query = query.Where(d => !string.IsNullOrEmpty(d.Region));

      // Ensure that Indices are present and not empty
      query = query.Where(d => d.DividendIndices
                                .Any(di => di.IndexDividendsTracker.Indices != null &&
                                           di.IndexDividendsTracker.Indices.Any()));

      // Apply region filter if provided
      if (!string.IsNullOrEmpty(region))
      {
        query = query.Where(d => d.Region == region);
      }

      // Apply index filter if provided
      if (!string.IsNullOrEmpty(index))
      {
        query = query.Where(d => d.DividendIndices
                                  .Any(di => di.IndexDividendsTracker.Indices.Contains(index)));
      }

      return await query.ToListAsync();
    }


    public async Task<IEnumerable<string>> GetIndicesAsync(string region)
    {
      return await _context.IndexDividendsTrackers
                           .Where(i => i.Region == region)
                           .SelectMany(i => i.Indices)
                           .ToListAsync();
    }

    public async Task<DividendsTracker> GetDividendByIdAsync(int id)
    {
      return await _context.DividendsTrackers
                           .Include(d => d.DividendIndices)
                           .ThenInclude(di => di.IndexDividendsTracker)
                           .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddDividendAsync(DividendsTracker dividend)
    {
      _context.DividendsTrackers.Add(dividend);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateDividendAsync(DividendsTracker dividend)
    {
      _context.Entry(dividend).State = EntityState.Modified;
      await _context.SaveChangesAsync();
    }

    public async Task DeleteDividendAsync(int id)
    {
      var dividend = await _context.DividendsTrackers.FindAsync(id);
      if (dividend != null)
      {
        _context.DividendsTrackers.Remove(dividend);
        await _context.SaveChangesAsync();
      }
    }

    public async Task<IEnumerable<DividendsTracker>> GetAllDividendsAsync()
    {
      return await _context.DividendsTrackers
                           .Include(d => d.DividendIndices)
                           .ThenInclude(di => di.IndexDividendsTracker)
                           .ToListAsync();
    }
  }
}
