using static MarketAnalyticHub.Controllers.SocialSentimentService;
using System.ComponentModel.DataAnnotations.Schema;
using MarketAnalyticHub.Models.SetupDb;

namespace MarketAnalyticHub.Models.Portfolio.Entities
{
  public class PortfolioItem
  {
    public int Id { get; set; }
    public string OperationType { get; set; }
    public string? SalePrice { get; set; }
    public string? SaleCommission { get; set; }
    
    public string UserId { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? Commission { get; set; }

    public int? CompanyId { get; set; } // Foreign key for Company
    public Company? Industry { get; set; } // Navigation property

    // Navigation property for dividends
    public ICollection<Dividend>? Dividends { get; set; }

    // Calculated fields
    public decimal TotalInvestment => Quantity * PurchasePrice + Commission ?? 0;
    public decimal CurrentMarketValue => Quantity * CurrentPrice;

    // Total Dividend Income for this portfolio item
    public decimal TotalDividendIncome => Dividends?.Sum(dividend => dividend.Amount) ?? 0;

    [NotMapped]
    public decimal? PreviousClosePrice { get; set; }

    [NotMapped]
    public decimal? OpenPrice { get; set; }

    [NotMapped]
    public decimal? LowPrice { get; set; }

    [NotMapped]
    public decimal? HighPrice { get; set; }

    [NotMapped]
    public decimal? PercentChange { get; set; }

    [NotMapped]
    public decimal? Change { get; set; }
    [NotMapped]
    public double? SentimentImpact { get; set; }
    [NotMapped]
    public decimal? AdjustedPrice { get; set; }

    [NotMapped]
    public SocialSentiment? SocialSentiment { get; set; }
    public IEnumerable<StockEvent>? StockEvents { get; set; }

    // Foreign key relationship to Portfolio
    public Portfolio? Portfolio { get; set; }
    [NotMapped]
    public string? SectorActivity { get;  set; }
  }
}
