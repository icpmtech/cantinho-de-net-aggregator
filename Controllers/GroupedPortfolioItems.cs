using AspnetCoreMvcFull.Models;
using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Models
{
  public class GroupedPortfolioItems
  {
    public string Symbol { get; set; }
    public decimal? TotalInvestment { get; set; }
    public decimal? CurrentMarketValue { get; set; }
    public decimal? TotalDifferenceValue { get; internal set; }
    public decimal? TotalDividends { get; internal set; }
    public decimal? TotalProfit { get; internal set; }
    public decimal? TotalDifferencePercentage { get; internal set; }
    public decimal? TotalProfitDifferencePercentage { get; internal set; }
    public decimal? TotalProfitWithDividends { get; internal set; }
    public List<PortfolioItem> Items { get; set; }
  }
}
