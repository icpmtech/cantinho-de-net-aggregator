using MarketAnalyticHub.Models.Dashboard;
using MarketAnalyticHub.Models.Portfolio;

namespace MarketAnalyticHub.Models
{
  public class DashboardViewModel
  {
    public List<TotalRevenueByYearDto> TotalRevenueByYear { get; set; }
    public DashboardData DashboardData { get; set; }
    public List<AmountTotalYearDto> AmountTotalYear { get; set; }
    public List<Portfolio.Portfolio> ProfileReportCurrentYear { get; set; }
    public decimal PortfolioGrowthPercentage { get; internal set; }
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
