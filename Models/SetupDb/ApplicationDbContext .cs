using AspnetCoreMvcFull.Models.Portfolio;
using MarketAnalyticHub.Models.Configurations.News;
using MarketAnalyticHub.Models.News;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MarketAnalyticHub.Models.SetupDb
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<NewsItem> News { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    public DbSet<SymbolItem> Symbols { get; set; }
    public DbSet<NewsScrapingItem> NewsScrapingItem { get; set; }
    public DbSet<UserProfile> UserProfiles { get;  set; }
  }
}
