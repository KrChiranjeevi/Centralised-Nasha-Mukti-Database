using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NashaMuktiDB.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    public IActionResult Index() { ViewData["Title"] = "Follow-Ups"; return View(); }
}
