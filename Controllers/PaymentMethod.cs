

namespace MarketAnalyticHub.Models
{
  public class PaymentMethod
  {
    public int Id { get; set; }
    public string CardType { get; set; }
    public string CardName { get; set; }
    public string CardNumber { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsPrimary { get; set; }
    public int UserProfileId { get; set; }
    public virtual UserProfile UserProfile { get; set; }
  }



}
