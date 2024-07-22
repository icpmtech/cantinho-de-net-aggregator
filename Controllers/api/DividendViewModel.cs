namespace MarketAnalyticHub.Controllers.api
{
  public class DividendViewModel
  {
    public string Symbol { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public int PortfolioItemId { get;  set; }
    public int Id { get;  set; }
  }
}
