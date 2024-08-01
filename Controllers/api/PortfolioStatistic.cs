using AspnetCoreMvcFull.Models;

namespace MarketAnalyticHub.Controllers.api
{
  public class PortfolioStatistic
  {
    public string Name { get; set; }
    public decimal? TotalInvestment { get; set; }
    public decimal? CurrentMarketValue { get; set; }
    public List<PortfolioItemDetail> Items { get; set; }
    public decimal? TotalDifferenceValue { get; internal set; }
    public decimal? TotalDividends { get; internal set; }
    public decimal? TotalProfit { get; internal set; }
    public decimal? TotalDifferencePercentage { get; internal set; }
    public decimal? TotalProfitDifferencePercentage { get; internal set; }
  }


}
