using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VehicleRentalSystem.Data;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model, string password)
        {
            ModelState.Remove("PasswordHash"); // we set this manually below, not from the form

            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Age validation (must be 18+)
            if (model.DateOfBirth.HasValue)
            {
                int age = DateTime.Today.Year - model.DateOfBirth.Value.Year;
                if (model.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;

                if (age < 18)
                {
                    ModelState.AddModelError("DateOfBirth", "You must be at least 18 years old.");
                    return View(model);
                }
            }

            model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            model.Role = UserRole.Customer; // registration is always Customer

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (user.Role == UserRole.Admin)
                return RedirectToAction("Index", "AdminDashboard"); // we'll create this later
            else
                return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}