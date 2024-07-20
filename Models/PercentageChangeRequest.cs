namespace AspnetCoreMvcFull.Models
{
  public class MarketValueRequest
  {
    public double CurrentMarketValue { get; set; }
    public double PercentageIncrease { get; set; }
  }

  public class MarketValueResponse
  {
    public double OriginalMarketValue { get; set; }
  }

  public class PercentageChangeRequest
  {
    public double OriginalMarketValue { get; set; }
    public double CurrentMarketValue { get; set; }
  }

  
}
