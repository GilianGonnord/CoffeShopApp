using CoffeeShopApp.Data;
using CoffeeShopApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Claim = System.Security.Claims.Claim;

namespace CoffeeShopApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly CoffeeShopContext _context;

        public AccountController(CoffeeShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? error = null)
        {
            if (!string.IsNullOrEmpty(error))
            {
                switch (error)
                {
                    case "microsoft_auth_failed":
                        ViewBag.Error = "Microsoft authentication failed. Please try again.";
                        break;
                    case "user_not_found":
                        ViewBag.Error = "No account found with your Microsoft email address. Please contact an administrator.";
                        break;
                    default:
                        ViewBag.Error = "An error occurred during authentication.";
                        break;
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult LoginWithMicrosoft()
        {
            var redirectUrl = Url.Action("MicrosoftCallback", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> MicrosoftCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login", new { error = "microsoft_auth_failed" });
            }

            // Get email from Microsoft claims
            var emailClaim = authenticateResult.Principal?.FindFirst(ClaimTypes.Email)
                           ?? authenticateResult.Principal?.FindFirst("email")
                           ?? authenticateResult.Principal?.FindFirst("preferred_username");

            if (emailClaim == null || string.IsNullOrEmpty(emailClaim.Value))
            {
                // Clean up any authentication state before redirecting
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login", new { error = "microsoft_auth_failed" });
            }

            var email = emailClaim.Value.ToLowerInvariant();

            // Look up user by email
            var user = await _context.Users
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                // Completely clean up authentication state
                await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear any remaining authentication cookies manually
                Response.Cookies.Delete(".AspNetCore.Cookies");
                Response.Cookies.Delete(".AspNetCore.OpenIdConnect.Nonce");
                Response.Cookies.Delete(".AspNetCore.Correlation");

                // Ensure user is not authenticated
                HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

                return RedirectToAction("Login", new { error = "user_not_found" });
            }

            // Sign out from Microsoft and sign in with our cookie scheme
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add custom claims
            foreach (var userClaim in user.Claims)
            {
                claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                ViewBag.Error = "Username already exists.";
                return View();
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add default claim
            var defaultClaim = new CoffeeShopApp.Models.Claim
            {
                UserId = user.Id,
                ClaimType = CoffeeClaims.CanViewCoffee,
                ClaimValue = "true"
            };

            _context.Claims.Add(defaultClaim);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Registration successful! You can now login.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // Check if user is signed in with Microsoft
            var microsoftUser = HttpContext.User.FindFirst("iss")?.Value?.Contains("microsoft");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (microsoftUser == true)
            {
                // Also sign out from Microsoft
                return SignOut(new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                }, OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}