namespace MarketAnalyticHub.Controllers
{
  using AspnetCoreMvcFull.Models;
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using System.Security.Claims;
  using System.Threading.Tasks;

  [Route("api/[controller]")]
  [Authorize]
  public partial class UserProfileController : ControllerBase
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
      _userManager = userManager;
      _context = context;
    }

    // GET: api/UserProfile
    [HttpGet]
    public async Task<ActionResult<ApplicationUser>> GetUserProfile()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return NotFound();
      }

      return user;
    }

    // PUT: api/UserProfile
    [HttpPut]
    public async Task<IActionResult> PutUserProfile([FromBody] UpdateUserProfileDto updatedUser)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return NotFound();
      }

      user.FirstName = updatedUser.FirstName;
      user.LastName = updatedUser.LastName;
      user.Organization = updatedUser.Organization;
      user.PhoneNumber = updatedUser.PhoneNumber;
      user.Address = updatedUser.Address;
      user.State = updatedUser.State;
      user.ZipCode = updatedUser.ZipCode;
      user.Country = updatedUser.Country;
      user.Language = updatedUser.Language;
      user.TimeZone = updatedUser.TimeZone;
      user.Currency = updatedUser.Currency;
      user.AvatarUrl = updatedUser.AvatarUrl;

      var result = await _userManager.UpdateAsync(user);

      if (!result.Succeeded)
      {
        return BadRequest(result.Errors);
      }

      return NoContent();
    }

    // POST: api/UserProfile/AIPilotActivation
    [HttpPost("AIPilotActivation")]
    public async Task<IActionResult> AIPilotActivation([FromBody] AIPilotActivation accountAIPilotActivation)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return NotFound();
      }
      user.AIPilot = accountAIPilotActivation.AccountAIPilotActivation == "on" ? true : false;
      if ((bool)user.AIPilot)
      {
        var addRoleResult = await _userManager.AddToRoleAsync(user, "aiPilot");
        if (!addRoleResult.Succeeded)
        {
          return BadRequest(addRoleResult.Errors);
        }
      }
      else
      {
        var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, "aiPilot");
        if (!removeRoleResult.Succeeded)
        {
          return BadRequest(removeRoleResult.Errors);
        }
      }
      var result = await _userManager.UpdateAsync(user);

      if (!result.Succeeded)
      {
        return BadRequest(result.Errors);
      }

      return NoContent();
    }
    // POST: api/UserProfile/profile-avatar
    [HttpPost("profile-avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] ProfileAvatar profileAvatar)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      if (profileAvatar.Avatar == null || profileAvatar.Avatar.Length == 0)
      {
        return BadRequest("File not found or empty.");
      }

      // Validate the file type (only allow specific file types)
      var allowedFileTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
      var extension = Path.GetExtension(profileAvatar.Avatar.FileName).ToLowerInvariant();
      if (!allowedFileTypes.Contains(extension))
      {
        return BadRequest("Invalid file type. Allowed types are .jpg, .jpeg, .png, .gif.");
      }

      // Process and save the file
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return NotFound();
      }

      var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
      if (!Directory.Exists(uploadsFolder))
      {
        Directory.CreateDirectory(uploadsFolder);
      }

      var filePath = Path.Combine(uploadsFolder, user.Id + extension);

      try
      {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
          await profileAvatar.Avatar.CopyToAsync(stream);
        }

        var avatarUrl = $"/uploads/avatars/{user.Id}{extension}";
        user.AvatarUrl = avatarUrl;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
          return BadRequest(result.Errors);
        }

        return Ok(avatarUrl); // Ensure the URL is returned as a plain string
      }
      catch (Exception ex)
      {
        // Log the exception
        Console.Error.WriteLine($"File upload failed: {ex.Message}");
        return StatusCode(500, "Internal server error");
      }
    }
    // GET: api/UserProfile/HasAIPilotActivation
    [HttpGet("HasAIPilotActivation")]
    public async Task<bool?> HasAIPilotActivation()
    {


      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      if (user == null)
      {
        return false;
      }
      return user.AIPilot;
    }
  }
}
