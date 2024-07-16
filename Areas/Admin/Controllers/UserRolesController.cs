using AspnetCoreMvcFull.Areas.Admin.Models;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Areas.Admin.Controllers
{
  [Area("Admin")]
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
      ViewBag.userId = userId;
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
        return View("NotFound");
      }
      ViewBag.UserName = user.UserName;

      var roles = _roleManager.Roles.ToList();
      var userRoles = await _userManager.GetRolesAsync(user);

      var model = new List<ManageUserRolesViewModel>();
      foreach (var role in roles)
      {
        var userRolesViewModel = new ManageUserRolesViewModel
        {
          RoleId = role.Id,
          RoleName = role.Name,
          Selected = userRoles.Contains(role.Name)
        };
        model.Add(userRolesViewModel);
      }

      return PartialView("_ManageUserRolesPartial", model);
    }
    [HttpPost]
    public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return View();
      }
      var roles = await _userManager.GetRolesAsync(user);
      var result = await _userManager.RemoveFromRolesAsync(user, roles);
      if (!result.Succeeded)
      {
        ModelState.AddModelError("", "Cannot remove user existing roles");
        return View(model);
      }
      result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
      if (!result.Succeeded)
      {
        ModelState.AddModelError("", "Cannot add selected roles to user");
        return View(model);
      }
      return RedirectToAction("Index");
    }


  }
}
