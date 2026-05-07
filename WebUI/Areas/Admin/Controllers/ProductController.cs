using Application.Products.Commands;
using Application.Products.DTOs;
using Application.Products.Queries;
using Application.Categories.Queries;
using Application.Menus.Queries;
using Application.Medias.Commands;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController(IMediator mediator) : Controller
{
    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] string? search,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? storeId,
        [FromQuery] Guid? menuId,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageIndex = 0,
        CancellationToken ct = default)
    {
        var currentPage = pageIndex > 0 ? pageIndex : page;

        if (User.IsInRole("Admin"))
        {
            var req = new GetAllProductsPagedQuery { Search = search, Page = currentPage, PageSize = 20 };
            var products = await mediator.Send(req, ct);
            return View("Index", products);
        }
        else
        {
            var req = new GetProductsPagedByCurrentOwnerQuery
            {
                Search = search,
                CompanyId = companyId,
                StoreId = storeId,
                MenuId = menuId,
                CategoryId = categoryId,
                Page = currentPage,
                PageSize = 20
            };
            var result = await mediator.Send(req, ct);
            return View("MyProducts", result);
        }
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Details(Guid id, string? returnUrl, CancellationToken ct)
    {
        ProductDTO? product = await mediator.Send(new GetProductByIdQuery(id), ct);
        ViewData["ReturnUrl"] = returnUrl;
        return View(product);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public IActionResult Create(Guid categoryId, string? returnUrl)
    {
        // Notice we are returning the command directly so we have categoryId pre-filled
        ViewData["ReturnUrl"] = returnUrl;
        return View(new CreateProductCommand { CategoryId = categoryId, IsActive = true });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCommand req, List<IFormFile>? photoFiles, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(req);
        }

        var product = await mediator.Send(req, ct);
        await UploadProductPhotosAsync(product.Id, photoFiles, product.Title, ct);

        // Redirect back to the category details page where the product was created
        return RedirectToLocal(returnUrl, RedirectToAction("Details", "Category", new { id = req.CategoryId, role = RouteData.Values["role"] }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> CreateProduct(string? returnUrl, CancellationToken ct)
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
        
        return View(new CreateProductCommand { IsActive = true });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Guid storeId, Guid menuId, Guid categoryId, CreateProductCommand req, List<IFormFile>? photoFiles, string? returnUrl, CancellationToken ct)
    {
        req.CategoryId = categoryId;
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["SelectedStoreId"] = storeId;
        ViewData["SelectedMenuId"] = menuId;
        ViewData["SelectedCategoryId"] = categoryId;

        // Tek dükkan modu kontrolü
        var ownerCompany = await mediator.Send(new Application.Companies.Queries.GetCompanyByCurrentOwnerQuery(), ct);
        var isSingleStore = ownerCompany?.IsSingleStore == true && User.IsInRole("Owner");
        
        if (isSingleStore)
        {
            // Tek dükkan modunda storeId otomatik olarak ayarlanır
            if (storeId == Guid.Empty)
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
                    ViewData["SelectedStoreId"] = storeId;
                }
            }
        }
        else
        {
            // Çoklu dükkan modunda dükkan seçimi zorunlu
            if (storeId == Guid.Empty)
                ModelState.AddModelError(nameof(storeId), "Dükkan seçimi zorunludur.");
        }
        
        if (menuId == Guid.Empty)
            ModelState.AddModelError(nameof(menuId), "Menü seçimi zorunludur.");
        if (categoryId == Guid.Empty)
            ModelState.AddModelError(nameof(categoryId), "Kategori seçimi zorunludur.");

        if (!ModelState.IsValid)
        {
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = storeId == Guid.Empty ? (Guid?)null : storeId;
            return View(req);
        }

        var selectedMenu = await mediator.Send(new GetMenuByIdQuery { Id = menuId }, ct);
        if (selectedMenu.StoreId != storeId)
        {
            ModelState.AddModelError(nameof(menuId), "Seçilen menü bu dükkana ait değil.");
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = storeId == Guid.Empty ? (Guid?)null : storeId;
            return View(req);
        }

        var category = await mediator.Send(new GetCategoryByIdQuery(categoryId), ct);
        if (category.MenuId != menuId)
        {
            ModelState.AddModelError(nameof(categoryId), "Seçilen kategori bu menüye ait değil.");
            ViewBag.IsSingleStore = isSingleStore;
            ViewBag.SingleStoreId = storeId == Guid.Empty ? (Guid?)null : storeId;
            return View(req);
        }

        // Security Check - verify user owns the category
        if (User.IsInRole("Owner"))
        {
            bool isOwner = false;
            if (ownerCompany != null)
            {
                if (selectedMenu.CompanyId == ownerCompany.Id) isOwner = true;
                else if (selectedMenu.StoreId.HasValue)
                {
                    var store = await mediator.Send(new Application.Stores.Queries.GetStoreByIdQuery(selectedMenu.StoreId.Value), ct);
                    if (store.CompanyId == ownerCompany.Id) isOwner = true;
                }
            }
            if (!isOwner) return Forbid();
        }

        var product = await mediator.Send(req, ct);
        await UploadProductPhotosAsync(product.Id, photoFiles, product.Title, ct);

        TempData["Success"] = "Ürün başarıyla eklendi.";
        return RedirectToLocal(returnUrl, RedirectToAction("Index"));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, string? returnUrl, CancellationToken ct)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), ct);
        ViewData["ReturnUrl"] = returnUrl;
        return View(product);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductCommand req, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(req);
        }

        await mediator.Send(req, ct);
        // Fetch after update to get the categoryId for redirect (avoids EF tracking conflict)
        var product = await mediator.Send(new GetProductByIdQuery(req.Id), ct);
        return RedirectToLocal(returnUrl, RedirectToAction("Details", "Category", new { id = product.CategoryId, role = RouteData.Values["role"] }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid categoryId, string? returnUrl, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand { ProductId = id }, ct);
        TempData["Success"] = "Ürün başarıyla silindi.";
        return RedirectToLocal(returnUrl, RedirectToAction("Details", "Category", new { id = categoryId, role = RouteData.Values["role"] }));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSortOrder([FromBody] UpdateProductSortOrderCommand cmd, CancellationToken ct)
    {
        await mediator.Send(cmd, ct);
        return Ok();
    }

    private async Task UploadProductPhotosAsync(Guid productId, IEnumerable<IFormFile>? photoFiles, string? altText, CancellationToken ct)
    {
        if (photoFiles == null)
            return;

        var sortOrder = 0;
        foreach (var file in photoFiles.Where(f => f is { Length: > 0 }))
        {
            await mediator.Send(new UploadMediaCommand
            {
                File = file,
                ReferenceId = productId,
                Type = MediaRefType.Product,
                Slot = "default-gallery",
                AltText = altText,
                SortOrder = sortOrder++
            }, ct);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl, IActionResult fallback)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return fallback;
    }
}

