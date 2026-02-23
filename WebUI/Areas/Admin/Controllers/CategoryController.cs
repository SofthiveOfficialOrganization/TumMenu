using Application.Categories.Queries;
using Application.Categories.Commands;
using Application.Menus.Commands;
using Application.Menus.Queries;
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
        // 1. Get the menu and its existing categories
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = menuId }, ct);
        
        // Security Check
        if (User.IsInRole("Owner"))
        {
             var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
             bool isOwner = false;
             if (ownerCompany != null)
             {
                 if (menu.CompanyId == ownerCompany.Id) isOwner = true;
                 else if (menu.StoreId.HasValue)
                 {
                     var store = await mediator.Send(new Application.Stores.Queries.GetStoreByIdQuery(menu.StoreId.Value), ct);
                     if (store.CompanyId == ownerCompany.Id) isOwner = true;
                 }
             }
             if (!isOwner) return Forbid();
        }

        // 2. Extract CategoryLibraryItemIds that are already in the menu
        var existingLibraryItemIds = menu.Categories.Select(c => c.CategoryLibraryItemId).ToList();

        // 3. Fetch Library Items, excluding the existing ones
        var categoryLibraryItems = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery 
        { 
            PageSize = 1000,
            ExcludeIds = existingLibraryItemIds
        }, ct);
        
        ViewBag.CategoryLibraryItems = new SelectList(categoryLibraryItems.Items, "Id", "Title");
        
        return View(new AddCategoryToMenuCommand { MenuId = menuId });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddCategoryToMenuCommand cmd, CancellationToken ct)
    {
        // Security Check
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = cmd.MenuId }, ct);
        if (User.IsInRole("Owner"))
        {
             var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
             bool isOwner = false;
             if (ownerCompany != null)
             {
                 if (menu.CompanyId == ownerCompany.Id) isOwner = true;
                 else if (menu.StoreId.HasValue)
                 {
                     var store = await mediator.Send(new Application.Stores.Queries.GetStoreByIdQuery(menu.StoreId.Value), ct);
                     if (store.CompanyId == ownerCompany.Id) isOwner = true;
                 }
             }
             if (!isOwner) return Forbid();
        }

        if (!ModelState.IsValid)
        {
             // Re-fetch existing items to exclude them again
             var existingLibraryItemIds = menu.Categories.Select(c => c.CategoryLibraryItemId).ToList();

             var categoryLibraryItems = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery 
             { 
                 PageSize = 1000,
                 ExcludeIds = existingLibraryItemIds
             }, ct);
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
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSortOrder([FromBody] UpdateCategorySortOrderCommand cmd, CancellationToken ct)
    {
        await mediator.Send(cmd, ct);
        return Ok();
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var category = await mediator.Send(new GetCategoryByIdQuery(id), ct);
        
        // Security Check
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = category.MenuId }, ct);
        if (User.IsInRole("Owner"))
        {
             var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
             bool isOwner = false;
             if (ownerCompany != null)
             {
                 if (menu.CompanyId == ownerCompany.Id) isOwner = true;
                 else if (menu.StoreId.HasValue)
                 {
                     var store = await mediator.Send(new Application.Stores.Queries.GetStoreByIdQuery(menu.StoreId.Value), ct);
                     if (store.CompanyId == ownerCompany.Id) isOwner = true;
                 }
             }
             if (!isOwner) return Forbid();
        }

        return View(category);
    }
}
