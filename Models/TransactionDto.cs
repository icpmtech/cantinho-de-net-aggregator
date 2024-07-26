namespace MarketAnalyticHub.Models
{
  public class TransactionDto
  {
    public string Type { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public DateTime Date { get; set; }
    public string Source { get; set; }
  }
}
