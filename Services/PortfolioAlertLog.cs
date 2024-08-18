
namespace MarketAnalyticHub.Models
{
  public class PortfolioAlertLog
  {
    public int Id { get; set; }
    public string UserId { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal LossPercentage { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; internal set; } = false;
  }
}
