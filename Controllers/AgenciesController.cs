using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  public class AgenciesController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult StockMarketRatingsNotations()
    {
      return View();
    }
  }
}
