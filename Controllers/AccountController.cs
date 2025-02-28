

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
      var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", null, Request.Scheme);
      var properties = new AuthenticationProperties
      {
        RedirectUri = redirectUrl,
        Items =
        {
            { "scheme", "Google" },
        }
      };
      return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
      var authenticateResult = await HttpContext!.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
      if (!authenticateResult.Succeeded && authenticateResult.Principal == null)
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
          user = new ApplicationUser { UserName = email, Email = email };
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
    [HttpGet]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (model == null)
      {
        _logger.LogWarning("Register model is null.");
        return View(new RegisterViewModel());
      }

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
          _logger.LogInformation("User created a new account with password.");
          return RedirectToAction("Index", "Dashboards");
        }
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }
      else
      {
        _logger.LogWarning("Invalid model state. Validation failed.");
        foreach (var value in ModelState.Values)
        {
          foreach (var error in value.Errors)
          {
            _logger.LogWarning(error.ErrorMessage);
          }
        }
      }
      return View(model);
    }


    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
      ViewData["ReturnUrl"] = returnUrl;
      return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
      if (model == null)
      {
        _logger.LogWarning("Login model is null.");
        return View(new LoginViewModel());
      }

      ViewData["ReturnUrl"] = returnUrl;

      if (ModelState.IsValid)
      {
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
          _logger.LogInformation("User logged in.");
          return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboards"));
        }

        if (result.RequiresTwoFactor)
        {
          _logger.LogInformation("User needs to do two-factor authentication.");
          return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
        }

        if (result.IsLockedOut)
        {
          _logger.LogWarning("User account locked out.");
          return RedirectToPage("./Lockout");
        }
        else
        {
          _logger.LogWarning("Invalid login attempt.");
          ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials and try again.");
          ViewData["LoginError"] = "Invalid login attempt. Please check your credentials and try again.";
        }
      }
      else
      {
        _logger.LogWarning("Invalid model state. Validation failed.");
        foreach (var value in ModelState.Values)
        {
          foreach (var error in value.Errors)
          {
            _logger.LogWarning(error.ErrorMessage);
          }
        }
      }

      // If we got this far, something failed, redisplay form
      return View(model);
    }

    private bool IsLocalUrl(string url)
    {
      return !string.IsNullOrEmpty(url) && (url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\"));
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
