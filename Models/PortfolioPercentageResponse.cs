namespace AspnetCoreMvcFull.Models
{
  public class PortfolioPercentageResponse
  {
    public int PortfolioId { get; set; }
    public double TotalMarketValue { get; set; }
    public double TotalCustMarketValue { get; set; }
    public double TotalDifferenceValue { get; set; }
    public double TotalDifferencePercentage { get; set; }
    public List<PortfolioItemPercentage> ItemPercentages { get; set; }
  }

  public class PortfolioItemPercentage
  {
    public string Symbol { get; set; }
    public double CurrentPercentage { get; set; }
    public double CustomPercentage { get; set; }
    public double DifferenceValue { get; set; }
    public double DifferencePercentage { get; set; }
  }
}
