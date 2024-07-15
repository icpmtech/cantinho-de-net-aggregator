using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;

namespace MarketAnalyticHub.Controllers;

public class TablesController : Controller
{
  public IActionResult Basic() => View();
}
