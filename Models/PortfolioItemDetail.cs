namespace AspnetCoreMvcFull.Models
{
  public class PortfolioItemDetail
  {
    public string Symbol { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal CurrentMarketValue { get; set; }
    public decimal Dividends { get; set; }
    public int Id { get; internal set; }
  }
}
