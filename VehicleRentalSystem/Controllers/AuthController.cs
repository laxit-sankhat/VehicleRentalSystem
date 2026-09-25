using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.Interfaces;

namespace VehicleRentalSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model, string password)
        {
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
                return View(model);

            bool emailExists = await _userRepository.EmailExistsAsync(model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

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
            model.Role = UserRole.Customer;

            await _userRepository.AddAsync(model);

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

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
                return RedirectToAction("Index", "AdminDashboard");
            else
                return RedirectToAction("Index", "Home");
        }

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();

            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(User model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();

            ModelState.Remove("PasswordHash");
            ModelState.Remove("Email");
            ModelState.Remove("Role");

            if (!ModelState.IsValid)
                return View(model);

            // Only update the fields the user is allowed to change
            user.Name = model.Name;
            user.Phone = model.Phone;
            user.DLNumber = model.DLNumber;
            user.DLExpiryDate = model.DLExpiryDate;

            await _userRepository.UpdateAsync(user);

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }
    }
}