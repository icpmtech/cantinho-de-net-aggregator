using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Areas.SupportCenter.Controllers
{
  [Area("SupportCenter")]
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
