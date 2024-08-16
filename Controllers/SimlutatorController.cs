using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{
  public class PortfolioBuilderController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
