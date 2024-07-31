namespace MarketAnalyticHub.Models.Portfolio
{
  public partial class Portfolio
  {
    public class WeeklyPortfolioSummary
    {
      public decimal TotalInvestment { get; set; }
      public decimal CurrentMarketValue { get; set; }
      public decimal TotalDividendIncome { get; set; }
    }
  }
}
