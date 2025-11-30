using Application.Categories.Commands;
using Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class CategoryController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index(CancellationToken ct)
	{
		var categories = await mediator.Send(new GetAllCategoriesPagedQuery(), ct);
		return View(categories);
	}

	[HttpGet("{slug}")]
	public async Task<IActionResult> Details(string slug, CancellationToken ct)
	{
		var categories = await mediator.Send(new GetCategoryBySlugQuery(slug), ct);
		return View(categories);
	}

	[HttpGet("[action]/{menuId}")]
	public async Task<IActionResult> Categories(Guid menuId, CancellationToken ct)
	{
		var categories = await mediator.Send(new GetCategoriesPagedByMenuIdQuery(menuId), ct);
		return View(categories);
	}

	[HttpGet("[action]/{menuId}")]
	public IActionResult Create(Guid menuId)
	{
		return View(menuId);
	}

	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index), new { menuId = cmd.MenuId });
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		var dto = await mediator.Send(new GetCategoryByIdQuery(id), ct);
		return View(dto);
	}

	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteCategoryCommand(id), ct);
		return RedirectToAction(nameof(Index));
	}
}
