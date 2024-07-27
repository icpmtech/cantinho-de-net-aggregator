using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MarketAnalyticHub.Models.Portfolio
{
  public class Portfolio
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
    public IDictionary<DateTime, MonthlyPortfolioSummary> CalculateMonthlySummaries()
    {
      var monthlySummaries = Items
          .GroupBy(item => new DateTime(item.PurchaseDate.Year, item.PurchaseDate.Month, 1))
          .ToDictionary(
              g => g.Key,
              g => new MonthlyPortfolioSummary
              {
                TotalInvestment = g.Sum(item => item.TotalInvestment),
                CurrentMarketValue = g.Sum(item => item.CurrentMarketValue),
                TotalDividendIncome = g.Sum(item => item.TotalDividendIncome)
              });

      return monthlySummaries;
    }
    public class MonthlyPortfolioSummary
    {
      public decimal TotalInvestment { get; set; }
      public decimal CurrentMarketValue { get; set; }
      public decimal TotalDividendIncome { get; set; }
    }
  }
}
