using Application.Categories.Commands;
using Application.Products.Commands;
using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public class MenuController(ISender sender, IServiceScopeFactory scopeFactory) : Controller
{
	// GET /{companySlug}/{storeSlug}
	public async Task<IActionResult> Index(string companySlug, string storeSlug, [FromQuery] bool isQr = false)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
        ViewBag.IsQrSource = true; // Show ads to all visitors for AdSense approval
		return View(menu);
	}

	public async Task<IActionResult> Category(string companySlug, string storeSlug, string categorySlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetCategoryBySlugQuery(companySlug, storeSlug, categorySlug));
        ViewBag.IsQrSource = true; // Show ads to all visitors for AdSense approval

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
                await mediator.Send(new RecordCategoryViewCommand(categoryId, userAgent, remoteIp));
            }
            catch { /* Silent */ }
        });

		return View(model);
	}

	public async Task<IActionResult> Product(string companySlug, string storeSlug, string categorySlug, string productSlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetProductBySlugQuery(companySlug, storeSlug, categorySlug, productSlug));
        ViewBag.IsQrSource = true; // Show ads to all visitors for AdSense approval

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
                await mediator.Send(new RecordProductViewCommand(productId, userAgent, remoteIp));
            }
            catch { /* Silent */ }
        });

		return View(model);
	}
}
