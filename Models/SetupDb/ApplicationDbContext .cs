using AspnetCoreMvcFull.Models.Configurations.News;
using AspnetCoreMvcFull.Models.News;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AspnetCoreMvcFull.Models.SetupDb
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<NewsItem> News { get; set; }
    public DbSet<NewsScrapingItem> NewsScrapingItem { get; set; }
    
  }
}
