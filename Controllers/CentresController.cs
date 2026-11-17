using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Centres CRUD — Phase 4 will add full DB operations.</summary>
public class CentresController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Centres"; return View(); }
}
