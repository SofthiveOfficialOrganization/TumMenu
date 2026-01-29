using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
// [Authorize(Policy = "OwnerOrAdmin")] // Commented out for easier testing, enable later if needed
public class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        return View();
    }
}
