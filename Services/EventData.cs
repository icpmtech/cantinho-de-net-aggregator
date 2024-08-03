using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Services
{
  public static class EventData
  {
    public static List<Event> GetSampleEvents()
    {
      var currentDate = DateTime.Now;
      var nextDay = currentDate.AddDays(1);
      var nextMonth = DateTime.Now.AddMonths(1);
      var prevMonth = DateTime.Now.AddMonths(-1);

      return new List<Event>
            {
                new Event
                {
                    Id = 1,
                    Url = "",
                    Title = "Design Review",
                    Start = currentDate,
                    End = nextDay,
                    AllDay = false,
                    ExtendedProps = new ExtendedProps { Calendar = "Business" }
                },
               
            };
    }
  }
}
