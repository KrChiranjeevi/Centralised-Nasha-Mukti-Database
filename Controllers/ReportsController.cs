using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Reports and analytics — Phase 7 implementation.</summary>
public class ReportsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Reports & Analytics"; return View(); }
}
