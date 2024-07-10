using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Services;

namespace AspnetCoreMvcFull.Controllers;

public class SourcesController : Controller
{
  private readonly NewsService _newsService;
  public SourcesController(NewsService newsService)
  {
    _newsService = newsService;
  }
  public IActionResult Blank() => View();
  public IActionResult Container() => View();
  public IActionResult Fluid() => View();
  public  async Task<IActionResult> GoogleNews() {

    var news = await _newsService.GetNewsAsync();
    return View(news);
  } 
  public IActionResult HorizontalMenu() => View();
  public IActionResult WithoutMenu() => View();
  public IActionResult WithoutNavbar() => View();
}
