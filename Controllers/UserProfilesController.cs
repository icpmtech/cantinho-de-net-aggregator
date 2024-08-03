namespace MarketAnalyticHub.Controllers
{
  using MarketAnalyticHub.Models;
  using MarketAnalyticHub.Models.SetupDb;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.EntityFrameworkCore;
  using System.Linq;
  using System.Security.Claims;
  using System.Threading.Tasks;

  public class UserProfilesController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    public async Task<ActionResult> Index()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var user = await _userManager.FindByIdAsync(userId);

      var userProfile = await _context.UserProfiles
          .Where(p => p.UserId == userId)
          .Include(up => up.Addresses)
          .Include(up => up.PaymentMethods)
          .FirstOrDefaultAsync();

      if (userProfile is null)
      {
        userProfile = MapToUserProfile(user);
        userProfile.UserId = userId;
        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();
      }

      return View(userProfile);
    }

    public UserProfile MapToUserProfile(ApplicationUser appUser)
    {
      return new UserProfile
      {
        FirstName = appUser.FirstName,
        LastName = appUser.LastName,
        Email = appUser.Email,
        Organization = appUser.Organization,
        PhoneNumber = appUser.PhoneNumber,
        Address = appUser.Address,
        State = appUser.State,
        ZipCode = appUser.ZipCode,
        Country = appUser.Country,
        Language = appUser.Language,
        TimeZone = appUser.TimeZone,
        Currency = appUser.Currency,
        AvatarUrl = appUser.AvatarUrl,
        UserId = appUser.Id,
        Username = appUser.UserName,
        Status = "Active",
        Role = "User",
        TaxId = string.Empty,
        Contact = appUser.PhoneNumber,
        Plan = "Free",
        PlanExpiry = DateTime.MaxValue,
        PaymentMethod = string.Empty,
        BillingAddress = appUser.Address,
        TasksDone = 0,
        ProjectsDone = 0,
        Languages = new List<string> { appUser.Language },
        PaymentMethods = new List<PaymentMethod>(),
        Addresses = new List<Address>(),
        ActivationKey = string.Empty,
        IsActivated = true
      };
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserProfile model)
    {
      var userProfile = await _context.UserProfiles
          .Include(up => up.Addresses)
          .Include(up => up.PaymentMethods)
          .FirstOrDefaultAsync(up => up.Id == model.Id);

      if (userProfile != null)
      {
        userProfile.FirstName = model.FirstName;
        userProfile.LastName = model.LastName;
        userProfile.Username = model.Username;
        userProfile.Email = model.Email;
        userProfile.Status = model.Status;
        userProfile.TaxId = model.TaxId;
        userProfile.Contact = model.Contact;
        userProfile.Languages = model.Languages;
        userProfile.Country = model.Country;

        await _context.SaveChangesAsync();
      }

      return View("Index", userProfile);
    }

    [HttpPost]
    public async Task<IActionResult> AddPaymentMethod(PaymentMethod method)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        method.UserProfileId = userProfile.Id;
        _context.PaymentMethods.Add(method);
        await _context.SaveChangesAsync();
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> EditAddress(Address address)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles
          .Include(up => up.Addresses)
          .FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        var existingAddress = userProfile.Addresses.FirstOrDefault(a => a.IsBillingAddress == address.IsBillingAddress);
        if (existingAddress != null)
        {
          existingAddress.FirstName = address.FirstName;
          existingAddress.LastName = address.LastName;
          existingAddress.Country = address.Country;
          existingAddress.AddressLine1 = address.AddressLine1;
          existingAddress.AddressLine2 = address.AddressLine2;
          existingAddress.Landmark = address.Landmark;
          existingAddress.City = address.City;
          existingAddress.State = address.State;
          existingAddress.ZipCode = address.ZipCode;
        }
        else
        {
          address.UserProfileId = userProfile.Id;
          userProfile.Addresses.Add(address);
        }

        await _context.SaveChangesAsync();
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpgradePlan(string plan)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        // Add logic to update the user's plan
        // userProfile.Plan = plan; // Assuming a Plan property exists in UserProfile
        await _context.SaveChangesAsync();
      }

      return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Activate()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Activate(string activationKey)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null && userProfile.ActivationKey == activationKey)
      {
        userProfile.IsActivated = true;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ModelState.AddModelError(string.Empty, "Invalid activation key.");
      return View();
    }

    [HttpGet]
    public IActionResult CreateActivationKey()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateActivationKey(string activationKey)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        userProfile.ActivationKey = activationKey;
        userProfile.IsActivated = false;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ModelState.AddModelError(string.Empty, "Failed to create activation key.");
      return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditActivationKey()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile == null)
      {
        return View("NoProfile");
      }

      return View(userProfile);
    }

    [HttpPost]
    public async Task<IActionResult> EditActivationKey(string activationKey)
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        userProfile.ActivationKey = activationKey;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ModelState.AddModelError(string.Empty, "Failed to update activation key.");
      return View(userProfile);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteActivationKey()
    {
      var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
      var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

      if (userProfile != null)
      {
        userProfile.ActivationKey = null;
        userProfile.IsActivated = false;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
      }

      ModelState.AddModelError(string.Empty, "Failed to delete activation key.");
      return RedirectToAction("Index");
    }
  }
}
