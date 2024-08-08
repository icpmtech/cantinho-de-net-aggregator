using MarketAnalyticHub.Models.Portfolio.Entities;

namespace MarketAnalyticHub.Controllers.api
{
  public class EventModelView
  {
    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string EventName { get; set; }
    public string Details { get; set; }
    public string Impact { get; set; }
    public string Sentiment { get; set; }
    public string Source { get; set; }

    public decimal? Price { get; set; }
    public decimal? PriceChange { get; set; }
    // Foreign key property
    public int PortfolioItemId { get; set; }

    // Navigation property
    public string? Url { get; internal set; }
    public string? Title { get; internal set; }
    public DateTime? End { get; internal set; }
    public DateTime? Start { get; internal set; }
    public bool? AllDay { get; internal set; }
    public string? Calendar { get; internal set; }
    public string? Location { get; internal set; }
    public string? Guests { get; internal set; }
    public string? Description { get; internal set; }

    public int? Score { get; internal set; }
    public string? SummaryAnalisys { get; internal set; }
  }
}
