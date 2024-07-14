using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Controllers.api;
using Microsoft.Extensions.Logging;
using AspnetCoreMvcFull.Models.SetupDb;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Services;

namespace AspnetCoreMvcFull.Controllers;
public class MarketsController : Controller
{
  private readonly MarketsService _MarketsService;
  private readonly ILogger<MarketsController> _logger;
  public MarketsController(MarketsService marketsService, ILogger<MarketsController> logger)
  {
    _MarketsService = marketsService;
    _logger = logger;
  }
  public IActionResult Blank() => View();
  public IActionResult Container() => View();
  public IActionResult Fluid() => View();
  public  async Task<IActionResult> Stocks(string category, string sortOrder, int pageNumber = 1, int pageSize = 50, string searchQuery = "") {

    var paginatedSymbols = await _MarketsService.GetPaginatedSymbolsAsync(category, sortOrder, pageNumber, pageSize, searchQuery);
    ViewBag.PageSize = pageSize; // Pass pageSize to view
    ViewBag.SearchQuery = searchQuery; // Pass searchQuery to view
    return View(paginatedSymbols);
  }

  [HttpGet("get/{id}")]
  public async Task<IActionResult> GetSymbolsItem(int id)
  {
    var SymbolsItem = await _MarketsService.GetSymbolsByIdAsync(id);
    if (SymbolsItem == null)
    {
      return NotFound();
    }
    return Json(SymbolsItem);
  }

  public IActionResult Reddit() => View();
  public IActionResult Linkedin() => View();

  public IActionResult Facebook() => View();

  public IActionResult HorizontalMenu() => View();
  public IActionResult WithoutMenu() => View();
  public IActionResult WithoutNavbar() => View();
}
