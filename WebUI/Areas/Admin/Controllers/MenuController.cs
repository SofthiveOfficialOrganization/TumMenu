using Application.Companies.Commands;
using Application.Companies.DTOs;
using Application.Companies.Queries;
using Application.Menus.Commands;
using Application.Menus.Queries;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        {
            if (cmd.StoreId != Guid.Empty)
            {
                // Try to find the store to repopulate the name
                try 
                {
                    var store = await mediator.Send(new GetStoreByIdQuery(cmd.StoreId), ct);
                    ViewBag.StoreName = store.Title;
                }
                catch 
                { 
                    // Ignore if not found, user will just see empty name
                }
            }
            return View(cmd);
        }

        var dto = await mediator.Send(cmd, ct);
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public IActionResult CreateToCompany()
    {
        return View(new CreateMenuToCompanyCommand());
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateToCompany([FromForm] CreateMenuToCompanyCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            if (cmd.CompanyId != Guid.Empty)
            {
                try
                {
                    var company = await mediator.Send(new GetCompanyByIdQuery { Id = cmd.CompanyId }, ct);
                    ViewBag.CompanyName = company.Title;
                }
                catch
                {
                    // Ignore
                }
            }
            return View(cmd);
        }

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
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> SearchCompanies(string? search, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        IEnumerable<CompanyDTO> items;
        long totalCount = 0;

        if (User.IsInRole("Admin"))
        {
            var result = await mediator.Send(new GetAllCompaniesPagedQuery 
            { 
                Search = search, 
                Page = page - 1, 
                PageSize = pageSize 
            }, ct);
            items = result.Items;
            totalCount = result.Count;
        }
        else
        {
            var result = await mediator.Send(new GetCompaniesPagedByCurrentOwnerQuery 
            { 
                Search = search, 
                Page = page - 1, 
                PageSize = pageSize 
            }, ct);
            items = result.Items;
            totalCount = result.Count;
        }

        return Json(new { items, totalCount });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> SearchStores(string? search, Guid? companyId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetStoresPagedQuery 
        { 
            Search = search, 
            CompanyId = companyId,
            Page = page - 1, 
            PageSize = pageSize 
        }, ct);

        return Json(new { items = result.Items, totalCount = result.Count });
    }

    [HttpGet("[action]")]
    public IActionResult DivideByZeroError()
    {
        int zero = 0;
        int result = 1 / zero;
        return View();
    }
}
