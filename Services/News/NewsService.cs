using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.News;
using AspnetCoreMvcFull.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Services.News
{
  public class AppNewsService
  {
    private readonly ApplicationDbContext _context;

    public AppNewsService(ApplicationDbContext context)
    {
      _context = context;
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
  }
}
  
