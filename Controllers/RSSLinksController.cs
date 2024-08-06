using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  public class RSSLinksController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
