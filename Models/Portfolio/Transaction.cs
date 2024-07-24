using MarketAnalyticHub.Models.Portfolio;

namespace MarketAnalyticHub.Models.Portfolio
{
  public class Transaction
  {
    public int Id { get; set; }
    public int PortfolioItemId { get; set; }
    public PortfolioItem PortfolioItem { get; set; }
    public int Quantity { get; set; }
    public decimal SellPrice { get; set; }
    public DateTime SellDate { get; set; }
    public decimal? Commission { get; set; } // New field for commission
    public decimal? Revenue { get; set; } // New field for revenue
  }

}
