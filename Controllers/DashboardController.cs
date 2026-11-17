using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>
/// Dashboard controller — Phase 7 will wire real DB queries.
/// For now returns the view which loads data via API calls from JavaScript.
/// </summary>
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
