using AspnetCoreMvcFull.Models;
using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarketAnalyticHub.Controllers.api
{
  [Authorize]
  public class PortfolioIndexingController : Controller
  {
    private readonly PortfolioIndexingService _indexingService;

    public PortfolioIndexingController(PortfolioIndexingService indexingService)
    {
      _indexingService = indexingService;
    }

    
      public async Task<IActionResult> IndexPortfolioData()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (userId == null)
      {
        return Unauthorized();
      }
      await _indexingService.IndexPortfolioDataAsync(userId);
      return Ok("Indexing completed");
    }

    // GET: PortfolioIndexing
    [HttpGet]
      public async Task<IActionResult> Index()
      {
        var indices = await _indexingService.GetIndicesAsync();
        return View(indices);
      }

      // GET: PortfolioIndexing/Create
      [HttpGet]
      public IActionResult Create()
      {
        return View();
      }

      // POST: PortfolioIndexing/Create
      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> Create(string indexName)
      {
        if (string.IsNullOrEmpty(indexName))
        {
          ModelState.AddModelError("", "Index name cannot be empty.");
          return View();
        }

        var created = await _indexingService.CreateIndexAsync(indexName);
        if (created)
        {
          return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to create index.");
        return View();
      }

      // GET: PortfolioIndexing/Details/{indexName}
      [HttpGet]
      public async Task<IActionResult> Details(string indexName)
      {
        if (string.IsNullOrEmpty(indexName))
        {
          return BadRequest();
        }

        var indexDetails = await _indexingService.GetIndexDetailsAsync(indexName);
        if (indexDetails == null)
        {
          return NotFound();
        }

        return View(indexDetails);
      }

    // GET: PortfolioIndexing/Delete/{indexName}
    [HttpGet]
    public async Task<IActionResult> Delete(string indexName)
    {
      if (string.IsNullOrEmpty(indexName))
      {
        return BadRequest("Index name cannot be null or empty.");
      }

      var indexDetails = await _indexingService.GetIndexDetailsAsync(indexName);
      if (indexDetails == null)
      {
        return NotFound($"Index '{indexName}' not found.");
      }

      var viewModel = new IndexViewModel
      {
        IndexName = indexName,
        IndexState = indexDetails
      };

      return View(viewModel);
    }


    // POST: PortfolioIndexing/DeleteConfirmed/{indexName}
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string indexName)
    {
      if (string.IsNullOrEmpty(indexName))
      {
        return BadRequest("Index name cannot be null or empty.");
      }

      var deleted = await _indexingService.DeleteIndexAsync(indexName);
      if (deleted)
      {
        TempData["SuccessMessage"] = $"Index '{indexName}' deleted successfully.";
        return RedirectToAction(nameof(Index));
      }

      TempData["ErrorMessage"] = $"Failed to delete index '{indexName}'. Please try again.";
      return RedirectToAction(nameof(Delete), new { indexName });
    }
  }
  }
