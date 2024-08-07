using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Identity;

namespace MarketAnalyticHub.Controllers;

public class AuthController : Controller
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly SignInManager<ApplicationUser> _signInManager;

  public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
  {
    _userManager = userManager;
    _signInManager = signInManager;
  }
  public IActionResult ForgotPasswordBasic() => View();
  public IActionResult LoginBasic() => View();
  public IActionResult RegisterBasic() => View();

 

}
