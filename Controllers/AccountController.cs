using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Handles user authentication, login, logout, and access-denied handling
/// using ASP.NET Core Identity with secure cookie authentication.
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    // ── GET: /Account/Login ──────────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // If already logged in, redirect directly to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // ── POST: /Account/Login ─────────────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Locate user by email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Non-disclosing generic error to prevent account enumeration
            _logger.LogWarning("Failed login attempt for non-existent user: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View(model);
        }

        // 2. Check if account is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for deactivated account: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "This account has been deactivated. Please contact your administrator.");
            return View(model);
        }

        // 3. Attempt password sign-in (using secure ASP.NET Identity password hasher)
        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User {Email} account is locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Account is temporarily locked out due to multiple failed attempts. Please try again later.");
            return View(model);
        }

        // Generic error: never disclose whether password or username was wrong
        _logger.LogWarning("Invalid password attempt for user: {Email}", model.Email);
        ModelState.AddModelError(string.Empty, "Invalid email address or password.");
        return View(model);
    }

    // ── POST: /Account/Logout ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var email = User.Identity?.Name;
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User {Email} logged out.", email);
        return RedirectToAction("Login", "Account");
    }

    // ── GET: /Account/Logout (Convenience redirect) ───────────────
    [HttpGet]
    public async Task<IActionResult> LogoutGet()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    // ── GET: /Account/AccessDenied ────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View("~/Views/Shared/AccessDenied.cshtml");
    }
}
