using Application.Companies.Queries;
using Application.Stores.Commands;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Owner")]
public class StoreController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index(Guid? companyId, string? search, int page = 1, CancellationToken ct = default)
	{
		bool isFixedCompany = false;
		var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
		
		if (User.IsInRole("Owner"))
		{
			if (ownerCompany != null)
			{
				companyId = ownerCompany.Id;
				isFixedCompany = true;
				
				// Tek dükkan modunda ve sadece 1 dükkan varsa doğrudan detay sayfasına yönlendir
				if (ownerCompany.IsSingleStore)
				{
					var storesQuery = new GetStoresPagedQuery
					{
						CompanyId = companyId,
						Page = 1,
						PageSize = 2 // Sadece 1 dükkan var mı kontrolü için yeterli
					};
					var storesResult = await mediator.Send(storesQuery, ct);
					
					if (storesResult.Items.Count == 1)
					{
						var singleStore = storesResult.Items.First();
						return RedirectToAction(nameof(Details), new { id = singleStore.Id, role = RouteData.Values["role"] });
					}
				}
			}
		}

        if (companyId.HasValue)
        {
            var company = await mediator.Send(new GetCompanyByIdQuery { Id = companyId.Value }, ct);
            ViewBag.CurrentCompanyName = company.Title;
        }
		ViewBag.CurrentCompanyId = companyId;
		ViewBag.CurrentSearch = search;
		ViewBag.IsFixedCompany = isFixedCompany;

		var query = new GetStoresPagedQuery
		{
			CompanyId = companyId,
			Search = search,
			Page = page,
			PageSize = 20 
		};
		
		var stores = await mediator.Send(query, ct);
		return View(stores);
	}

	[HttpGet]
	public async Task<IActionResult> Details(Guid id, string? returnUrl, CancellationToken ct)
	{
		var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
		ViewData["ReturnUrl"] = returnUrl;
		
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || store.CompanyId != ownerCompany.Id)
			{
				return Forbid();
			}
		}

		return View(store);
	}

	[HttpGet]
	public async Task<IActionResult> Create(Guid? companyId, CancellationToken ct)
	{
		bool isFixedCompany = false;
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany != null)
			{
				companyId = ownerCompany.Id;
				ViewBag.CurrentCompanyName = ownerCompany.Title;
				isFixedCompany = true;
			}
		}

        if (!isFixedCompany && companyId.HasValue)
        {
             var company = await mediator.Send(new GetCompanyByIdQuery { Id = companyId.Value }, ct);
             ViewBag.CurrentCompanyName = company.Title;
        }

		ViewBag.IsFixedCompany = isFixedCompany;
		return View(new Application.Stores.DTOs.StoreDTO { CompanyId = companyId ?? Guid.Empty });
	}

	[HttpPost]
	public async Task<IActionResult> Create(CreateStoreCommand req, CancellationToken ct)
	{
		// Owner can only create for their company
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany != null)
			{
				req.CompanyId = ownerCompany.Id; // Force company ID
			}
			else
			{
				return Forbid(); // Should have a company
			}
		}

		var store = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index), new { companyId = store.CompanyId, role = RouteData.Values["role"] });
	}

	[HttpGet]
	public async Task<IActionResult> Update(Guid id, CancellationToken ct)
	{
		var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
		
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || store.CompanyId != ownerCompany.Id)
			{
				return Forbid();
			}
		}

		return View(store);
	}

	[HttpPost]
	public async Task<IActionResult> Update(Guid id, UpdateStoreCommand req, CancellationToken ct)
	{
		if (User.IsInRole("Owner"))
		{
			// Fetch store to verify company (since req might not have companyId or it might be forged - though command usually just updates fields)
			// But we need to know if the store belongs to owner.
			var store = await mediator.Send(new GetStoreByIdQuery(req.Id), ct);
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || store.CompanyId != ownerCompany.Id)
			{
				return Forbid();
			}
		}

		await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] }); 
	}

	[HttpPost]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		if (User.IsInRole("Owner"))
		{
			var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || store.CompanyId != ownerCompany.Id)
			{
				return Forbid();
			}
		}

		await mediator.Send(new DeleteStoreCommand(id), ct);
		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
	}

	[HttpGet]
	public IActionResult ByCompany(Guid id)
	{
		return RedirectToAction(nameof(Index), new { companyId = id, role = RouteData.Values["role"] });
	}
	[HttpGet]
	public async Task<IActionResult> CheckSlug(string slug, Guid companyId, Guid? excludeId, CancellationToken ct)
	{
		var result = await mediator.Send(new CheckStoreSlugQuery { Slug = slug, CompanyId = companyId, ExcludeId = excludeId }, ct);
		return Json(new { available = result.Available, message = result.Message });
	}

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetStoresForSelect2(Guid? companyId, string? search, int page = 1, int pageSize = 15, CancellationToken ct = default)
    {
        var query = new GetStoresPagedQuery { CompanyId = companyId, Search = search, Page = page, PageSize = pageSize };
        var result = await mediator.Send(query, ct);
        var items = result.Items.Select(s => new { id = s.Id, text = s.Title });
        return Json(new { results = items, pagination = new { more = result.HasNext } });
    }
}





