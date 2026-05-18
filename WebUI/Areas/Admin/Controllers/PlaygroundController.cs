using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class PlaygroundController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "UI Test";
        return View();
    }
}

