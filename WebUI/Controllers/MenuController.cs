using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public class MenuController(ISender sender) : Controller
{
	// GET /{companySlug}/{storeSlug}
	public async Task<IActionResult> Index(string companySlug, string storeSlug)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
		return View(menu);
	}

	// GET /{companySlug}/{storeSlug}/{categorySlug}
	public async Task<IActionResult> Category(string companySlug, string storeSlug, string categorySlug)
	{
		var model = await sender.Send(new GetCategoryBySlugQuery(companySlug, storeSlug, categorySlug));
		return View(model);
	}

	// GET /{companySlug}/{storeSlug}/{categorySlug}/{productSlug}
	public async Task<IActionResult> Product(string companySlug, string storeSlug, string categorySlug, string productSlug)
	{
		var model = await sender.Send(new GetProductBySlugQuery(companySlug, storeSlug, categorySlug, productSlug));
		return View(model);
	}
}
