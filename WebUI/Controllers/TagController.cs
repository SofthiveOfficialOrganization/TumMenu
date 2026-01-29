using Application.Tags.Commands;
using Application.Tags.DTOs;
using Application.Tags.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class TagController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery] GetTagsPagedQuery req, CancellationToken ct)
	{
		var tags = await mediator.Send(req, ct);
		return View(tags);
	}

	[Authorize(Policy = "OwnerOnly")]
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View();
	}

	[Authorize(Policy = "OwnerOnly")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateTagCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		var dto = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index));
	}

	[Authorize(Policy = "OwnerOnly")]
	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		return View();
	}

	[Authorize(Policy = "OwnerOnly")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Update(UpdateTagCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		var dto = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Index));
	}

	[Authorize(Policy = "OwnerOnly")]
	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteTagCommand(id), ct);
		return RedirectToAction(nameof(Index));
	}
}
