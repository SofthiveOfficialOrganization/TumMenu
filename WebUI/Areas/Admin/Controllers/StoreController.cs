using Application.Companies.Queries;
using Application.Stores.Commands;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
[Authorize(Roles = "Admin,Owner")]
public class StoreController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index(Guid? companyId, string? search, int page = 1, CancellationToken ct = default)
	{
		bool isFixedCompany = false;
		if (!companyId.HasValue && User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany != null)
			{
				return RedirectToAction(nameof(Index), new { companyId = ownerCompany.Id, search });
			}
		}
		
		if (User.IsInRole("Owner"))
		{
			// Verify if the requested CompanyId matches the Owner's company to consider it "fixed"
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany != null && companyId == ownerCompany.Id)
			{
				isFixedCompany = true;
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
			Page = page - 1,
			PageSize = 20 
		};
		
		var stores = await mediator.Send(query, ct);
		return View(stores);
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
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

	[HttpGet("[action]")]
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

	[HttpPost("[action]")]
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
		return RedirectToAction(nameof(Index), new { companyId = store.CompanyId });
	}

	[HttpGet("[action]/{id}")]
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

	[HttpPost("[action]")]
	public async Task<IActionResult> Update(UpdateStoreCommand req, CancellationToken ct)
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
		return RedirectToAction(nameof(Index)); 
	}

	[HttpPost("[action]/{id}")]
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
		return RedirectToAction(nameof(Index));
	}

	[HttpGet("[action]/{companyId}")]
	public IActionResult ByCompany(Guid companyId)
	{
		return RedirectToAction(nameof(Index), new { companyId });
	}
	[HttpGet("[action]")]
	public async Task<IActionResult> CheckSlug(string slug, Guid? excludeId, CancellationToken ct)
	{
		var result = await mediator.Send(new CheckStoreSlugQuery { Slug = slug, ExcludeId = excludeId }, ct);
		return Json(result);
	}
}
