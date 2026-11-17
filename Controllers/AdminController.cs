using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Admin panel — Phase 3/7 will add Identity user management.</summary>
public class AdminController : Controller
{
    public IActionResult Users()      { ViewData["Title"] = "User Management"; return View(); }
    public IActionResult AuditLog()   { ViewData["Title"] = "Audit Log"; return View(); }
}

