namespace MarketAnalyticHub.Models.Dashboard
{
  public class DashboardData
  {
    public decimal Profit { get; set; }
    public decimal Dividends { get; set; }
    public decimal Payments { get; set; }
    public decimal Operations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal Growth { get; set; }
    public decimal PortfolioGrowth { get; set; }
    public decimal YearlyReport { get; set; }
    public double ProfitPercentage { get; internal set; }
  }

}
