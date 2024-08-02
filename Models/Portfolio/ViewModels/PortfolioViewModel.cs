using MarketAnalyticHub.Models.Portfolio.ViewModels;

namespace MarketAnalyticHub.Models.Portfolio.ViewModels
{
  public class PortfolioViewModel
  {

    public PortfolioViewModel()
    {
      Items = new List<PortfolioItemViewModel>(); // Initialize the Items collection
    }

    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public ICollection<PortfolioItemViewModel> Items { get; set; }

    // Calculated fields

    public decimal TotalInvestment => Items?.Sum(item => item.TotalInvestment) ?? 0;

    public decimal CurrentMarketValue => Items?.Sum(item => item.CurrentMarketValue) ?? 0;

    public decimal TotalGainsLosses => CurrentMarketValue - TotalInvestment;

    // Total Dividend Income for the entire portfolio

    public decimal TotalDividendIncome => Items?.Sum(item => item.TotalDividendIncome) ?? 0;


    public PortfolioPercentageResponseViewModel PortfolioPercentageResponse { get; internal set; }

    public decimal PortfolioPercentage { get; internal set; }

    public dynamic GroupedItems { get; set; }

    public DateTime? CreationDate { get; private set; }

  }
}

public class PortfolioItemViewModel
  {
  public string Status { get; set; }
  public int Id { get; set; }
    public string OperationType { get; set; }
    public string UserId { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Shares { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? Commission { get; set; }

  public decimal? LastPrice { get; set; }

  public decimal? TodaysGain { get; set; }

  public decimal? TotalGain =>Shares * CurrentPrice - Commission ?? 0;
  public decimal? AverageCost { get; set; }
  public decimal? TotalReturn { get; set; }
  
      

  // Navigation property for dividends
  public ICollection<DividendViewModel>? Dividends { get; set; }

    // Calculated fields
    public decimal TotalInvestment => Shares * PurchasePrice + Commission ?? 0;
    public decimal CurrentMarketValue => Shares * CurrentPrice;

    // Total Dividend Income for this portfolio item
    public decimal TotalDividendIncome => Dividends?.Sum(dividend => dividend.Amount) ?? 0;


    public decimal? PreviousClosePrice { get; set; }


    public decimal? OpenPrice { get; set; }


    public decimal? LowPrice { get; set; }


    public decimal? HighPrice { get; set; }


    public decimal? PercentChange { get; set; }


    public decimal? Change { get; set; }

    public double? SentimentImpact { get; set; }

    public decimal? AdjustedPrice { get; set; }


    public SocialSentimentViewModel? SocialSentiment { get; set; }
    public IEnumerable<StockEventViewModel>? StockEvents { get; set; }




  
}
