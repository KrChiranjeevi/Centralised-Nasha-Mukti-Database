using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Dashboard controller — Accessible to all authenticated users (Admin and CentreStaff).
/// </summary>
[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
