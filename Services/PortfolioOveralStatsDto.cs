using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Services
{
  public class PortfolioOveralStatsDto
  {
    public List<PortfolioItemPercentage> ItemPercentages { get; internal set; }
    public decimal? TotalDividendsPercentage { get; internal set; }
    public decimal? TotalDifferenceWithDividendsPercentage { get; internal set; }
    public decimal? TotalPortfolioProfit { get; internal set; }
    public decimal? TotalDifferencePercentage { get; internal set; }
    public decimal? TotalDifferenceValue { get; internal set; }
    public decimal? TotalCustMarketValue { get; internal set; }
    public decimal? TotalMarketValue { get; internal set; }
    public decimal? TotalDividends { get; internal set; }
  }
}
