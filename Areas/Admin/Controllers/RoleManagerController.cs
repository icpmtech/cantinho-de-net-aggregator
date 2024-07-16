using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Areas.Admin.Controllers
{
  [Area("Admin")]
  public class RoleManagerController : Controller
  {
    private readonly RoleManager<IdentityRole> _roleManager;
    public RoleManagerController(RoleManager<IdentityRole> roleManager)
    {
      _roleManager = roleManager;
    }
    public async Task<IActionResult> Index()
    {
      var roles = await _roleManager.Roles.ToListAsync();
      return View(roles);
    }
    [HttpPost]
    public async Task<IActionResult> AddRole(string roleName)
    {
      if (!string.IsNullOrEmpty(roleName))
      {
        var role = new IdentityRole { Name = roleName.Trim() };
        var result = await _roleManager.CreateAsync(role);
        if (result.Succeeded)
        {
          return Json(new { id = role.Id, name = role.Name });
        }
      }
      return BadRequest();
    }
  }
}
