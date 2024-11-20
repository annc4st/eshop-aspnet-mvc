using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskAuthenticationAuthorization.Models;
using TaskAuthenticationAuthorization.ViewModels;
using TaskAuthenticationAuthorization.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;



namespace TaskAuthenticationAuthorization.Controllers
{
    public class AccountsController : Controller
    {
 
        private ShoppingContext _contextDb;

        public AccountsController(ShoppingContext contextDb)
        {
            _contextDb = contextDb;
        }


// register GET
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

// register POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _contextDb.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    // add user to db
                    //hash psw before saving to db
                    var (hash, salt) = PasswordHelper.HashPassword(model.Password);
                    user = new User
                    {
                        Email = model.Email,
                        PasswordHash = hash,
                        Salt = salt,
                        // Role = UserRole.Admin,
                        Role = UserRole.Buyer,// default role of buyer
                        BuyerType = BuyerType.Regular //default type regular
                    };
                    _contextDb.Users.Add(user);
                    await _contextDb.SaveChangesAsync();

                    var customer = new Customer
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Address = model.Address,
                        UserId = user.UserId,// Links Customer to User via foreign key
                        Discount = model.Discount ?? Discount.O  // Set a default Discount if none is provided

                    };
        _contextDb.Customers.Add(customer);
        await _contextDb.SaveChangesAsync();
 
                    await Authenticate(user);
 
                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "User with this email already exists.");
            }
            return View(model);
        }


        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("BuyerType", user.BuyerType?.ToString() ?? "None"),  // Handling BuyerType as nullable enum
                new Claim("UserId", user.UserId.ToString()) 
            };
            // создаем объект ClaimsIdentity
            var identity = new ClaimsIdentity(
                claims, 
                "ApplicationCookie",             
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

                 var principal = new ClaimsPrincipal(identity);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

//logout 
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Accounts");
        }


//login GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

 //login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> Login (LoginViewModel model)
         {
            if(ModelState.IsValid)
            {
                User user = await _contextDb.Users.FirstOrDefaultAsync(u => 
                    u.Email == model.Email);
                if (user != null  && PasswordHelper.VerifyPassword(model.Password, user.PasswordHash, user.Salt))
                {
                    // If the password matches, authenticate the user
                     await Authenticate(user); 
                     return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Login failed - incorrect email or password
                    ModelState.AddModelError("", "Invalid email or password.");
                }

            }
            return View(model);
         }
         

          [Authorize(Policy = "AdminOnly")]
          public async Task<IActionResult> Index()
          {
            var users = await _contextDb.Users.ToListAsync();
            return View(users);
          }

  // GET : Accounts/Edit
          [HttpGet]
        public async Task<IActionResult> Edit(int? id)
            {
                var user = await _contextDb.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
            
                // Pass the user to the Edit view, along with a list of roles and buyer types
                ViewData["Roles"] = new SelectList(Enum.GetValues(typeof(UserRole)));  // Assuming UserRole is an enum
                ViewData["BuyerTypes"] = new SelectList(Enum.GetValues(typeof(BuyerType)));  // Assuming BuyerType is an enum

                return View(user);
            }

        // POST: Accounts/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Role,BuyerType")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                   // Ensure the user is loaded from the database before making updates
                    var dbUser = await _contextDb.Users.FindAsync(id);
                    if (dbUser == null)
                    {
                        return NotFound();
                    }

                     // Update the Role
                    dbUser.Role = user.Role;
            // Only update BuyerType if the role is Buyer
                        if (user.Role == UserRole.Buyer)
                        {
                            dbUser.BuyerType = user.BuyerType;
                        }
                        else
                        {
                            dbUser.BuyerType = null; // Reset BuyerType if role is changed to Admin
                        }

                    _contextDb.Update(dbUser);
                   await _contextDb.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Roles"] = new SelectList(Enum.GetValues(typeof(UserRole)));

             ViewData["BuyerTypes"] = new SelectList(Enum.GetValues(typeof(BuyerType)));
            // If validation fails, re-display the edit form
            return View(user);
        }

        // Helper method to check if the user exists
        private bool UserExists(int id)
        {
            return _contextDb.Users.Any(e => e.UserId == id);
        }

    }
}

