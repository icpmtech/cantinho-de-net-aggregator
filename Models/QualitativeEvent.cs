namespace AspnetCoreMvcFull.Models
{
  public class QualitativeEvent
  {
    public int Id { get; set; }
    public string Symbol { get; set; }
    public string EventDescription { get; set; }
    public DateTime EventDate { get; set; }
    public List<MarketAnalyticHub.Models.News.NewsItem> News { get; set; }
  }
}
