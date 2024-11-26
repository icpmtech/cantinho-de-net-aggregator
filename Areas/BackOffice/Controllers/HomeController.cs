using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Areas.BackOffice.Controllers
{
  [Area("BackOffice")]
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
