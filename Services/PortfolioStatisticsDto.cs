namespace MarketAnalyticHub.Services
{
  using System.Collections.Generic;

  public class PortfolioStatisticsDto
  {
    public double TotalValue { get; set; }
    public int TotalSymbols { get; set; }
    public List<PortfolioItemDto> Items { get; set; }
    public ChartDataDto ChartData { get; set; }
  }
}
