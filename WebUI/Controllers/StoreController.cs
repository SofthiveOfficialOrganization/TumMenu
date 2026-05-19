using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[AllowAnonymous]
public sealed class StoreController(ISender sender) : Controller
{
	public async Task<IActionResult> Public(string companySlug, string storeSlug, CancellationToken ct)
	{
		var model = await sender.Send(new GetPublicStorePageQuery(companySlug, storeSlug), ct);

		ViewData["Title"] = $"{model.StoreTitle} – İşletme";
		ViewData["Description"] =
			$"{model.StoreTitle} iletişim, konum ve görseller. Menüye buradan geçebilirsiniz.";
		ViewData["OgImage"] = model.LogoUrl ?? model.BannerUrl;
		ViewData["HideFooter"] = true;
		ViewData["CompactHeader"] = true;

		return View(model);
	}
}
