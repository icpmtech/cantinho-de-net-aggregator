using MarketAnalyticHub.Models.Dashboard;
using MarketAnalyticHub.Models.Portfolio;

namespace MarketAnalyticHub.Models
{
  public class DashboardViewModel
  {
    public List<TransactionDto> Transactions { get; set; }
    public List<TotalRevenueByYearDto> TotalRevenueByYear { get; set; }
    public DashboardData DashboardData { get; set; }
    public List<AmountTotalYearDto> AmountTotalYear { get; set; }
    public List<Portfolio.Portfolio> ProfileReportCurrentYear { get; set; }
    public decimal PortfolioGrowthPercentage { get; internal set; }
    public Dictionary<string, RealTimeDataDto> RealTimeData { get; set; }
  }

  public class RealTimeDataDto
  {
    public string Symbol { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal Change { get; set; }
    public decimal PercentChange { get; set; }
  }

  public class TotalRevenueByYearDto
  {
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
  }

  public class AmountTotalYearDto
  {
    public int Year { get; set; }
    public decimal TotalInvestment { get; set; }
  }

}
