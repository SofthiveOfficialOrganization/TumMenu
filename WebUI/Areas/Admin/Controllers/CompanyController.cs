using Application.Companies.Commands;
using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/[controller]")]
public sealed class CompanyController(IMediator mediator) : Controller
{
	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken ct)
	{
		var companies = await mediator.Send(new GetAllCompaniesPagedQuery(), ct);
		return View("AllCompanies", companies);
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet("[action]")]
	public async Task<IActionResult> Create(CancellationToken ct)
	{
		return View(new CreateCompanyCommand());
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateCompanyCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		var dto = await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Details), new { id = dto.Id });
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet("{id:guid}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByIdQuery { Id = id }, ct);
		return View(company);
	}

	[Authorize(Policy = "AdminOnly")]
	[HttpGet("[action]")]
	public async Task<IActionResult> AllCompanies(GetAllCompaniesPagedQuery req, CancellationToken ct)
	{
		var compaines = await mediator.Send(req, ct);
		return View(compaines);
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet("[action]")]
	public async Task<IActionResult> MyCompanies(CancellationToken ct)
	{
		var companies = await mediator.Send(new GetCompaniesPagedByCurrentOwnerQuery(), ct);
		return View(companies);
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet("[action]")]
	public async Task<IActionResult> MyCompany(CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByCurrentOwnerQuery(), ct);
		if (company == null)
			return RedirectToAction(nameof(Create));
		return RedirectToAction(nameof(Details), new { id = company.Id });
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Update(Guid id, CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByIdQuery { Id = id }, ct);
		var cmd = new UpdateCompanyCommand
		{
			Id = company.Id,
			Title = company.Title,
			Slug = company.Slug
		};
		return View(cmd);
	}
	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost("[action]/{id}")]
	public async Task<IActionResult> Update([FromForm] UpdateCompanyCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		var company = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = company.Id });
	}

	[Authorize(Policy = "OwnerOrAdmin")]
	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteCompanyCommand { Id = id }, ct);
		
		if (User.IsInRole("Owner"))
		{
			// Owner sildiğinde, yeni şirket oluşturmaya yönlendir (veya MyCompany action'ına)
			return RedirectToAction(nameof(Create));
		}
		
		return RedirectToAction(nameof(AllCompanies));
	}
	[HttpGet("[action]")]
	public IActionResult DivideByZeroError()
	{
		int zero = 0;
		int result = 1 / zero;
		return View();
	}

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Search(string? term, int page = 0, int pageSize = 10, CancellationToken ct = default)
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
    [HttpGet("[action]")]
    public async Task<IActionResult> CheckSlug(string slug, Guid? excludeId, CancellationToken ct)
    {
        var result = await mediator.Send(new CheckCompanySlugQuery { Slug = slug, ExcludeId = excludeId }, ct);
        return Json(result);
    }
}
