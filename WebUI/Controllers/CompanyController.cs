using Application.Companies.Commands;
using Application.Companies.DTOs;
using Application.Companies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class CompanyController(IMediator mediator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        CompanyDTO? company = await mediator.Send(new GetCompanyByIdQuery(id), ct);
        return View(company);
    }
    [Authorize(Policy = "Owner")]
    [HttpGet("[action]")]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Create(CreateCompanyCommand req, CancellationToken ct)
    {
        var company = await mediator.Send(req, ct);
        return RedirectToAction(nameof(Details), new { id = company.Id });
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> AllCompanies(GetAllCompaniesPagedQuery req, CancellationToken ct)
    {
        var compaines = await mediator.Send(req, ct);
        return View(compaines);
    }
    [HttpGet("[action]")]
    public async Task<IActionResult> MyCompanies(CancellationToken ct)
    {
        var companies = await mediator.Send(new GetCompaniesPagedByCurrentOwnerQuery(), ct);
        return View(companies);
    }
    [HttpPost("[action]")]
    public async Task<IActionResult> Update(UpdateCompanyCommand req, CancellationToken ct)
    {
        var company = await mediator.Send(req, ct);
        return RedirectToAction(nameof(Details), new { id = company.Id });
    }
    [HttpPost("[action]/{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteCompanyCommand(id), ct);
        return RedirectToAction(nameof(MyCompanies));
    }
}
