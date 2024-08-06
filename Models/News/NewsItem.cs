using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Models.News
{
  public class NewsItem
  {
    public int? Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Date { get; set; }
    public string Link { get;  set; }
    public string? Author { get;  set; }
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public double? Sentiment { get; set; }
    public List<string>? Keywords { get;  set; }
    public string? SentimentImpact { get;  set; }
    public string? IndustriesImpact { get; internal set; }
    public string? Summary { get; internal set; }
  }
}
