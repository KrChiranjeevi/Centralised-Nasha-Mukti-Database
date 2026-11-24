using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Data;
using NashaMuktiDB.Models;
using NashaMuktiDB.ViewModels;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Handles user authentication, login, logout, access-denied handling,
/// public registration/account requests, and password establishment
/// using ASP.NET Core Identity with secure cookie authentication.
/// </summary>
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
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
            // Check if there is an AccountRequest pending or rejected for this email
            var emailLower = model.Email.Trim().ToLower();
            var accountRequest = await _context.AccountRequests
                .Where(r => r.Email.ToLower() == emailLower)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (accountRequest != null)
            {
                if (accountRequest.Status == nameof(AccountRequestStatus.Pending))
                {
                    _logger.LogInformation("Sign in blocked for pending account request: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Your account request is currently pending administrative review. You will receive access once an administrator approves your request.");
                    return View(model);
                }
                else if (accountRequest.Status == nameof(AccountRequestStatus.Rejected))
                {
                    _logger.LogInformation("Sign in blocked for rejected account request: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Your account request was declined by the system administrator. Please contact your nodal centre for assistance.");
                    return View(model);
                }
            }

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

            // If user must establish a permanent password, redirect to ChangePassword flow
            if (user.MustChangePassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

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

    // ── GET: /Account/RequestAccount ─────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RequestAccount()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        await PopulateCentresDropDownAsync();
        return View(new RequestAccountViewModel());
    }

    // ── POST: /Account/RequestAccount ────────────────────────────
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestAccount(RequestAccountViewModel model)
    {
        if (model.RequestedCentreId.HasValue)
        {
            var centreExists = await _context.Centres.AnyAsync(c => c.CentreId == model.RequestedCentreId.Value && c.IsActive);
            if (!centreExists)
            {
                ModelState.AddModelError(nameof(model.RequestedCentreId), "The selected de-addiction centre is invalid or inactive.");
            }
        }

        // Duplicate check 1: Active Identity user with this email
        var normalizedEmail = model.Email.Trim().ToLower();
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An authorized account with this email address already exists. Please sign in or contact your administrator.");
        }

        // Duplicate check 2: Existing pending request with this email
        var pendingRequest = await _context.AccountRequests
            .AnyAsync(r => r.Email.ToLower() == normalizedEmail && r.Status == nameof(AccountRequestStatus.Pending));
        if (pendingRequest)
        {
            ModelState.AddModelError(nameof(model.Email), "An access request with this email address is already pending administrative review.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCentresDropDownAsync(model.RequestedCentreId);
            return View(model);
        }

        var accountRequest = new AccountRequest
        {
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            ContactNumber = string.IsNullOrWhiteSpace(model.ContactNumber) ? null : model.ContactNumber.Trim(),
            RequestedCentreId = model.RequestedCentreId!.Value,
            ReasonForAccess = model.ReasonForAccess.Trim(),
            Status = nameof(AccountRequestStatus.Pending),
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountRequests.Add(accountRequest);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New access request #{RequestId} submitted for {Email} ({FullName}) at Centre {CentreId}",
            accountRequest.AccountRequestId, accountRequest.Email, accountRequest.FullName, accountRequest.RequestedCentreId);

        return RedirectToAction(nameof(RequestConfirmation), new { id = accountRequest.AccountRequestId });
    }

    // ── GET: /Account/RequestConfirmation ────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RequestConfirmation(int id)
    {
        var request = await _context.AccountRequests
            .Include(r => r.RequestedCentre)
            .FirstOrDefaultAsync(r => r.AccountRequestId == id);

        if (request == null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(request);
    }

    // ── GET: /Account/ChangePassword ─────────────────────────────
    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        ViewData["Title"] = "Establish Permanent Password";
        return View(new ChangePasswordViewModel());
    }

    // ── POST: /Account/ChangePassword ────────────────────────────
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        ViewData["Title"] = "Establish Permanent Password";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        user.MustChangePassword = false;
        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        _logger.LogInformation("User {Email} established permanent password successfully.", user.Email);
        TempData["Success"] = "Your permanent password has been established successfully. Welcome to Nasha Mukti DB.";
        return RedirectToAction("Index", "Dashboard");
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

    // ── GET: /Account/Logout (Convenience safe redirect) ─────────
    [HttpGet]
    public IActionResult LogoutGet()
    {
        // Require POST with antiforgery token for sign out to protect against CSRF attacks (CWE-352)
        return RedirectToAction("Login", "Account");
    }

    // ── GET: /Account/AccessDenied ────────────────────────────────
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("~/Views/Shared/AccessDenied.cshtml");
    }

    private async Task PopulateCentresDropDownAsync(int? selectedCentreId = null)
    {
        var centres = await _context.Centres
            .Where(c => c.IsActive)
            .OrderBy(c => c.CentreName)
            .Select(c => new
            {
                c.CentreId,
                DisplayName = $"{c.CentreName} ({c.District})"
            })
            .ToListAsync();

        ViewBag.CentresList = new SelectList(centres, "CentreId", "DisplayName", selectedCentreId);
    }
}
