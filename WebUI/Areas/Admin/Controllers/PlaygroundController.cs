using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "OwnerOrAdmin")]
public class PlaygroundController(
    IWebHostEnvironment environment,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "UI Test";
        ViewData["CanThrowServerError"] = CanThrowServerError();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public IActionResult ThrowServerError()
    {
        if (!CanThrowServerError())
        {
            return NotFound();
        }

        throw new InvalidOperationException("Intentional admin 500 simulation.");
    }

    private bool CanThrowServerError()
    {
        return environment.IsDevelopment()
            || environment.IsStaging()
            || configuration.GetValue<bool>("Diagnostics:EnableAdmin500Simulation");
    }
}
