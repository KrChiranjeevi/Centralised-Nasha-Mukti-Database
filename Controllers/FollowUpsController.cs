using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

/// <summary>Follow-ups — Phase 6 implementation.</summary>
public class FollowUpsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Follow-Ups"; return View(); }
}

