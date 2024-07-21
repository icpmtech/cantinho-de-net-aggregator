using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  public class SentimentAnalizerController : Controller
  {
    public IActionResult Index()
    {
      var stockEvents = new List<StockEvent>
            {
                new StockEvent
                {
                    Date = "July 21, 2024",
                    EventName = "Event 1",
                    Details = "Details of event 1",
                    Impact = "Positive",
                    Sentiment = "Positive",
                    Source = "https://example.com/source1"
                },
                new StockEvent
                {
                    Date = "July 20, 2024",
                    EventName = "Event 2",
                    Details = "Details of event 2",
                    Impact = "Negative",
                    Sentiment = "Negative",
                    Source = "https://example.com/source2"
                },
                new StockEvent
                {
                    Date = "July 19, 2024",
                    EventName = "Event 3",
                    Details = "Details of event 3",
                    Impact = "Positive",
                    Sentiment = "Positive",
                    Source = "https://example.com/source3"
                }
            };


      return View(stockEvents);
    }
  }
}
