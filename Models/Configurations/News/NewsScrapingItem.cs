namespace MarketAnalyticHub.Models.Configurations.News
{
  public class NewsScrapingItem
  {
    public int? Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Date { get; set; }
    public string Link { get; set; }
    public string? Author { get;  set; }
    public string? TemplateScraping { get; set; }
    public string? TitleSelector { get;  set; }
    public string? LinkSelector { get;  set; }
    public string? DescriptionSelector { get;  set; }
    public string? AuthorSelector { get;  set; }
    public string? DateSelector { get;  set; }
  }
}
