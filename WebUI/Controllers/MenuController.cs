using Application.Menus.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public class MenuController(ISender sender) : Controller
{
	// GET /{companySlug}/{storeSlug}/menu
	public async Task<IActionResult> Index(string companySlug, string storeSlug)
	{
		var menu = await sender.Send(new GetActiveMenuBySlugQuery(companySlug, storeSlug));
		return View(menu);
	}
}
