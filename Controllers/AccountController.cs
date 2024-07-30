

namespace MarketAnalyticHub.Controllers
{
  using MarketAnalyticHub.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Identity.Client;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Authentication;
  using Microsoft.AspNetCore.Authentication.Cookies;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Authentication.Google;
  using Azure;
  using System.Security.Claims;

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
    [HttpGet]
    public IActionResult LoginGoogle()
    {
      var redirectUrl = Url.Action(nameof(LoginCallback), "Account");
      var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
      return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> LoginCallback()
    {
      var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

      if (!authenticateResult.Succeeded)
        return BadRequest("Failed to authenticate.");

      var claims = authenticateResult.Principal.Claims;
      var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
      var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
      var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

      if (email == null)
        return BadRequest("Email claim not found.");

      var user = await _userManager.FindByEmailAsync(email);

      if (user == null)
      {
        user = new ApplicationUser { UserName = name, Email = email };
        var createUserResult = await _userManager.CreateAsync(user);

        if (!createUserResult.Succeeded)
        {
          foreach (var error in createUserResult.Errors)
          {
            ModelState.AddModelError(string.Empty, error.Description);
          }
          return RedirectToAction("Index", "Home");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Standard");

        if (!roleResult.Succeeded)
        {
          foreach (var error in roleResult.Errors)
          {
            ModelState.AddModelError(string.Empty, error.Description);
          }
          await _userManager.DeleteAsync(user); // Rollback user creation if role assignment fails
          return RedirectToAction("Index", "Home");
        }
      }

      await _signInManager.SignInAsync(user, isPersistent: false);
      return RedirectToAction("Index", "Dashboards");
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
          return LocalRedirect(returnUrl ?? "/Dashboard/Index");
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
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      await _signInManager.SignOutAsync();
      return RedirectToAction("Index", "Home");
    }
  }
}
