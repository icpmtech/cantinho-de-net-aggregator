using System.Collections.Generic;

namespace MarketAnalyticHub.Models
{
  public class DividendsTracker
  {
    public int Id { get; set; }
    public string Company { get; set; }
    public string Ticker { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string Exchange { get; set; }
    public string SharePrice { get; set; }
    public string PrevDividend { get; set; }

    // Navigation property for the many-to-many relationship
    public List<DividendIndex> DividendIndices { get; set; } = new List<DividendIndex>();
  }

  public class IndexDividendsTracker
  {
    public int Id { get; set; }
    public string Region { get; set; }
    public List<string> Indices { get; set; }

    // Navigation property for the many-to-many relationship
    public List<DividendIndex> DividendIndices { get; set; } = new List<DividendIndex>();
  }

  public class DividendIndex
  {
    public int DividendsTrackerId { get; set; }
    public DividendsTracker DividendsTracker { get; set; }

    public int IndexDividendsTrackerId { get; set; }
    public IndexDividendsTracker IndexDividendsTracker { get; set; }
  }
}
