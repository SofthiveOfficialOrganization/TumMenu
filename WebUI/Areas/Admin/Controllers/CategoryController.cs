using Application.Categories.Queries;
using Application.Menus.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
public class CategoryController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Create(Guid menuId, CancellationToken ct)
    {
        var categoryLibraryItems = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery { PageSize = 1000 }, ct);
        
        ViewBag.CategoryLibraryItems = new SelectList(categoryLibraryItems.Items, "Id", "Title");
        
        return View(new AddCategoryToMenuCommand { MenuId = menuId });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddCategoryToMenuCommand cmd, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
             var categoryLibraryItems = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery { PageSize = 1000 }, ct);
             ViewBag.CategoryLibraryItems = new SelectList(categoryLibraryItems.Items, "Id", "Title");
             return View(cmd);
        }

        await mediator.Send(cmd, ct);
        return RedirectToAction("Details", "Menu", new { id = cmd.MenuId });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid menuId, CancellationToken ct)
    {
        await mediator.Send(new RemoveCategoryFromMenuCommand { Id = id }, ct);
        return RedirectToAction("Details", "Menu", new { id = menuId });
    }
}
