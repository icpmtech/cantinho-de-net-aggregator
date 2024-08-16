using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Configurations.News;
using MarketAnalyticHub.Models.News;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MarketAnalyticHub.Controllers;
using MarketAnalyticHub.Models.Portfolio.Entities;
using MarketAnalyticHub.Controllers.api;

namespace MarketAnalyticHub.Models.SetupDb
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }
    public DbSet<UserCredit> UserCredits { get; set; } // Add this line
    public DbSet<Dividend> Dividends { get; set; }
    public DbSet<NewsItem> News { get; set; }
    public DbSet<Sector> Sectors { get; set; }

    public DbSet<Company> Companies { get; set; }
    public DbSet<RSSLink> RSSLinks { get; set; }
    public DbSet<MarketAnalyticHub.Models.Portfolio.Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }

    public DbSet<SymbolItem> Symbols { get; set; }
    public DbSet<NewsScrapingItem> NewsScrapingItem { get; set; }
    public DbSet<UserProfile> UserProfiles { get;  set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<QualitativeEvent> QualitativeEvents { get; internal set; }

    public DbSet<StockEvent> StockEvents { get; set; }

    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PortfolioLossRule> PortfolioLossRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<PortfolioItem>()
          .HasMany(p => p.StockEvents)
          .WithOne(e => e.PortfolioItem)
          .HasForeignKey(e => e.PortfolioItemId);

      modelBuilder.Entity<UserProfile>()
         .HasOne(up => up.UserCredit)
         .WithOne(uc => uc.UserProfile)
         .HasForeignKey<UserCredit>(uc => uc.UserId);


    }


  }
}
