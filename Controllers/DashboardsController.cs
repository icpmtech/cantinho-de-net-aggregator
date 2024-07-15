using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;

namespace MarketAnalyticHub.Controllers;


public class DashboardsController : Controller
{
  public IActionResult Index() => View();
}
