using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Counselling sessions — Phase 6 implementation.</summary>
public class CounsellingController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Counselling Sessions"; return View(); }
}

