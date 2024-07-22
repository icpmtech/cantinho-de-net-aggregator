using MarketAnalyticHub.Areas.Admin.Models;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalyticHub.Areas.Admin.Controllers
{
  [Area("Admin")]
  public class UsersManagerController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersManagerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
      _roleManager = roleManager;
      _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
      var users = await _userManager.Users.ToListAsync();
      var userRolesViewModel = new List<UserViewModel>();
      foreach (ApplicationUser user in users)
      {
        var thisViewModel = new UserViewModel
        {
          UserId = user.Id,
          Email = user.Email,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Roles = await GetUserRoles(user)
        };
        userRolesViewModel.Add(thisViewModel);
      }
      return View(userRolesViewModel);
    }

    private async Task<List<string>> GetUserRoles(ApplicationUser user)
    {
      return new List<string>(await _userManager.GetRolesAsync(user));
    }

    public async Task<IActionResult> Edit(string id)
    {
      var user = await _userManager.FindByIdAsync(id);
      if (user == null)
      {
        return NotFound();
      }

      var userRoles = await _userManager.GetRolesAsync(user);

      var model = new EditUserViewModel
      {
        UserId = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Roles = userRoles
      };

      ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
          return NotFound();
        }

        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
          var userRoles = await _userManager.GetRolesAsync(user);
          var selectedRoles = model.Roles ?? new List<string>();

          var resultRemove = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
          if (!resultRemove.Succeeded)
          {
            ModelState.AddModelError("", "Failed to remove old roles");
            return View(model);
          }

          var resultAdd = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
          if (!resultAdd.Succeeded)
          {
            ModelState.AddModelError("", "Failed to add new roles");
            return View(model);
          }

          return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to update user");
      }

      ViewData["Roles"] = new SelectList(_roleManager.Roles, "Name", "Name");
      return View(model);
    }
  }
}
