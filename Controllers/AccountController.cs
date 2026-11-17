using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Account controller — Phase 3 replaces this with full Identity-based auth.
/// Phase 1: placeholder to allow the app to build and show login page.
/// </summary>
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Sign In";
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        // Phase 3 will implement real Identity authentication here.
        ViewData["Error"] = "Authentication not yet configured. Database setup is required (Phase 2).";
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View("~/Views/Shared/AccessDenied.cshtml");
    }
}
