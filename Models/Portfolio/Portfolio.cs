using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Portfolio;
using System.ComponentModel.DataAnnotations.Schema;

public class Portfolio
{
  public int Id { get; set; }
  public string UserId { get; set; }
  public string Name { get; set; }
  public ICollection<PortfolioItem> Items { get; set; }

  // Calculated fields
  public decimal TotalInvestment => Items?.Sum(item => item.TotalInvestment) ?? 0;
  public decimal CurrentMarketValue => Items?.Sum(item => item.CurrentMarketValue) ?? 0;
  public decimal TotalGainsLosses => CurrentMarketValue - TotalInvestment;

  // Total Dividend Income for the entire portfolio
  public decimal TotalDividendIncome => Items?.Sum(item => item.TotalDividendIncome) ?? 0;
  [NotMapped]
  public PortfolioPercentageResponse PortfolioPercentageResponse { get; internal set; }
  [NotMapped]
  public double PortfolioPercentage { get; internal set; }
}
