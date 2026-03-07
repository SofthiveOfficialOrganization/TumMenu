using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public class MenuController(ISender sender) : Controller
{
	// GET /{companySlug}/{storeSlug}
	public async Task<IActionResult> Index(string companySlug, string storeSlug, [FromQuery] bool isQr = false)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
        ViewBag.IsQrSource = isQr;
		return View(menu);
	}

	// GET /{companySlug}/{storeSlug}/{categorySlug}
	public async Task<IActionResult> Category(string companySlug, string storeSlug, string categorySlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetCategoryBySlugQuery(companySlug, storeSlug, categorySlug));
        ViewBag.IsQrSource = isQr;
		return View(model);
	}

	// GET /{companySlug}/{storeSlug}/{categorySlug}/{productSlug}
	public async Task<IActionResult> Product(string companySlug, string storeSlug, string categorySlug, string productSlug, [FromQuery] bool isQr = false)
	{
		var model = await sender.Send(new GetProductBySlugQuery(companySlug, storeSlug, categorySlug, productSlug));
        ViewBag.IsQrSource = isQr;
		return View(model);
	}
}
