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
	public async Task<IActionResult> Index(Guid? companyId, int page = 1, CancellationToken ct = default)
	{
		if (!companyId.HasValue && User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany != null)
			{
				return RedirectToAction(nameof(Index), new { companyId = ownerCompany.Id });
			}
		}

        if (companyId.HasValue)
        {
            var company = await mediator.Send(new GetCompanyByIdQuery { Id = companyId.Value }, ct);
            ViewBag.CurrentCompanyName = company.Title;
        }
		ViewBag.CurrentCompanyId = companyId;

		var query = new GetStoresPaginatedByCurrentUserQuery
		{
			CompanyId = companyId,
			Page = page,
			PageSize = 20 
		};
		
		var stores = await mediator.Send(query, ct);
		return View(stores);
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
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
		var store = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index), new { companyId = store.CompanyId });
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Update(Guid id, CancellationToken ct)
	{
		var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
        // We might want to show company info or allow changing it (if admin)
        // For now just basic update
		return View(store);
	}

	[HttpPost("[action]")]
	public async Task<IActionResult> Update(UpdateStoreCommand req, CancellationToken ct)
	{
		await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index)); 
        // Or redirect to Details/Index with filter? 
        // For simplicity index is fine, user can filter again.
	}

	[HttpPost("[action]/{id}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
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
