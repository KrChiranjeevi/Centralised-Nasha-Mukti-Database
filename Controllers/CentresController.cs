using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class CentresController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Centres"; return View(); }
}
