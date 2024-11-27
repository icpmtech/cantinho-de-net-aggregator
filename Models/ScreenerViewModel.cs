// File: Models/ScreenerViewModel.cs
using System.Collections.Generic;

namespace MarketAnalyticHub.Models
{
  public class ScreenerViewModel
  {
    public List<StockViewModel> Stocks { get; set; }
    public bool HasQuery { get; set; }
  }
}
