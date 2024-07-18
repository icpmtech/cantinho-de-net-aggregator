using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models.Portfolio
{
  public class PortfolioItem
  {
    public int Id { get; set; }
    public int PortfolioId { get; set; }
    public string Symbol { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }

    // Navigation property for dividends
    public ICollection<Dividend>? Dividends { get; set; }

    // Calculated fields
    public decimal TotalInvestment => Quantity * PurchasePrice;
    public decimal CurrentMarketValue => Quantity * CurrentPrice;

    // Total Dividend Income for this portfolio item
    public decimal TotalDividendIncome => Dividends?.Sum(dividend => dividend.Amount) ?? 0;

    [NotMapped]
    public decimal? PreviousClosePrice { get; set; }

    [NotMapped]
    public decimal? OpenPrice { get; set; }

    [NotMapped]
    public decimal? LowPrice { get; set; }

    [NotMapped]
    public decimal? HighPrice { get; set; }

    [NotMapped]
    public decimal? PercentChange { get; set; }

    [NotMapped]
    public decimal? Change { get; set; }
  }
}
