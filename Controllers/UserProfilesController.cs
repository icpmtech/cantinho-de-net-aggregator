using MarketAnalyticHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MarketAnalyticHub.Controllers
{
  public class UserProfilesController : Controller
  {

    public IActionResult Index()
    {
      var model = new UserProfile
      {
        FirstName = "Violet",
        LastName = "Mendoza",
        Username = "@@violet.dev",
        Email = "vafgot@vultukir.org",
        Status = "Active",
        Role = "Author",
        TaxId = "Tax-8965",
        Contact = "(123) 456-7890",
        Languages = new List<string> { "French" },
        Country = "England",
        PlanExpiry = DateTime.Now.AddMonths(1),
        PaymentMethods = new List<PaymentMethod>
                {
                    new PaymentMethod
                    {
                        CardType = "MasterCard",
                        CardName = "Kaith Morrison",
                        CardNumber = "**** **** **** 9856",
                        ExpiryDate = new DateTime(2024, 12, 1),
                        IsPrimary = true
                    },
                    new PaymentMethod
                    {
                        CardType = "Visa",
                        CardName = "Tom McBride",
                        CardNumber = "**** **** **** 6542",
                        ExpiryDate = new DateTime(2024, 2, 1),
                        IsPrimary = false
                    }
                },
        Addresses = new List<Address>
        {
                    new Address
                    {
                        AddressLine1 = "100 Water Plant",
                        AddressLine2 = "Avenue, Building 1303",
                        City = "Wake Island",
                        State = "Capholim",
                        ZipCode = "403114",
                        Country = "Wake Island",
                        IsBillingAddress = true
                    }
                }
      };

      return View(model);
    }

    [HttpPost]
    public IActionResult Edit(UserProfile model)
    {
      // Handle the update logic here
      // For now, just return the same view with the updated model
      return View("Index", model);
    }

    [HttpPost]
    public IActionResult AddPaymentMethod(PaymentMethod method)
    {
      // Handle adding a new payment method here
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult EditAddress(Address address)
    {
      // Handle editing an address here
      return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult UpgradePlan(string plan)
    {
      // Handle upgrading the plan here
      return RedirectToAction("Index");
    }
  }
}
