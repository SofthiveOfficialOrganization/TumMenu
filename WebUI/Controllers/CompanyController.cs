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
	[HttpGet("[action]")]
	public async Task<IActionResult> ExampleCompanies()
	{
		List<CompanyDTO> company = [];
		for(int i = 1; i <= 5; i++)
		{
			CompanyDTO comp = new CompanyDTO()
			{
				Id = Guid.NewGuid(),
				Name = $"Example Company {i}",
				Slug = $"example-company-{i}",
				OwnerId = Guid.NewGuid()
			};

			company.Add(comp);
		}
		return View(company);
	}


	[HttpPost("[action]")]
	public IActionResult CreateTestCompany([FromBody] CompanyDTO dto)
	{
		if(!ModelState.IsValid)
			return BadRequest(ModelState);

		if(string.IsNullOrWhiteSpace(dto.Name))
			return BadRequest("Name is required.");

		var created = new CompanyDTO
		{
			Id = Guid.NewGuid()
		};

		return Ok(created);
	}

}
