using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class ReportsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Reports & Analytics"; return View(); }
}
