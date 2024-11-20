using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TaskAuthenticationAuthorization.Models;

namespace TaskAuthenticationAuthorization.Controllers
{
    [Authorize(Policy = "PremiumBuyerOnly")] // Apply the PremiumBuyerOnly policy
    public class DiscountController : Controller
    {
        private readonly ShoppingContext _contextDb;

        public DiscountController(ShoppingContext contextDb)
        {
            _contextDb = contextDb;
        }

        // GET: /Discount/Index
        public IActionResult Index()
        {
            // You can get the logged-in user's ID from the claims and fetch their discount
            var userEmail = User.Identity.Name;
            var user = _contextDb.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null) return NotFound();

            // Fetch the customer's discount based on the user
            var customer = _contextDb.Customers.FirstOrDefault(c => c.UserId == user.UserId);
            if (customer == null) return NotFound();

            return View(customer.Discount);  // Pass the discount value to the view
        }
    }
}
