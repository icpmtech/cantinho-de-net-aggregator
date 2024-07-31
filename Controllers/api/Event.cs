namespace MarketAnalyticHub.Controllers.api
{
  public class Event
  {
    public int Id { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string URL { get; set; }
    public string Location { get; set; }
    public string Guests { get; set; }
    public string Calendar { get; set; }
    public string Description { get; set; }
    public bool AllDay { get; set; }
    public ExtendedProps ExtendedProps { get;  set; }
  }
}
