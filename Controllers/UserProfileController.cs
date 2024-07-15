namespace AspnetCoreMvcFull.Controllers
{
  using AspnetCoreMvcFull.Models;
  using AspnetCoreMvcFull.Models.SetupDb;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using System.Security.Claims;
  using System.Threading.Tasks;

  [Route("api/[controller]")]
  [Authorize]
  public class UserProfileController : ControllerBase
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
  }

  public class UpdateUserProfileDto
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Organization { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
    public string Language { get; set; }
    public string TimeZone { get; set; }
    public string Currency { get; set; }
    public string AvatarUrl { get; set; }
  }
}
