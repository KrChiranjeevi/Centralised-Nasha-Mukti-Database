using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class CounsellingController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Counselling Sessions"; return View(); }
}
