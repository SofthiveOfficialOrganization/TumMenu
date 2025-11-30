using Application.Companies.Commands;
using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Owner")] // varsa
[Route("admin/[controller]/[action]")]
public sealed class CompanyController : Controller
{
	private readonly IMediator _mediator;

	public CompanyController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet]
	public async Task<IActionResult> Create(CancellationToken ct)
	{
		// view model doldur vs.
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateCompanyCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		var dto = await _mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Details), new { id = dto.Id });
	}

	[HttpGet("{id:guid}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		var company = await _mediator.Send(new GetCompanyByIdQuery(id), ct);
		return View(company);
	}
}
