using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.Portfolio;
using MarketAnalyticHub.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace MarketAnalyticHub.Models.Portfolio
{
  public class Portfolio
  {
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public ICollection<Models.Portfolio.PortfolioItem> Items { get; set; }

    // Calculated fields
    public decimal TotalInvestment => Items?.Sum(item => item.TotalInvestment) ?? 0;
    public decimal CurrentMarketValue => Items?.Sum(item => item.CurrentMarketValue) ?? 0;
    public decimal TotalGainsLosses => CurrentMarketValue - TotalInvestment;

    // Total Dividend Income for the entire portfolio
    public decimal TotalDividendIncome => Items?.Sum(item => item.TotalDividendIncome) ?? 0;
    [NotMapped]
    public PortfolioPercentageResponse PortfolioPercentageResponse { get; internal set; }
    [NotMapped]
    public double PortfolioPercentage { get; internal set; }
    [NotMapped]
    public dynamic GroupedItems { get; set; }
  }
}
