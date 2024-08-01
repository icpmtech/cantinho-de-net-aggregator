namespace MarketAnalyticHub.Models
{
  public class PortfolioPercentageResponse
  {
    public int PortfolioId { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal TotalCustMarketValue { get; set; }
    public decimal TotalDifferenceValue { get; set; }
    public decimal TotalDifferencePercentage { get; set; }
    public List<PortfolioItemPercentage> ItemPercentages { get; set; }
    public decimal TotalDifferenceWithDividendsPercentage { get; internal set; }
    public decimal TotalPortfolioProfit { get; internal set; }
    public decimal TotalDividendsPercentage { get; internal set; }
  }

  public class PortfolioItemPercentage
  {
    public string Symbol { get; set; }
    public decimal CurrentPercentage { get; set; }
    public decimal CustomPercentage { get; set; }
    public decimal DifferenceValue { get; set; }
    public decimal DifferencePercentage { get; set; }
    public decimal CurrentWithDividendsPercentage { get; internal set; }
    public decimal CustomWithDividendsPercentage { get; internal set; }
    public decimal DifferenceWithDividendsPercentage { get; internal set; }
    public decimal DifferenceWithDividendsValue { get; internal set; }
  }
}
