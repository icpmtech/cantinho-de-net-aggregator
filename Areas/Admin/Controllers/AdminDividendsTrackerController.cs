using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using MarketAnalyticHub.Models.SetupDb;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize]
  public class AdminDividendsTrackerController : Controller
  {
    private readonly ApplicationDbContext _context;

    public AdminDividendsTrackerController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: Admin/DividendsTracker
    public async Task<IActionResult> Index()
    {
      var dividends = _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker);
      return View(await dividends.ToListAsync());
    }

    // GET: Admin/DividendsTracker/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (dividend == null)
      {
        return NotFound();
      }

      return View(dividend);
    }

    // GET: Admin/DividendsTracker/Create
    public IActionResult Create()
    {
      ViewData["IndexIds"] = new SelectList(_context.IndexDividendsTrackers, "Id", "Region");
      return View();
    }

    // POST: Admin/DividendsTracker/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Company,Ticker,Country,Region,Exchange,SharePrice,PrevDividend")] DividendsTracker dividend, int[] selectedIndices)
    {
      if (ModelState.IsValid)
      {
        foreach (var indexId in selectedIndices)
        {
          var index = await _context.IndexDividendsTrackers.FindAsync(indexId);
          if (index != null)
          {
            dividend.DividendIndices.Add(new DividendIndex
            {
              DividendsTracker = dividend,
              IndexDividendsTracker = index
            });
          }
        }

        _context.Add(dividend);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      ViewData["IndexIds"] = new SelectList(_context.IndexDividendsTrackers, "Id", "Region", selectedIndices);
      return View(dividend);
    }

    // GET: Admin/DividendsTracker/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker)
          .FirstOrDefaultAsync(d => d.Id == id);

      if (dividend == null)
      {
        return NotFound();
      }

      ViewData["IndexIds"] = new SelectList(_context.IndexDividendsTrackers, "Id", "Region", dividend.DividendIndices.Select(di => di.IndexDividendsTrackerId));
      return View(dividend);
    }

    // POST: Admin/DividendsTracker/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Company,Ticker,Country,Region,Exchange,SharePrice,PrevDividend")] DividendsTracker dividend, int[] selectedIndices)
    {
      if (id != dividend.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          // Update the dividend details
          _context.Update(dividend);

          // Remove existing associations
          var existingIndices = _context.DividendIndices
              .Where(di => di.DividendsTrackerId == id)
              .ToList();

          _context.DividendIndices.RemoveRange(existingIndices);

          // Add new associations
          foreach (var indexId in selectedIndices)
          {
            var index = await _context.IndexDividendsTrackers.FindAsync(indexId);
            if (index != null)
            {
              dividend.DividendIndices.Add(new DividendIndex
              {
                DividendsTrackerId = dividend.Id,
                IndexDividendsTrackerId = indexId
              });
            }
          }

          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!DividendsTrackerExists(dividend.Id))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        return RedirectToAction(nameof(Index));
      }

      ViewData["IndexIds"] = new SelectList(_context.IndexDividendsTrackers, "Id", "Region", selectedIndices);
      return View(dividend);
    }

    // GET: Admin/DividendsTracker/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .ThenInclude(di => di.IndexDividendsTracker)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (dividend == null)
      {
        return NotFound();
      }

      return View(dividend);
    }

    // POST: Admin/DividendsTracker/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var dividend = await _context.DividendsTrackers
          .Include(d => d.DividendIndices)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (dividend != null)
      {
        _context.DividendsTrackers.Remove(dividend);
        await _context.SaveChangesAsync();
      }

      return RedirectToAction(nameof(Index));
    }

    private bool DividendsTrackerExists(int id)
    {
      return _context.DividendsTrackers.Any(e => e.Id == id);
    }
  }
}
