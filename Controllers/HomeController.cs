using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Test_Drive.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Test_Drive.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ODB _context;


        public HomeController(ILogger<HomeController> logger , ODB context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Index()
        {
            
            return View();
        }

        [Authorize(Roles = "Admin")]

        public IActionResult IndexAdmin()
        {
            int Users = int.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var admin = _context.Users.Include(u => u.Roles).Where(a => a.Roles.Name == "Admin").Count();
            ViewBag.Admin = admin;

            var user = _context.Users.Include(u => u.Roles).Where(a => a.Roles.Name == "User").Count();
            ViewBag.Users = user;

            var cars = _context.Cars.Count();
            ViewBag.Cars = cars;

            var book = _context.Book.Include(u => u.Cars).Include(u => u.Users).Count();
            ViewBag.Book = book;

            return View();
        }

        public IActionResult SignUp()
        {
            ViewData["RolesId"] = new SelectList(_context.Roles, "Id", "Id");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Id,Name,PhoneNumber,Email,Gender,Password")] Users users)
        {
            if (ModelState.IsValid)
            {
                _context.Add(users);
                users.RolesId = 2;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolesId"] = new SelectList(_context.Roles, "Id", "Id", users.RolesId);
            return View(users);
        }

        #region Login/Logout
        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string UserName = User.FindFirst(ClaimTypes.Name).Value;

                    // Assuming you have a role claim for the user
                    string UserRole = User.FindFirst(ClaimTypes.Role).Value;

                    if (UserRole == "User")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (UserRole == "Admin")
                    {
                        return RedirectToAction("IndexAdmin", "Home");
                    }
                  
                }

                return View();
            }
            catch
            {
                return View();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                Users check = _context.Users.Include(u => u.Roles).Where(u => u.Email == Email && u.Password == Password).SingleOrDefault();
 

                if (check != null)
                {

                    var identity = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, check.Email),
                    new Claim(ClaimTypes.Role,check.Roles.Name),
                    new Claim(ClaimTypes.NameIdentifier, check.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, check.Name)

                }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal);

                    if (check.Roles.Name == "Admin")
                    {
                        return RedirectToAction("IndexAdmin", "Home");
                    }
                    else if (check.Roles.Name == "User")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

              
                else
                {
                    ModelState.AddModelError(string.Empty, "Error: Incorrect username or password!");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error: Incorrect username or password!");
                return View();
            }

        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }


        #endregion Login/Logout


       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
