namespace MarketAnalyticHub.Controllers
{
  public class PaymentMethod
  {
    public string CardType { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsPrimary { get; set; }
  }



}
