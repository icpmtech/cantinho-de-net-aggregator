namespace AspnetCoreMvcFull.Models
{
  public class PortfolioItemPercentage
  {
    public string Symbol { get; set; }
    public double Percentage { get; set; }
  }

  public class PortfolioPercentageResponse
  {
    public int PortfolioId { get; set; }
    public double TotalMarketValue { get; set; }
    public List<PortfolioItemPercentage> ItemPercentages { get; set; }
  }
}
