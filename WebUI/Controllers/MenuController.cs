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
	// Helper method to determine if we should show ads (IsQrSource = true means SHOW ADS)
	private bool ShouldTreatAsQrSource(bool requestedIsQr)
	{
		// 1. If URL explicitly says it's from our legitimate /q/ endpoint, it's definitely a QR
		if (requestedIsQr) return true;

		// 2. Check if user has the platform cookie (came from Home/Search)
		var hasPlatformCookie = Request.Cookies.ContainsKey("FromTumMenuPlatform");
		
		// 3. Check Referer
		var referer = Request.Headers.Referer.ToString();
		var isFromOurDomain = !string.IsNullOrEmpty(referer) && referer.Contains(Request.Host.Value);

		// If they don't have our platform cookie AND they didn't come from our domain (direct link)
		// -> Treat them as QR (Show Ads) to prevent bypass.
		// If they have the cookie or came from our domain, they are a platform user -> Don't show ads.
		return !hasPlatformCookie && !isFromOurDomain;
	}

	// GET /{companySlug}/{storeSlug}
	public async Task<IActionResult> Index(string companySlug, string storeSlug, [FromQuery] bool isQr = false)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
        ViewBag.IsQrSource = ShouldTreatAsQrSource(isQr);
		return View(menu);
	}

	public async Task<IActionResult> Category(string companySlug, string storeSlug, string categorySlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetCategoryBySlugQuery(companySlug, storeSlug, categorySlug));
        ViewBag.IsQrSource = ShouldTreatAsQrSource(isQr);

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
        ViewBag.IsQrSource = ShouldTreatAsQrSource(isQr);

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
