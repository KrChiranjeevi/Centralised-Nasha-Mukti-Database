using Microsoft.AspNetCore.Mvc;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Dashboard");

    public IActionResult Error()
    {
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    public IActionResult Status(int code)
    {
        if (code == 404)
            return View("~/Views/Shared/NotFound.cshtml");
        return View("~/Views/Shared/Error.cshtml");
    }
}
