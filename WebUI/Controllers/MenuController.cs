using Application.Categories.Commands;
using Application.Categories.DTOs;
using Application.Products.Commands;
using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public class MenuController(ISender sender, IServiceScopeFactory scopeFactory, ILogger<MenuController> logger) : Controller
{
	// GET /{companySlug}/{storeSlug}
	public async Task<IActionResult> Index(string companySlug, string storeSlug, [FromQuery] bool isQr = false)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
        ApplyPublicMenuRobots(menu);
        ViewBag.IsQrSource = true;
		return View(menu);
	}

	public async Task<IActionResult> Category(string companySlug, string storeSlug, string categorySlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetCategoryBySlugQuery(companySlug, storeSlug, categorySlug));
        ApplyPublicMenuRobots(model);
        ViewBag.IsQrSource = true;

        // Background Recording
        var categoryId = model.CategoryId;
        var userAgent = Request.Headers.UserAgent.ToString();
        var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new RecordCategoryViewCommand { CategoryId = categoryId, UserAgent = userAgent, IpAddress = remoteIp });
            }
            catch (Exception ex) { logger.LogWarning(ex, "Category view recording failed for CategoryId={CategoryId}", categoryId); }
        });

		return View(model);
	}

	public async Task<IActionResult> Product(string companySlug, string storeSlug, string categorySlug, string productSlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetProductBySlugQuery(companySlug, storeSlug, categorySlug, productSlug));
        ApplyPublicMenuRobots(model);
        ViewBag.IsQrSource = true;

        // Background Recording
        var productId = model.ProductId;
        var userAgent = Request.Headers.UserAgent.ToString();
        var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                await mediator.Send(new RecordProductViewCommand { ProductId = productId, UserAgent = userAgent, IpAddress = remoteIp });
            }
            catch (Exception ex) { logger.LogWarning(ex, "Product view recording failed for ProductId={ProductId}", productId); }
        });

		return View(model);
	}

    private void ApplyPublicMenuRobots(Application.Menus.DTOs.MenuDTO menu)
    {
        var categories = menu.Categories.Where(c => c.IsActive).ToList();
        var activeProductCount = categories.Sum(CountActiveProducts);

        if (IsPlaceholderText(menu.CompanySlug) ||
            IsPlaceholderText(menu.CompanyName) ||
            IsPlaceholderText(menu.StoreSlug) ||
            IsPlaceholderText(menu.StoreName) ||
            categories.Count == 0 ||
            activeProductCount == 0)
        {
            ViewData["Robots"] = "noindex,follow";
        }
    }

    private void ApplyPublicMenuRobots(CategoryPageDTO category)
    {
        if (IsPlaceholderText(category.CompanySlug) ||
            IsPlaceholderText(category.CompanyName) ||
            IsPlaceholderText(category.StoreSlug) ||
            IsPlaceholderText(category.StoreName) ||
            IsPlaceholderText(category.CategorySlug) ||
            IsPlaceholderText(category.CategoryTitle) ||
            ((category.Products?.Count ?? 0) == 0 && (category.SubCategories?.Count ?? 0) == 0))
        {
            ViewData["Robots"] = "noindex,follow";
        }
    }

    private void ApplyPublicMenuRobots(ProductPageDTO product)
    {
        if (IsPlaceholderText(product.CompanySlug) ||
            IsPlaceholderText(product.CompanyName) ||
            IsPlaceholderText(product.StoreSlug) ||
            IsPlaceholderText(product.StoreName) ||
            IsPlaceholderText(product.CategorySlug) ||
            IsPlaceholderText(product.CategoryTitle) ||
            IsPlaceholderText(product.ProductSlug) ||
            IsPlaceholderText(product.ProductTitle) ||
            string.IsNullOrWhiteSpace(product.ProductDescription) ||
            product.ProductDescription.Trim().Length < 20)
        {
            ViewData["Robots"] = "noindex,follow";
        }
    }

    private static bool IsPlaceholderText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized.Contains("test") ||
            normalized.Contains("deneme") ||
            normalized.Contains("dummy") ||
            normalized.Contains("sample") ||
            normalized.Contains("ornek") ||
            normalized.Contains("örnek") ||
            normalized.Contains("lorem");
    }

    private static int CountActiveProducts(CategoryDTO category)
    {
        var directProductCount = category.Products?.Count(p => p.IsActive) ?? 0;
        var subCategoryProductCount = category.SubCategories?.Where(c => c.IsActive).Sum(CountActiveProducts) ?? 0;

        return directProductCount + subCategoryProductCount;
    }
}
