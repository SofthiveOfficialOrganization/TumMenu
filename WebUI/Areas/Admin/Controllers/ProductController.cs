using Application.Products.Commands;
using Application.Products.DTOs;
using Application.Products.Queries;
using Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> Index([FromQuery] string? search, int page = 1, CancellationToken ct = default)
    {
        if (User.IsInRole("Admin"))
        {
            var req = new GetAllProductsPagedQuery { Search = search, Page = page, PageSize = 20 };
            var products = await mediator.Send(req, ct);
            return View("Index", products);
        }
        else
        {
            var req = new GetProductsPagedByCurrentOwnerQuery { Search = search, Page = page, PageSize = 20 };
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
    public IActionResult Create(Guid categoryId)
    {
        // Notice we are returning the command directly so we have categoryId pre-filled
        return View(new CreateProductCommand(string.Empty, null, null, 0, true, null, null, null, 0, null, categoryId));
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCommand req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(req);

        await mediator.Send(req, ct);
        // Redirect back to the category details page where the product was created
        return RedirectToAction("Details", "Category", new { id = req.CategoryId, role = RouteData.Values["role"] });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), ct);
        return View(product);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductCommand req, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(req);

        await mediator.Send(req, ct);
        // Fetch after update to get the categoryId for redirect (avoids EF tracking conflict)
        var product = await mediator.Send(new GetProductByIdQuery(req.Id), ct);
        return RedirectToAction("Details", "Category", new { id = product.CategoryId, role = RouteData.Values["role"] });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid categoryId, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        TempData["Success"] = "Ürün başarıyla silindi.";
        return RedirectToAction("Details", "Category", new { id = categoryId, role = RouteData.Values["role"] });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSortOrder([FromBody] UpdateProductSortOrderCommand cmd, CancellationToken ct)
    {
        await mediator.Send(cmd, ct);
        return Ok();
    }
}





