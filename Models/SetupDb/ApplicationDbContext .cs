using MarketAnalyticHub.Models.Configurations.News;
using MarketAnalyticHub.Models.News;
using MarketAnalyticHub.Models.Portfolio.Entities;
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

    public DbSet<UserCredit> UserCredits { get; set; }
    public DbSet<Dividend> Dividends { get; set; }
    public DbSet<NewsItem> News { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<PortfolioAlertLog> PortfolioAlertLogs { get; set; }
    public DbSet<PushSubscriptionEntity> PushSubscriptions { get; set; }
    public DbSet<DividendsTracker> DividendsTrackers { get; set; }
    public DbSet<IndexDividendsTracker> IndexDividendsTrackers { get; set; }
    public DbSet<DividendIndex> DividendIndices { get; set; }
    public DbSet<CreditRatingAgency> CreditRatingAgencies { get; set; }
    public DbSet<Company> Companies { get; set; }


    public DbSet<RSSLink> RSSLinks { get; set; }
    public DbSet<MarketAnalyticHub.Models.Portfolio.Portfolio> Portfolios { get; set; }
    public DbSet<PortfolioItem> PortfolioItems { get; set; }
    public DbSet<SymbolItem> Symbols { get; set; }
    public DbSet<NewsScrapingItem> NewsScrapingItems { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<QualitativeEvent> QualitativeEvents { get; set; }
    public DbSet<StockEvent> StockEvents { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<PortfolioLossRule> PortfolioLossRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      // Configure relationships
      modelBuilder.Entity<PortfolioItem>()
          .HasMany(p => p.StockEvents)
          .WithOne(e => e.PortfolioItem)
          .HasForeignKey(e => e.PortfolioItemId);

      modelBuilder.Entity<UserProfile>()
          .HasOne(up => up.UserCredit)
          .WithOne(uc => uc.UserProfile)
          .HasForeignKey<UserCredit>(uc => uc.UserId);

      // Configure the many-to-many relationship
      modelBuilder.Entity<DividendIndex>()
          .HasKey(di => new { di.DividendsTrackerId, di.IndexDividendsTrackerId });

      modelBuilder.Entity<DividendIndex>()
          .HasOne(di => di.DividendsTracker)
          .WithMany(d => d.DividendIndices)
          .HasForeignKey(di => di.DividendsTrackerId);

      modelBuilder.Entity<DividendIndex>()
          .HasOne(di => di.IndexDividendsTracker)
          .WithMany(i => i.DividendIndices)
          .HasForeignKey(di => di.IndexDividendsTrackerId);

      // Seed data for DividendsTrackers
      modelBuilder.Entity<DividendsTracker>().HasData(
          new DividendsTracker
          {
            Id = 1,
            Company = "Banco de Sabadell, S.A.",
            Ticker = "SAB",
            Country = "Spain",
            Region = "Europe",
            Exchange = "XVAL",
            SharePrice = "€1.80",
            PrevDividend = "3¢"
          },
          new DividendsTracker
          {
            Id = 2,
            Company = "Industria De Diseno Textil SA",
            Ticker = "ITX",
            Country = "Spain",
            Region = "Europe",
            Exchange = "XMAD",
            SharePrice = "€49.40",
            PrevDividend = "19.6¢"
          },
          new DividendsTracker
          {
            Id = 3,
            Company = "Telefonica S.A",
            Ticker = "TEF",
            Country = "Spain",
            Region = "Europe",
            Exchange = "XMAD",
            SharePrice = "€4.06",
            PrevDividend = "15¢"
          },
          new DividendsTracker
          {
            Id = 4,
            Company = "Apple Inc.",
            Ticker = "AAPL",
            Country = "United States",
            Region = "North America",
            Exchange = "NASDAQ",
            SharePrice = "$150.00",
            PrevDividend = "20¢"
          },
          new DividendsTracker
          {
            Id = 5,
            Company = "Microsoft Corp",
            Ticker = "MSFT",
            Country = "United States",
            Region = "North America",
            Exchange = "NASDAQ",
            SharePrice = "$280.00",
            PrevDividend = "30¢"
          },
          new DividendsTracker
          {
            Id = 6,
            Company = "Royal Dutch Shell",
            Ticker = "RDSA",
            Country = "United Kingdom",
            Region = "United Kingdom",
            Exchange = "LSE",
            SharePrice = "£20.00",
            PrevDividend = "40p"
          },
          new DividendsTracker
          {
            Id = 7,
            Company = "Unilever PLC",
            Ticker = "ULVR",
            Country = "United Kingdom",
            Region = "United Kingdom",
            Exchange = "LSE",
            SharePrice = "£43.00",
            PrevDividend = "37p"
          }
      );

      // Seed data for IndexDividendsTrackers
      modelBuilder.Entity<IndexDividendsTracker>().HasData(
          new IndexDividendsTracker
          {
            Id = 1,
            Region = "Europe",
            Indices = new List<string>
              {
                        "AEX 25", "BBC Global 30", "BEL 20", "CAC 40",
                        "DAX 40", "Euronext 100", "Euro Stoxx 50",
                        "FTSE Eurotop 100", "FTSE MIB", "IBEX 35",
                        "OBX Norway 25", "OMX Copenhagen 20", "OMX Helsinki 25",
                        "OMX Stockholm 30", "PSI 20", "S&P Global 100",
                        "STOXX600", "TSIC Dutch15", "TSIC Euro30"
              }
          },
          new IndexDividendsTracker
          {
            Id = 2,
            Region = "North America",
            Indices = new List<string>
              {
                        "S&P 500", "NASDAQ 100",
                        "Dow Jones Industrial Average", "Russell 2000"
              }
          },
          new IndexDividendsTracker
          {
            Id = 3,
            Region = "United Kingdom",
            Indices = new List<string>
              {
                        "FTSE 100", "FTSE 250",
                        "FTSE All-Share", "FTSE AIM 100"
              }
          }
      );

      // Seed data for the DividendIndex entity
      modelBuilder.Entity<DividendIndex>().HasData(
          new DividendIndex { DividendsTrackerId = 3, IndexDividendsTrackerId = 1 },
          new DividendIndex { DividendsTrackerId = 4, IndexDividendsTrackerId = 2 },
          new DividendIndex { DividendsTrackerId = 5, IndexDividendsTrackerId = 2 },
          new DividendIndex { DividendsTrackerId = 6, IndexDividendsTrackerId = 3 },
          new DividendIndex { DividendsTrackerId = 7, IndexDividendsTrackerId = 3 }
      );

      // Seed data for CreditRatingAgencies
      modelBuilder.Entity<CreditRatingAgency>().HasData(
          new CreditRatingAgency { Id = 1, Name = "Moody's", Country = "United States", Website = "https://www.moodys.com/", Description = "Moody's is a leading global provider of credit ratings, research, and risk analysis." },
          new CreditRatingAgency { Id = 2, Name = "Standard & Poor's (S&P)", Country = "United States", Website = "https://www.standardandpoors.com/", Description = "S&P Global Ratings is known for providing credit ratings, research, and insights essential to global markets." }
      );
    }
  }
}
