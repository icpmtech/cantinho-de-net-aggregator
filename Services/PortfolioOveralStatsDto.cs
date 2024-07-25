namespace MarketAnalyticHub.Services
{
  public class PortfolioOveralStatsDto
  {
    public decimal TotalMarketValue { get; set; }
    public decimal TotalCustMarketValue { get; set; }
    public decimal TotalDifferenceValue { get; set; }
    public decimal TotalDifferencePercentage { get; set; }
  }
}
