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
public class CategoryController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Create(Guid menuId, Guid? parentId, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;

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
        
        string? parentName = null;
        if (parentId.HasValue)
        {
            var parentCat = menu.Categories.FirstOrDefault(c => c.Id == parentId.Value);
            if (parentCat != null)
            {
                parentName = parentCat.CategoryLibraryItem.Title;
            }
        }
        ViewBag.ParentName = parentName;

        return View(new AddCategoryToMenuCommand { MenuId = menuId, ParentId = parentId });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddCategoryToMenuCommand cmd, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;

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
        return RedirectToLocal(returnUrl, RedirectToAction("Create", "Category", new { menuId = cmd.MenuId, role = RouteData.Values["role"] }));
    }

    /// <summary>
    /// AJAX endpoint for Select2: returns category library items matching search, excluding given IDs.
    /// </summary>
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetCategoryLibraryItems(
        string? search,
        string? excludeIds,
        Guid? menuId,
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

        if (menuId.HasValue && menuId.Value != Guid.Empty)
        {
            var menu = await mediator.Send(new GetMenuByIdQuery { Id = menuId.Value }, ct);
            excludeList.AddRange(menu.Categories.Select(c => c.CategoryLibraryItemId));
            excludeList = excludeList.Distinct().ToList();
        }

        var result = await mediator.Send(new GetAllCategoryLibraryItemsPagedQuery
        {
            Search = search,
            ExcludeIds = excludeList,
            Page = page,
            PageSize = pageSize
        }, ct);

        var items = result.Items.Select(i => new { id = i.Id, text = i.Title });
        var hasMore = result.HasNext;

        return Json(new { results = items, pagination = new { more = hasMore } });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid menuId, string? returnUrl, CancellationToken ct)
    {
        await mediator.Send(new RemoveCategoryFromMenuCommand { Id = id }, ct);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Details", "Menu", new { id = menuId, role = RouteData.Values["role"] });
    }
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSortOrder([FromBody] UpdateCategorySortOrderCommand cmd, CancellationToken ct)
    {
        await mediator.Send(cmd, ct);
        return Ok();
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, string? returnUrl, CancellationToken ct)
    {
        var category = await mediator.Send(new GetCategoryByIdQuery(id), ct);
        ViewData["ReturnUrl"] = returnUrl;
        
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

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetVisibility(Guid id, bool isActive, string? returnUrl, CancellationToken ct)
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

        await mediator.Send(new UpdateCategorySortOrderCommand
        {
            Items =
            [
                new CategorySortItem
                {
                    Id = id,
                    SortOrder = category.SortOrder,
                    IsActive = isActive
                }
            ]
        }, ct);

        TempData["Success"] = isActive
            ? "Kategori müşterilere gösterilecek."
            : "Kategori müşterilerden gizlendi.";

        return RedirectToLocal(returnUrl, RedirectToAction("Details", "Category", new { id, role = RouteData.Values["role"] }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] string? search,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? menuId,
        int page = 1,
        CancellationToken ct = default)
    {
        var req = new GetCategoriesPagedByCurrentOwnerQuery
        {
            Search = search,
            CompanyId = companyId,
            StoreId = storeId,
            MenuId = menuId,
            Page = page,
            PageSize = 20
        };
        var result = await mediator.Send(req, ct);
        return View("MyCategories", result);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetCategoriesForSelect2(Guid? companyId, Guid? storeId, Guid? menuId, string? search, int page = 1, int pageSize = 15, CancellationToken ct = default)
    {
        var query = new GetCategoriesPagedByCurrentOwnerQuery
        {
            CompanyId = companyId,
            StoreId = storeId,
            MenuId = menuId,
            Search = search,
            Page = page,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, ct);
        var items = result.Items.Select(c => new { id = c.Id, text = c.Title });
        return Json(new { results = items, pagination = new { more = result.HasNext } });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> CreateCategory(string? returnUrl, CancellationToken ct)
    {
        var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
        var isSingleStore = ownerCompany?.IsSingleStore == true;
        ViewData["ReturnUrl"] = returnUrl;
        
        ViewBag.IsSingleStore = isSingleStore;
        ViewBag.SingleStoreId = (Guid?)null;
        
        if (isSingleStore && User.IsInRole("Owner"))
        {
            // Tek dükkan modunda şirketin dükkanını bul
            var storesQuery = new Application.Stores.Queries.GetStoresPagedQuery
            {
                CompanyId = ownerCompany!.Id,
                Page = 1,
                PageSize = 2
            };
            var storesResult = await mediator.Send(storesQuery, ct);
            
            if (storesResult.Items.Count == 1)
            {
                ViewBag.SingleStoreId = storesResult.Items.First().Id;
            }
        }
        
        return View(new AddCategoryToMenuCommand { IsActive = true });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Guid? storeId, Guid? menuId, Guid? parentId, Guid? categoryLibraryItemId, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["SelectedStoreId"] = storeId?.ToString() ?? string.Empty;
        ViewData["SelectedMenuId"] = menuId?.ToString() ?? string.Empty;
        ViewData["SelectedParentId"] = parentId?.ToString() ?? string.Empty;
        ViewData["SelectedCategoryLibraryItemId"] = categoryLibraryItemId?.ToString() ?? string.Empty;

        var command = new AddCategoryToMenuCommand
        {
            MenuId = menuId ?? Guid.Empty,
            ParentId = parentId,
            CategoryLibraryItemId = categoryLibraryItemId ?? Guid.Empty,
            IsActive = true
        };

        // Tek dükkan modu kontrolü
        var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
        var isSingleStore = ownerCompany?.IsSingleStore == true && User.IsInRole("Owner");
        
        if (isSingleStore)
        {
            // Tek dükkan modunda storeId otomatik olarak ayarlanır
            if (!storeId.HasValue || storeId.Value == Guid.Empty)
            {
                var storesQuery = new Application.Stores.Queries.GetStoresPagedQuery
                {
                    CompanyId = ownerCompany!.Id,
                    Page = 1,
                    PageSize = 2
                };
                var storesResult = await mediator.Send(storesQuery, ct);
                
                if (storesResult.Items.Count == 1)
                {
                    storeId = storesResult.Items.First().Id;
                    ViewData["SelectedStoreId"] = storeId.ToString()!;
                }
            }
        }
        else
        {
            // Çoklu dükkan modunda dükkan seçimi zorunlu
            if (!storeId.HasValue || storeId.Value == Guid.Empty)
                ModelState.AddModelError(nameof(storeId), "Dükkan seçimi zorunludur.");
        }
        
        if (!menuId.HasValue || menuId.Value == Guid.Empty)
            ModelState.AddModelError(nameof(menuId), "Menü seçimi zorunludur.");
        if (!categoryLibraryItemId.HasValue || categoryLibraryItemId.Value == Guid.Empty)
            ModelState.AddModelError(nameof(categoryLibraryItemId), "Kategori seçimi zorunludur.");

        if (!ModelState.IsValid)
        {
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = storeId;
            return View(command);
        }

        var selectedStoreId = storeId.GetValueOrDefault();
        var selectedMenuId = menuId.GetValueOrDefault();

        // Security Check
        var menu = await mediator.Send(new GetMenuByIdQuery { Id = selectedMenuId }, ct);
        if (menu.StoreId != selectedStoreId)
        {
            ModelState.AddModelError(nameof(menuId), "Seçilen menü bu dükkana ait değil.");
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = storeId;
            return View(command);
        }

        if (User.IsInRole("Owner"))
        {
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

        await mediator.Send(command, ct);
        TempData["Success"] = "Kategori başarıyla eklendi.";
        return RedirectToLocal(returnUrl, RedirectToAction("Index"));
    }

    private IActionResult RedirectToLocal(string? returnUrl, IActionResult fallback)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return fallback;
    }
}



