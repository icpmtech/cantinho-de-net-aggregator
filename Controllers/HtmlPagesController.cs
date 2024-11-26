using Ganss.Xss;
using MarketAnalyticHub.Models;
using MarketAnalyticHub.Models.SetupDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Controllers
{

  public class HtmlPagesController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HtmlPagesController> _logger;
    private readonly IWebHostEnvironment _environment;

    public HtmlPagesController(ApplicationDbContext context, ILogger<HtmlPagesController> logger, IWebHostEnvironment environment)
    {
      _context = context;
      _logger = logger;
      _environment = environment;
    }


    // POST: HtmlPages/Publish/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
      if (id == 0)
      {
        _logger.LogWarning("Publish action called with invalid id.");
        return BadRequest();
      }

      var htmlPage = await _context.HtmlPages.FindAsync(id);
      if (htmlPage == null)
      {
        _logger.LogWarning($"Publish action: HtmlPage with id {id} not found.");
        return NotFound();
      }

      // Define the path where the static HTML file will be saved
      string pagesDirectory = Path.Combine(_environment.WebRootPath, "HelpeCenter", "Pages");

      // Ensure the directory exists
      if (!Directory.Exists(pagesDirectory))
      {
        Directory.CreateDirectory(pagesDirectory);
        _logger.LogInformation($"Created directory at {pagesDirectory}.");
      }

      // Sanitize the slug to prevent directory traversal attacks
      string safeSlug = Path.GetFileName(htmlPage.Slug);

      // Define the full file path
      string filePath = Path.Combine(pagesDirectory, $"{safeSlug}.html");

      try
      {
        // Optionally, sanitize the HTML content here to prevent XSS
        // For example, using HtmlSanitizer library
         var sanitizer = new HtmlSanitizer();
         string sanitizedContent = sanitizer.Sanitize(htmlPage.Content);

        // For simplicity, we'll assume the content is safe
        await System.IO.File.WriteAllTextAsync(filePath, htmlPage.Content);

        _logger.LogInformation($"Published HtmlPage with id {id} to {filePath}.");

        // Optionally, add a success message
        TempData["SuccessMessage"] = "Page published successfully.";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error publishing HtmlPage with id {id}.");
        // Optionally, add an error message
        TempData["ErrorMessage"] = "An error occurred while publishing the page.";
        return RedirectToAction(nameof(Index));
      }

      return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchString, int? pageNumber)
    {
      int pageSize = 10; // Number of items per page

      var pages = from p in _context.HtmlPages
                  select p;

      if (!String.IsNullOrEmpty(searchString))
      {
        pages = pages.Where(p => p.Title.Contains(searchString) || p.Slug.Contains(searchString));
      }

      // Order by CreatedAt descending
      pages = pages.OrderByDescending(p => p.CreatedAt);

      // Implement pagination
      return View(await PaginatedList<HtmlPage>.CreateAsync(pages.AsNoTracking(), pageNumber ?? 1, pageSize));
    }
    // GET: HtmlPages/Create
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // POST: HtmlPages/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Slug,Content,MetaTitle,MetaDescription,Keywords")] HtmlPage htmlPage)
    {
      if (ModelState.IsValid)
      {
        // Assign the current user as the creator
        var currentUser = User.Identity.Name;
        htmlPage.LastEditedBy = currentUser;
        htmlPage.ChangeHistory = $"Page created by {currentUser} on {DateTime.UtcNow}.";

        _context.Add(htmlPage);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }
      return View(htmlPage);
    }
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        _logger.LogWarning("Edit action called with null id.");
        return BadRequest();
      }

      var htmlPage = await _context.HtmlPages.FindAsync(id);
      if (htmlPage == null)
      {
        _logger.LogWarning($"Edit action: HtmlPage with id {id} not found.");
        return NotFound();
      }

      return View(htmlPage);
    }

    // POST: HtmlPages/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Slug,Content,MetaTitle,MetaDescription,Keywords")] HtmlPage htmlPage)
    {
      if (id != htmlPage.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        try
        {
          // Assign the current user as the last editor
          var currentUser = User.Identity.Name;
          htmlPage.UpdatedAt = DateTime.UtcNow;
          htmlPage.LastEditedBy = currentUser;
          htmlPage.ChangeHistory += $"\nPage edited by {currentUser} on {DateTime.UtcNow}.";

          _context.Update(htmlPage);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!HtmlPageExists(htmlPage.Id))
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
      return View(htmlPage);
    }

    private bool HtmlPageExists(int id)
    {
      return _context.HtmlPages.Any(e => e.Id == id);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return BadRequest(); // Returns 400 Bad Request
      }

      var htmlPage = await _context.HtmlPages
          .FirstOrDefaultAsync(m => m.Id == id);
      if (htmlPage == null)
      {
        return NotFound(); // Returns 404 Not Found
      }

      return View(htmlPage);
    }


    // GET: HtmlPages/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var htmlPage = await _context.HtmlPages
          .FirstOrDefaultAsync(m => m.Id == id);
      if (htmlPage == null)
      {
        return NotFound();
      }

      return PartialView("_DeleteConfirmation", htmlPage);
    }

    // POST: HtmlPages/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var htmlPage = await _context.HtmlPages.FindAsync(id);
      _context.HtmlPages.Remove(htmlPage);
      await _context.SaveChangesAsync();
      return Json(new { success = true });
    }
  }
  }
