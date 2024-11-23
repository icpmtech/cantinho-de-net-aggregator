using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  public class DividendsTrackerController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
    public IActionResult DividendsHistory()
    {
      return View();
    }

  }
}
