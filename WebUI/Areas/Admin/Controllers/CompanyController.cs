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
        if (!ModelState.IsValid)
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
    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateCompanyCommand req, CancellationToken ct)
    {
        var company = await mediator.Send(req, ct);
        return RedirectToAction(nameof(Details), new { id = company.Id });
    }

    [Authorize(Policy = "OwnerOrAdmin")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCompanyCommand { Id = id }, ct);
        return RedirectToAction(nameof(MyCompanies));
    }
    [HttpGet("[action]")]
    public IActionResult DivideByZeroError()
    {
        int zero = 0;
        int result = 1 / zero;
        return View();
    }
}
