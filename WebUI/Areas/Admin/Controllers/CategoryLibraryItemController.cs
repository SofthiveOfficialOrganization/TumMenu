using Application.Categories.Commands;
using Application.Categories.Queries;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/category-library")]
public class CategoryLibraryItemController(IMediator mediator, IMapper mapper) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index(GetAllCategoryLibraryItemsPagedQuery req, CancellationToken ct)
	{
		var categories = await mediator.Send(req, ct);
		return View(categories);
	}

	[Authorize(Roles = "Admin")]
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View(new CreateCategoryLibraryItemCommand());
	}

	[Authorize(Roles = "Admin")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateCategoryLibraryItemCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index));
	}

	[Authorize(Roles = "Admin,Owner")]
	[HttpGet("{slug}")]
	public async Task<IActionResult> Details(string slug, CancellationToken ct)
	{
		var categories = await mediator.Send(new GetCategoryLibraryItemBySlugQuery { Slug = slug }, ct);
		return View(categories);
	}

	[HttpGet("[action]/{id:guid}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		var dto = await mediator.Send(new GetCategoryLibraryItemByIdQuery { CategoryId = id }, ct);
		if(dto is null) return NotFound();

		var cmd = mapper.Map<UpdateCategoryLibraryItemCommand>(dto);
		return View(cmd);
	}

	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UpdateCategoryLibraryItemCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
		{
			return View(cmd);
		}

		await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index));
	}


	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteCategoryLibraryItemCommand { Id = id }, ct);
		return RedirectToAction(nameof(Index));
	}
}
