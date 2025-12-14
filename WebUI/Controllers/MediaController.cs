using Application.Medias.Commands;
using Application.Medias.DTOs;
using Application.Medias.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class MediaController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery] GetAllMediasPagedQuery req, CancellationToken ct)
	{
		var medias = await mediator.Send(req, ct);
		return View(medias);
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		MediaDTO? media = await mediator.Send(new GetMediaByIdQuery(id), ct);
		return View(media);
	}

	[Authorize(Policy = "Owner")]
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View();
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]")]
	public async Task<IActionResult> Create(CreateMediaCommand req, CancellationToken ct)
	{
		var media = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = media.Id });
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]")]
	public async Task<IActionResult> Update(UpdateMediaCommand req, CancellationToken ct)
	{
		var media = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = media.Id });
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]/{id}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteMediaCommand(id), ct);
		return RedirectToAction(nameof(Index));
	}
}
