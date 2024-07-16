using AspnetCoreMvcFull.Models.Portfolio;

namespace MarketAnalyticHub.Models.SetupDb
{
  public class Dividend
  {
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public Portfolio Portfolio { get; set; }
  }
}
