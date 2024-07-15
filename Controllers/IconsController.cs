using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Controllers;

public class IconsController : Controller
{
  public IActionResult Boxicons() => View();
}
