using Application.Companies.Commands;
using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class CompanyController(IMediator mediator) : Controller
{
	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		var company = await mediator.Send(new GetCompanyByIdQuery(id), ct);
		return View(company);
	}
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View();
	}
	[HttpPost("[action]")]
	public async Task<IActionResult> Create(CreateCompanyCommand request, CancellationToken ct)
	{
		var company = await mediator.Send(request, ct);
		return RedirectToAction(nameof(Details), new { id = company.Id });
	}
	[HttpGet("[action]")]
	public async Task<IActionResult> AllCompanies(CancellationToken ct)
	{
		var compaines = await mediator.Send(new GetAllCompaniesPagedQuery(), ct);
		return View(compaines);
	}
	[HttpGet("[action]")]
	public async Task<IActionResult> MyCompanies(CancellationToken ct)
	{
		var companies = await mediator.Send(new GetCompaniesPagedByCurrentOwnerQuery(), ct);
		return View(companies);
	}
	[HttpPost("[action]")]
	public async Task<IActionResult> Update(UpdateCompanyCommand request, CancellationToken ct)
	{
		var company = await mediator.Send(request, ct);
		return RedirectToAction(nameof(Details), new { id = company.Id });
	}
	[HttpPost("[action]/{id}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteCompanyCommand(id), ct);
		return RedirectToAction(nameof(MyCompanies));
	}
}
