namespace AspnetCoreMvcFull.Models.Configurations.News
{
  public class NewsScrapingItem
  {
    public int? Id { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }
    public string Link { get; set; }
    public string? Author { get; internal set; }
    public string? TemplateScraping { get; set; }
    public string TitleSelector { get; internal set; }
    public string LinkSelector { get; internal set; }
    public string DescriptionSelector { get; internal set; }
    public string AuthorSelector { get; internal set; }
    public string DateSelector { get; internal set; }
  }
}
