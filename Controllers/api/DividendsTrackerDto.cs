namespace MarketAnalyticHub.Controllers.api
{
  public class DividendsTrackerDto
  {
    public string Company { get; set; }
    public string Ticker { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Exchange { get; set; }
    public string SharePrice { get; set; }
    public string PrevDividend { get; set; }
    public int[] SelectedIndices { get; set; }
    public DateTime? ExDateDividend { get; set; }
    public DateTime? PayDateDividend { get; set; }
  }

}
