using Application.Auths.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UserController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(GetAllUsersWithRolesQuery req, CancellationToken ct)
    {
        var users = await mediator.Send(req, ct);
        return View(users);
    }
}
