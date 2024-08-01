using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Services
{
  public class TotalPortfolioPercentageResponse
  {
    public int PortfolioId { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal TotalCustMarketValue { get; set; }
    public decimal TotalPortfolioProfit { get; set; }
    public decimal TotalDifferenceValue { get; set; }
    public decimal TotalDifferenceWithDividendsPercentage { get; set; }
    public decimal TotalDifferencePercentage { get; set; }
    public decimal TotalDividendsPercentage { get; set; }
    public decimal TotalDividends { get; internal set; }
    public IEnumerable<PortfolioItemPercentage> ItemPercentages { get; internal set; }
  }
}
