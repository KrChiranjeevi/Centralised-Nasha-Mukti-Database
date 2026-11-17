using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Interventions — Phase 6 implementation.</summary>
public class InterventionsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Interventions"; return View(); }
}

