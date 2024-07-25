using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalyticHub.Controllers
{

  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using System.Threading.Tasks;



  public class AccountController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _logger = logger;
    }
    public IActionResult ForgotPasswordBasic() => View();
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
          // Add user to the role
          var roleResult = await _userManager.AddToRoleAsync(user, "Standard");
          if (!roleResult.Succeeded)
          {
            foreach (var error in roleResult.Errors)
            {
              ModelState.AddModelError(string.Empty, error.Description);
            }
            // Optionally, you might want to delete the user if role assignment fails
            await _userManager.DeleteAsync(user);
            return View(model);
          }

          await _signInManager.SignInAsync(user, isPersistent: false);
          return RedirectToAction("Index", "Dashboards");
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }
      return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;
      if (ModelState.IsValid)
      {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
          _logger.LogInformation("User logged in.");
          return LocalRedirect(returnUrl ?? "/");
        }
        if (result.RequiresTwoFactor)
        {
          return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        }
        if (result.IsLockedOut)
        {
          _logger.LogWarning("User account locked out.");
          return RedirectToPage("./Lockout");
        }
        else
        {
          ModelState.AddModelError(string.Empty, "Invalid login attempt.");
          return View(model);
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (ModelState.IsValid)
      {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
          return RedirectToAction("Index", "Dashboards");
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
      }
      return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();
      return RedirectToAction("Login", "Account");
    }
  }
}
