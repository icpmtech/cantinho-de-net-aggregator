namespace MarketAnalyticHub.Controllers.api
{
  public class InvestmentInput
  {
    public double Amount { get; set; }
    public string InvestmentType { get; set; }
    public string Market { get; set; }
    public int Duration { get; set; }
  }
}
