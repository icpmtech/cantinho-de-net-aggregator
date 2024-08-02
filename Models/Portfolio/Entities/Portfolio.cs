using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Models.Portfolio
{
  public partial class Portfolio 
  {
    public Portfolio()
    {
      CreationDate = DateTime.Now; // Set the creation date to the current date and time
      Items = new List<PortfolioItem>(); // Initialize the Items collection
    }

    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public ICollection<PortfolioItem> Items { get; set; }

    // Calculated fields
    [NotMapped]
    public decimal TotalInvestment => Items?.Sum(item => item.TotalInvestment) ?? 0;
    [NotMapped]
    public decimal CurrentMarketValue => Items?.Sum(item => item.CurrentMarketValue) ?? 0;
    [NotMapped]
    public decimal TotalGainsLosses => CurrentMarketValue - TotalInvestment;

    // Total Dividend Income for the entire portfolio
    [NotMapped]
    public decimal TotalDividendIncome => Items?.Sum(item => item.TotalDividendIncome) ?? 0;

    [NotMapped]
    public PortfolioPercentageResponse PortfolioPercentageResponse { get; internal set; }
    [NotMapped]
    public decimal PortfolioPercentage { get; internal set; }
    [NotMapped]
    public dynamic GroupedItems { get; set; }

    public DateTime? CreationDate { get; private set; }
  }
}
