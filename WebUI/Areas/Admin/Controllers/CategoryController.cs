using Application.Categories.Queries;
using Application.Categories.Commands;
using Application.Menus.Commands;
using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

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

        // Pass exclude IDs as comma-separated string for AJAX calls
        ViewBag.ExcludeIds = string.Join(",", existingLibraryItemIds);
        
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
             var existingLibraryItemIds = menu.Categories.Select(c => c.CategoryLibraryItemId).ToList();
             ViewBag.ExcludeIds = string.Join(",", existingLibraryItemIds);
             return View(cmd);
        }

        await mediator.Send(cmd, ct);
        TempData["Success"] = "Kategori başarıyla eklendi.";
        return RedirectToAction("Create", "Category", new { menuId = cmd.MenuId });
    }

    /// <summary>
    /// AJAX endpoint for Select2: returns category library items matching search, excluding given IDs.
    /// </summary>
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCategoryLibraryItems(
        string? search,
        string? excludeIds,
        int page = 1,
        int pageSize = 15,
        CancellationToken ct = default)
    {
        var excludeList = string.IsNullOrWhiteSpace(excludeIds)
            ? new List<Guid>()
            : excludeIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.TryParse(id.Trim(), out var g) ? g : (Guid?)null)
                        .Where(g => g.HasValue)
                        .Select(g => g!.Value)
                        .ToList();

        var result = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery
        {
            Search = search,
            ExcludeIds = excludeList,
            Page = page - 1,
            PageSize = pageSize
        }, ct);

        var items = result.Items.Select(i => new { id = i.Id, text = i.Title });
        var hasMore = result.HasNext;

        return Json(new { results = items, pagination = new { more = hasMore } });
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
