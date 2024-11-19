namespace MarketAnalyticHub.Controllers.api
{
  public class ModelCustomerData
  {
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public Dictionary<string, string> RevenueByDivision { get; set; }
    public List<IndustryGrowth> RevenueGrowthByIndustry { get; set; }
    public CustomerPerformanceMetrics CustomerPerformanceMetrics { get; set; }
    public List<FinancialData> FinancialTable { get; set; } = new List<FinancialData>();

  }
  public class CompetitorData
  {
    public string CompanyName { get; set; }
    public string MarketCap { get; set; }
    public string Revenues { get; set; }
    public string Income { get; set; }
    public string Employees { get; set; }
  }
  public class ModelCompetitorData
  {
    public string Symbol { get; set; }
    public string CompanyName { get; set; }
    public string Sector { get; set; }
    public string Industry { get; set; }
    public List<CompetitorData> FinancialTable { get; set; }
    // You can add more properties as needed for other data points.
  }

  public class FinancialData
  {
    public string CompanyName { get; set; }
    public string MarketCap { get; set; }
    public string Revenues { get; set; }
    public string Income { get; set; }
    public string Employees { get; set; }
  }
  public class IndustryGrowth
  {
    public string Industry { get; set; }
    public string GrowthPercentage { get; set; }
  }

  public class CustomerPerformanceMetrics
  {
    public string NetIncomeGrowth { get; set; }
    public string NetMarginGrowth { get; set; }
  }
}
