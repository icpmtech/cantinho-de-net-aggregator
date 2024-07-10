namespace AspnetCoreMvcFull.Models.News
{
  public class NewsItem
  {
    public string Category { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }
    public string Link { get;  set; }
    public string? Author { get; internal set; }
  }
}
