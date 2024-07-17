namespace MarketAnalyticHub.Controllers.api
{
  public class PortfolioStatistic
  {
    public string Name { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal CurrentMarketValue { get; set; }
    public List<PortfolioItemDetail> Items { get; set; }
  }


}
