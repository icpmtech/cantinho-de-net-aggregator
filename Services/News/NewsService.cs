using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationDbContext = MarketAnalyticHub.Models.SetupDb.ApplicationDbContext;

namespace MarketAnalyticHub.Services.News
{
  public class AppNewsService
  {
    private readonly ApplicationDbContext _context;

    public AppNewsService(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<NewsItem>> Get()
    {
      return await _context.News.ToListAsync();
    }
    public async Task<NewsItem> GetNewsByIdAsync(int id)
    {
      return await _context.News.FindAsync(id);
    }

    public async Task<bool> UpdateNewsAsync(NewsItem updatedNews)
    {
      var newsItem = await _context.News.FindAsync(updatedNews.Id);
      if (newsItem == null)
      {
        return false;
      }

      newsItem.Category = updatedNews.Category;
      newsItem.Title = updatedNews.Title;
      newsItem.Description = updatedNews.Description;
      newsItem.Link = updatedNews.Link;
      newsItem.Date = updatedNews.Date;

      _context.News.Update(newsItem);
      await _context.SaveChangesAsync();
      return true;
    }
    public async Task<PaginatedList<NewsItem>> GetPaginatedNewsAsync(string category, string sortOrder, int pageNumber, int pageSize, string searchQuery, DateTime? startDate = null, DateTime? endDate = null)
    {
      var query = _context.News.AsQueryable();

      if (!string.IsNullOrEmpty(category))
      {
        query = query.Where(n => n.Category == category);
      }

      if (!string.IsNullOrEmpty(searchQuery))
      {
        query = query.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
      }

      // Fetch data from the database first
      var newsItems = await query.AsNoTracking().ToListAsync();

      // Filter dates after fetching
      if (startDate.HasValue || endDate.HasValue)
      {
        newsItems = newsItems.Where(n => IsDateInRange(n.Date, startDate, endDate)).ToList();
      }

      // Sort the filtered data
      newsItems = sortOrder switch
      {
        "asc" => newsItems.OrderBy(n => n.Category).ToList(),
        "desc" => newsItems.OrderByDescending(n => n.Category).ToList(),
        _ => newsItems.OrderBy(n => DateTime.TryParse(n.Date, out var date) ? date : DateTime.MaxValue).ToList()
      };

      // Apply pagination
      return PaginatedList<NewsItem>.Create(newsItems, pageNumber, pageSize);
    }

    public static bool IsDateInRange(string dateStr, DateTime? startDate, DateTime? endDate)
    {
      if (DateTime.TryParse(dateStr, out var date))
      {
        if (startDate.HasValue && date < startDate.Value)
        {
          return false;
        }
        if (endDate.HasValue && date > endDate.Value)
        {
          return false;
        }
        return true;
      }
      return false;
    }

    public async Task<PaginatedList<NewsItem>> GetPaginatedNewsAsync(string category, string sortOrder, int pageNumber, int pageSize, string searchQuery)
    {
      var query = _context.News.AsQueryable();

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

      return await PaginatedList<NewsItem>.CreateAsync(query.AsNoTracking(), pageNumber, pageSize);
    }

    public async Task<IEnumerable<StockEvent>> GetStockEventsAsync(string ticker)
    {
      return new List<StockEvent>();
    }
  }
}
  
