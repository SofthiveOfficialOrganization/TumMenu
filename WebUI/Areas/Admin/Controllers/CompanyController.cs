using Application.Companies.Commands;
using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class CompanyController(IMediator mediator) : Controller
{
	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken ct)
	{
		if (User.IsInRole("Admin"))
		{
			var companies = await mediator.Send(new GetAllCompaniesPagedQuery(), ct);
			return View("AllCompanies", companies);
		}
		else
		{
			var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (company == null)
				return RedirectToAction(nameof(Create), new { role = RouteData.Values["role"] });
			return RedirectToAction(nameof(Details), new { area = "Admin", id = company.Id, role = RouteData.Values["role"] ?? (User.IsInRole("Admin") ? "admin" : "owner") });
		}
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public IActionResult Create(CancellationToken ct)
	{
		return View(new CreateCompanyCommand());
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateCompanyCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		var dto = await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Details), new { area = "Admin", id = dto.Id, role = RouteData.Values["role"] ?? (User.IsInRole("Admin") ? "admin" : "owner") });
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Details(Guid id, string? returnUrl, CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByIdQuery { Id = id }, ct);
		ViewData["ReturnUrl"] = returnUrl;
		
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || ownerCompany.Id != company.Id)
			{
				return Forbid();
			}
		}

		return View(company);
	}



	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Update(Guid id, CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByIdQuery { Id = id }, ct);
		
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || ownerCompany.Id != company.Id)
			{
				return Forbid();
			}
		}

		var cmd = new UpdateCompanyCommand
		{
			Id = company.Id,
			Title = company.Title,
			Slug = company.Slug
		};
		return View(cmd);
	}
	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	public async Task<IActionResult> Update([FromForm] UpdateCompanyCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || ownerCompany.Id != req.Id)
			{
				return Forbid();
			}
		}

		var company = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { area = "Admin", id = company.Id, role = RouteData.Values["role"] ?? (User.IsInRole("Admin") ? "admin" : "owner") });
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		if (User.IsInRole("Owner"))
		{
			var ownerCompany = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
			if (ownerCompany == null || ownerCompany.Id != id)
			{
				return Forbid();
			}
		}

		await mediator.Send(new DeleteCompanyCommand { Id = id }, ct);
		
		if (User.IsInRole("Owner"))
		{
			// Owner sildiğinde, yeni şirket oluşturmaya yönlendir (veya MyCompany action'ına)
			return RedirectToAction(nameof(Create), new { role = RouteData.Values["role"] });
		}
		
		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
	}
	[HttpGet]
	public IActionResult DivideByZeroError()
	{
		int zero = 0;
		int result = 1 / zero;
		return View();
	}

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> Search(string? term, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = new GetCompanyListForSearchQuery
        {
            SearchTerm = term,
            Page = page,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, ct);
        return Json(result);
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> CheckSlug(string slug, Guid? excludeId, CancellationToken ct)
    {
        var result = await mediator.Send(new CheckCompanySlugQuery { Slug = slug, ExcludeId = excludeId }, ct);
        return Json(new { available = result.Available, message = result.Message });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet]
    public async Task<IActionResult> GetCompaniesForSelect2(string? search, int page = 1, int pageSize = 15, CancellationToken ct = default)
    {
        var query = new GetCompanyListForSearchQuery { SearchTerm = search, Page = page, PageSize = pageSize };
        var result = await mediator.Send(query, ct);
        var items = result.Items.Select(c => new { id = c.Id, text = c.Title });
        return Json(new { results = items, pagination = new { more = result.HasNext } });
    }
}




