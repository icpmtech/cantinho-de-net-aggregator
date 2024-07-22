using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models.Portfolio;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarketAnalyticHub.Services;

namespace MarketAnalyticHub.Controllers
{
  [Authorize]
  public class DashboardsController : Controller
  {

    public DashboardsController()
    {
    }

    public async Task<IActionResult> Index()
    {
     
      return View();
    }

    
  }
}
