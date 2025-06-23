using MarketAnalyticHub.Areas.Admin.Models;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
  [Area("Admin")]
  [Authorize]
  public class UserRolesController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserRolesController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
      _roleManager = roleManager;
      _userManager = userManager;
    }
    public async Task<IActionResult> Index()
    {
      var users = await _userManager.Users.ToListAsync();
      var userRolesViewModel = new List<UserRolesViewModel>();
      foreach (ApplicationUser user in users)
      {
        var thisViewModel = new UserRolesViewModel();
        thisViewModel.UserId = user.Id;
        thisViewModel.Email = user.Email;
        thisViewModel.FirstName = user.FirstName;
        thisViewModel.LastName = user.LastName;
        thisViewModel.Roles = await GetUserRoles(user);
        userRolesViewModel.Add(thisViewModel);
      }
      return View(userRolesViewModel);
    }
    private async Task<List<string>> GetUserRoles(ApplicationUser user)
    {
      return new List<string>(await _userManager.GetRolesAsync(user));
    }
    [HttpGet]
    public async Task<IActionResult> Manage(string userId)
    {
      if (string.IsNullOrEmpty(userId))
        return BadRequest();

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
        return View("NotFound", $"User with Id = {userId} cannot be found");

      ViewBag.UserId = userId;
      ViewBag.UserName = user.UserName;

      var roles = _roleManager.Roles.ToList();
      var userRoles = await _userManager.GetRolesAsync(user);

      var model = roles
          .Select(r => new ManageUserRolesViewModel
          {
            RoleId = r.Id,
            RoleName = r.Name,
            Selected = userRoles.Contains(r.Name)
          })
          .ToList();

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manage(
        List<ManageUserRolesViewModel> model,
        string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
        return View("NotFound", $"User with Id = {userId} cannot be found");

      ViewBag.UserId = userId;
      ViewBag.UserName = user.UserName;

      if (!ModelState.IsValid)
        return View(model);

      // Remove all roles
      var currentRoles = await _userManager.GetRolesAsync(user);
      var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
      if (!removeResult.Succeeded)
      {
        ModelState.AddModelError("", "Could not remove existing roles.");
        return View(model);
      }

      // Add selected
      var selectedRoles = model.Where(x => x.Selected).Select(x => x.RoleName);
      var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
      if (!addResult.Succeeded)
      {
        ModelState.AddModelError("", "Could not add selected roles.");
        return View(model);
      }

      return RedirectToAction("Index");  // or wherever you list users
    }


  }
}
