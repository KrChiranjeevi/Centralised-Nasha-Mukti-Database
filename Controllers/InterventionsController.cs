using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class InterventionsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Interventions"; return View(); }
}
