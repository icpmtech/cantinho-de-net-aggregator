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
    public string? Author { get; internal set; }
    public string? Content { get; set; }
    public DateTime? PublishedDate { get; set; }
    public double? Sentiment { get; set; }
  }
}
