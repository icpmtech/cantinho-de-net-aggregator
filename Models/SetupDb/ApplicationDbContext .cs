using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio;
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
    public DbSet<Dividend> Dividends { get; set; }
    public DbSet<NewsItem> News { get; set; }
    public DbSet<Sector> Sectors { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<MarketAnalyticHub.Models.Portfolio.Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    public DbSet<SymbolItem> Symbols { get; set; }
    public DbSet<NewsScrapingItem> NewsScrapingItem { get; set; }
    public DbSet<UserProfile> UserProfiles { get;  set; }
    public DbSet<QualitativeEvent> QualitativeEvents { get; internal set; }
  }
}
