using Application.Auths.Queries;
using Application.Auths.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class UserController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(GetAllUsersWithRolesQuery req, CancellationToken ct)
    {
        var users = await mediator.Send(req, ct);
        return View(users);
    }

    [HttpGet("Update/{id}")]
    public async Task<IActionResult> Update(string id, CancellationToken ct)
    {
        var users = await mediator.Send(new GetAllUsersWithRolesQuery { PageSize = 1000 }, ct);
        var user = users.Items.FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();

        var command = new UpdateUserCommand
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        return View(command);
    }

    [HttpPost("Update/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateUserCommand command, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(command);

        var result = await mediator.Send(command, ct);
        if (result)
        {
            TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
        }

        ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteUserCommand { Id = id }, ct);
        if (result)
        {
            TempData["Success"] = "Kullanıcı başarıyla silindi.";
        }
        else
        {
            TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";
        }
        return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
    }
}



