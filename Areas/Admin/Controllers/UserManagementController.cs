using MarketAnalyticHub.Models.SetupDb;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize]
  public partial class UserManagementController : Controller
  {
   
      private readonly ApplicationDbContext _context;
      private readonly UserManager<ApplicationUser> _userManager;

      public UserManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
      {
        _context = context;
        _userManager = userManager;
      }

      // Display all user profiles
      public async Task<IActionResult> Index()
      {
        var userProfiles = await _context.UserProfiles.Include(up => up.Addresses).Include(up => up.PaymentMethods).ToListAsync();
        return View(userProfiles);
      }

      // Show details of a specific user profile
      public async Task<IActionResult> Details(int id)
      {
        var userProfile = await _context.UserProfiles.Include(up => up.Addresses).Include(up => up.PaymentMethods).FirstOrDefaultAsync(up => up.Id == id);

        if (userProfile == null)
        {
          return NotFound();
        }

        return View(userProfile);
      }
    // Display the form to create a new user profile
    public IActionResult Create()
      {
      ViewBag.AvailableLanguages = new List<SelectListItem>
                {
                    new SelectListItem { Value = "English", Text = "English" },
                    new SelectListItem { Value = "Spanish", Text = "Spanish" },
                    new SelectListItem { Value = "French", Text = "French" }
                };
      ViewBag.AvailablePaymentMethods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "CreditCard", Text = "Credit Card" },
                    new SelectListItem { Value = "PayPal", Text = "PayPal" }
                };
      ViewBag.AvailableAddresses = new List<SelectListItem>
                {
                    new SelectListItem { Value = "123MainSt", Text = "123 Main St" },
                    new SelectListItem { Value = "456OakAve", Text = "456 Oak Ave" }
                };
      

      return View();
    }

      // Handle the creation of a new user profile
      [HttpPost]
      [ValidateAntiForgeryToken]
    public ActionResult Create(UserProfile model)
    {
      if (ModelState.IsValid)
      {
        // Save user profile logic
        return RedirectToAction("Index");
      }

      // Repopulate the dropdown lists if model state is invalid
      ViewBag.AvailableLanguages = new List<SelectListItem>
            {
                new SelectListItem { Value = "English", Text = "English" },
                new SelectListItem { Value = "Spanish", Text = "Spanish" },
                new SelectListItem { Value = "French", Text = "French" }
            };
      ViewBag.AvailablePaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "CreditCard", Text = "Credit Card" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" }
            };
      ViewBag.AvailableAddresses = new List<SelectListItem>
            {
                new SelectListItem { Value = "123MainSt", Text = "123 Main St" },
                new SelectListItem { Value = "456OakAve", Text = "456 Oak Ave" }
            };

      return View(model);
    }

    // Display the form to edit an existing user profile
    public async Task<IActionResult> Edit(int id)
      {
        var userProfile = await _context.UserProfiles.FindAsync(id);

        if (userProfile == null)
        {
          return NotFound();
        }

        return View(userProfile);
      }

      // Handle the editing of an existing user profile
      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> Edit(int id, UserProfile userProfile)
      {
        if (id != userProfile.Id)
        {
          return NotFound();
        }

        if (ModelState.IsValid)
        {
          try
          {
            _context.Update(userProfile);
            await _context.SaveChangesAsync();
          }
          catch (DbUpdateConcurrencyException)
          {
            if (!UserProfileExists(userProfile.Id))
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
        return View(userProfile);
      }

      // Display the form to delete an existing user profile
      public async Task<IActionResult> Delete(int id)
      {
        var userProfile = await _context.UserProfiles.Include(up => up.Addresses).Include(up => up.PaymentMethods).FirstOrDefaultAsync(up => up.Id == id);

        if (userProfile == null)
        {
          return NotFound();
        }

        return View(userProfile);
      }

      // Handle the deletion of an existing user profile
      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public async Task<IActionResult> DeleteConfirmed(int id)
      {
        var userProfile = await _context.UserProfiles.FindAsync(id);
        _context.UserProfiles.Remove(userProfile);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }

      private bool UserProfileExists(int id)
      {
        return _context.UserProfiles.Any(e => e.Id == id);
      }
    }
}
