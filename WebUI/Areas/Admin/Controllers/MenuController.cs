using Application.Companies.Commands;
using Application.Companies.Queries;
using Application.Menus.Commands;
using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
public sealed class MenuController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
         var menus = await mediator.Send(new GetAllMenusPagedQuery(), ct);
         return View("AllMenus", menus);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> CreateToStore(CancellationToken ct)
    {
        return View(new CreateMenuToStoreCommand());
    }
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateToStore([FromForm] CreateMenuToStoreCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(cmd);

        var dto = await mediator.Send(cmd, ct);
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> CreateToCompany(CancellationToken ct)
    {
        return View(new CreateMenuToCompanyCommand());
    }
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateToCompany([FromForm] CreateMenuToCompanyCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(cmd);

        var dto = await mediator.Send(cmd, ct);
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
        return View(menu);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("[action]")]
    public async Task<IActionResult> AllMenus(GetAllMenusPagedQuery req, CancellationToken ct)
    {
        var menus = await mediator.Send(req, ct);
        return View(menus);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> MyMenus(CancellationToken ct)
    {
        var menus = await mediator.Send(new GetMenusPagedByCurrentOwnerQuery(), ct);
        return View(menus);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CancellationToken ct)
    {
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = id }, ct);
        return View(new UpdateMenuCommand(menu.Id, menu.Name));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateMenuCommand req, CancellationToken ct)
    {
        var menu = await mediator.Send(req, ct);
        return RedirectToAction(nameof(Details), new { id = menu.Id });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteMenuCommand { Id = id }, ct);
        return RedirectToAction(nameof(AllMenus));
    }
    [HttpGet("[action]")]
    public IActionResult DivideByZeroError()
    {
        int zero = 0;
        int result = 1 / zero;
        return View();
    }
}
