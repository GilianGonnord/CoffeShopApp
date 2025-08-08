using CoffeeShopApp.Data;
using CoffeeShopApp.Models;
using CoffeeShopApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CoffeeShopApp.Controllers;

public class AccountController : Controller
{
    private readonly CoffeeShopContext _context;
    private readonly IExternalAuthService _externalAuthService;

    public AccountController(CoffeeShopContext context, IExternalAuthService externalAuthService)
    {
        _context = context;
        _externalAuthService = externalAuthService;
    }

    [HttpGet]
    public IActionResult Login(string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            ViewBag.Error = error switch
            {
                "external_auth_failed" => $"External authentication failed. Please try again.",
                "user_not_found" => $"No account found with your external email address. Please contact an administrator.",
                "external_not_configured" => "External authentication is not configured on this server.",
                _ => "An error occurred during authentication.",
            };
        }

        // Pass external auth information to the view
        ViewBag.IsExternalAuthEnabled = _externalAuthService.IsEnabled;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Username and password are required.";
            ViewBag.IsExternalAuthEnabled = _externalAuthService.IsEnabled;
            return View();
        }

        var user = await _context.Users
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid username or password.";
            ViewBag.IsExternalAuthEnabled = _externalAuthService.IsEnabled;
            return View();
        }

        await SignInUserAsync(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult LoginWithExternal()
    {
        // Check if external authentication is enabled
        if (!_externalAuthService.IsEnabled)
        {
            return RedirectToAction("Login", new { error = "external_not_configured" });
        }

        var redirectUrl = Url.Action("ExternalCallback", "Account");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(properties, _externalAuthService.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalCallback()
    {
        // Check if external authentication is enabled
        if (!_externalAuthService.IsEnabled)
        {
            return RedirectToAction("Login", new { error = "external_not_configured" });
        }

        var authenticateResult = await HttpContext.AuthenticateAsync(_externalAuthService.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            return RedirectToAction("Login", new { error = "external_auth_failed" });
        }

        // Extract email from claims (works for both OIDC and SAML)
        var email = ExtractEmailFromClaims(authenticateResult.Principal);

        if (string.IsNullOrEmpty(email))
        {
            // Clean up any authentication state before redirecting
            await HttpContext.SignOutAsync(_externalAuthService.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", new { error = "external_auth_failed" });
        }

        email = email.ToLowerInvariant();

        // Look up user by email
        var user = await _context.Users
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            // Completely clean up authentication state
            await HttpContext.SignOutAsync(_externalAuthService.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear any remaining authentication cookies manually
            Response.Cookies.Delete(".AspNetCore.Cookies");

            // Clear protocol-specific cookies
            if (_externalAuthService.Protocol == AuthenticationProtocol.OIDC)
            {
                Response.Cookies.Delete(".AspNetCore.OpenIdConnect.Nonce");
                Response.Cookies.Delete(".AspNetCore.Correlation");
            }

            // Ensure user is not authenticated
            HttpContext.User = new System.Security.Claims.ClaimsPrincipal();

            return RedirectToAction("Login", new { error = "user_not_found" });
        }

        // Sign out from external provider and sign in with our cookie scheme
        await HttpContext.SignOutAsync(_externalAuthService.AuthenticationScheme);
        await SignInUserAsync(user);

        return RedirectToAction("Index", "Home");
    }

    private string? ExtractEmailFromClaims(ClaimsPrincipal? principal)
    {
        if (principal == null) return null;

        // Try different claim types that might contain email
        var emailClaimTypes = new[]
        {
            ClaimTypes.Email,
            "email",
            "preferred_username",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
            "http://schemas.microsoft.com/identity/claims/preferred_username"
        };

        foreach (var claimType in emailClaimTypes)
        {
            var claim = principal.FindFirst(claimType);
            if (claim != null && !string.IsNullOrEmpty(claim.Value))
            {
                return claim.Value;
            }
        }

        return null;
    }

    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
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

    public async Task<IActionResult> Logout()
    {
        // Check if user is signed in with external auth
        var hasExternalAuth = HttpContext.User.Identity?.AuthenticationType != CookieAuthenticationDefaults.AuthenticationScheme;

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (hasExternalAuth && _externalAuthService.IsEnabled)
        {
            // Also sign out from external provider
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            }, _externalAuthService.AuthenticationScheme);
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}