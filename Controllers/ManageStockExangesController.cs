using MarketAnalyticHub.Models;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers
{
  [Route("managestockexanges")]
  public class ManageStockExangesViewController : Controller
  {

    public ManageStockExangesViewController()
    {
     
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      return View();
    }

  }
}
