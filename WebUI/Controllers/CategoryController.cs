using Application.Categories.Commands;
using Application.Categories.Queries;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace WebUI.Controllers;

[Route("[controller]")]
public class CategoryController(IMediator mediator, IMapper mapper) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index(GetAllCategoriesPagedQuery req, CancellationToken ct)
	{
		var categories = await mediator.Send(req, ct);
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
		return View(new CreateCategoryCommand(menuId, "", null, 0, null));
	}

	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateCategoryCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		var created = await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index), new { menuId = created.MenuId });
	}

	[HttpGet("[action]/{id:guid}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		var dto = await mediator.Send(new GetCategoryByIdQuery(id), ct);
		if(dto is null) return NotFound();

		var cmd = mapper.Map<UpdateCategoryCommand>(dto);
		ViewData["MenuId"] = dto.MenuId;
		return View(cmd);
	}

	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UpdateCategoryCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
		{
			var dto = await mediator.Send(new GetCategoryByIdQuery(cmd.Id), ct);
			ViewData["MenuId"] = dto?.MenuId;
			return View(cmd);
		}

		var updated = await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index), new { menuId = updated.MenuId });
	}


	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		var menuId = await mediator.Send(new DeleteCategoryCommand(id), ct);
		return RedirectToAction(nameof(Index), new { MenuId = menuId });
	}
}
