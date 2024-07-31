namespace MarketAnalyticHub.Models.Dashboard
{
  public class DashboardData
  {
    public decimal Profit { get; set; }
    public int? TotalEvents { get; set; } = 0;
    
    public decimal? WeekPercentage { get; set; }
    public decimal? TotalEventsPositivePercentage { get; set; } = 0;
    public decimal? TotalEventsNegativePercentage { get; set; } = 0;
    public decimal Dividends { get; set; }
    public int? Payments { get; set; }
    public int? Operations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Growth { get; set; }
    public decimal PortfolioGrowth { get; set; }
    public decimal YearlyReport { get; set; }
    public decimal ProfitPercentage { get; internal set; }
  }

}
