using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Services
{
  public class StockFinantialsViewModel 
  {

    public string Symbol { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal? TargetHighPrice { get; set; }
    public decimal? TargetLowPrice { get; set; }
    public decimal? TargetMeanPrice { get; set; }
    public decimal? TargetMedianPrice { get; set; }
    public decimal? RecommendationMean { get; set; }
    public string RecommendationKey { get; set; }
    public int? NumberOfAnalystOpinions { get; set; }
    public decimal? TotalCash { get; set; }
    public decimal? TotalCashPerShare { get; set; }
    public decimal? Ebitda { get; set; }
    public decimal? TotalDebt { get; set; }
    public decimal? QuickRatio { get; set; }
    public decimal? CurrentRatio { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? DebtToEquity { get; set; }
    public decimal? RevenuePerShare { get; set; }
    public decimal? ReturnOnAssets { get; set; }
    public decimal? ReturnOnEquity { get; set; }
    public decimal? FreeCashflow { get; set; }
    public decimal? OperatingCashflow { get; set; }
    public decimal? EarningsGrowth { get; set; }
    public decimal? RevenueGrowth { get; set; }
    public decimal? GrossMargins { get; set; }
    public decimal? EbitdaMargins { get; set; }
    public decimal? OperatingMargins { get; set; }
    public decimal? ProfitMargins { get; set; }
    public string FinancialCurrency { get; set; }
    // If you need 'grossProfits', adjust the type accordingly
    public decimal? GrossProfits { get; set; }
    // Add more properties as needed
  }
}
