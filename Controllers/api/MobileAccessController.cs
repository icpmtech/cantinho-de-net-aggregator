namespace MarketAnalyticHub.Controllers.api
{
  using MarketAnalyticHub.Models;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using System.Threading.Tasks;

  [Route("api/[controller]")]
  [ApiController]
  public class MobileAccessController : ControllerBase
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<MobileAccessController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    public MobileAccessController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<MobileAccessController> logger)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _logger = logger;
    }

    // POST: api/MobileAccess/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
      if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
      {
        _logger.LogWarning("Login attempt with missing email or password.");
        return BadRequest("Email and password must be provided.");
      }

      var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, lockoutOnFailure: true);
      if (result.Succeeded)
      {
        _logger.LogInformation("User logged in successfully via API.");

        // In a real application, generate a secure token (e.g., JWT) and return user details.
        var response = new LoginResponse
        {
          Token = "dummy-token",
          Username = request.Email  // Optionally, retrieve the user's display name from your user store.
        };

        return Ok(response);
      }
      else if (result.RequiresTwoFactor)
      {
        _logger.LogInformation("User requires two-factor authentication.");
        return Unauthorized("Two-factor authentication required.");
      }
      else if (result.IsLockedOut)
      {
        _logger.LogWarning("User account locked out.");
        return Unauthorized("User account locked out.");
      }
      else
      {
        _logger.LogWarning("Invalid login attempt for {Email}", request.Email);
        return Unauthorized("Invalid credentials.");
      }
    }

    // POST: api/MobileAccess/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
      if (request == null ||
          string.IsNullOrWhiteSpace(request.Username) ||
          string.IsNullOrWhiteSpace(request.Email) ||
          string.IsNullOrWhiteSpace(request.Password))
      {
        _logger.LogWarning("Registration attempt with missing username, email or password.");
        return BadRequest("Username, email and password must be provided.");
      }

      // Create a new user with the provided details.
      var user = new ApplicationUser
      {
        UserName = request.Username,
        Email = request.Email
      };

      // Attempt to create the user.
      var result = await _userManager.CreateAsync(user, request.Password);
      if (result.Succeeded)
      {
        _logger.LogInformation("User registered successfully via API.");

        // In a real application, generate a secure token (e.g., JWT) here.
        var response = new RegisterResponse
        {
          Token = "dummy-token",
          Username = request.Username
        };

        return Ok(response);
      }
      else
      {
        _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, result.Errors);
        return BadRequest(result.Errors);
      }
    }
  }

// DTO for registration request
public class RegisterRequest
{
  public string Username { get; set; }  // New field for username
  public string Email { get; set; }
  public string Password { get; set; }
}

// DTO for registration response
public class RegisterResponse
{
  public string Token { get; set; }
  public string Username { get; set; }
}
// DTO for login request
public class LoginRequest
  {
    public string Email { get; set; }
    public string Password { get; set; }
  }

  // DTO for login response
  public class LoginResponse
  {
    public string Token { get; set; }
    public string Username { get; set; }
  }
}
