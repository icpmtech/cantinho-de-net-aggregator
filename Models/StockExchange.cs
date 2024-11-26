using System.ComponentModel.DataAnnotations;

namespace MarketAnalyticHub.Models
{
  public class StockExchange
  {

    public int Year { get; set; }
    [Key]
    public string StockExchangeName { get; set; }
    public string MIC { get; set; }
    public string Region { get; set; }
    public string City { get; set; }
    public double? MarketCapUsdTrillion { get; set; }
    public double? MonthlyTradeVolumeUsdBillion { get; set; }
    public string TimeZone { get; set; }
    public double UtcOffset { get; set; }
    public string DST { get; set; }

    public int OpenHoursLocalId { get; set; }
    public OpenHours OpenHoursLocal { get; set; }

    public int OpenHoursUTCId { get; set; }
    public OpenHours OpenHoursUTC { get; set; }
  }


  public class OpenHours
  {
    public int Id { get; set; } // Add primary key
    public string Open { get; set; }
    public string Close { get; set; }
    public string Lunch { get; set; }
  }
}
