using Application.Stores.Commands;
using Application.Stores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class StoreController(IMediator mediator) : Controller
{
	[HttpGet]
	public IActionResult Index()
	{
		return View();
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		var store = await mediator.Send(new GetStoreByIdQuery(id), ct);
		return View(store);
	}

	[Authorize(Policy = "Owner")]
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View();
	}

	[HttpPost("[action]")]
	public async Task<IActionResult> Create(CreateStoreCommand req, CancellationToken ct)
	{
		var store = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = store.Id });
	}

	[HttpPost("[action]")]
	public async Task<IActionResult> Update(UpdateStoreCommand req, CancellationToken ct)
	{
		await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = req.Id });
	}

	[HttpPost("[action]/{id}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteStoreCommand(id), ct);
		return RedirectToAction(nameof(Index));
	}

	[HttpGet("[action]/{companyId}")]
	public async Task<IActionResult> ByCompany(Guid companyId, CancellationToken ct)
	{
		var stores = await mediator.Send(new GetStoresPaginatedByCompanyIdQuery(companyId), ct);
		return View(stores);
	}
}
