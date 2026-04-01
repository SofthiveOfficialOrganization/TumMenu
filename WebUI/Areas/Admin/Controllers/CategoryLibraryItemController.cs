using Application.Categories.Commands;
using Application.Categories.Queries;
using Application.CategorySuggestions.Commands;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryLibraryItemController(
	IMediator mediator, 
	IMapper mapper,
	Application.Abstractions.IRepository<Domain.Entities.CategoryLibraryItem> repoCategoryLibrary
) : Controller
{
	[Authorize(Roles = "Admin,Owner")]
	[HttpGet]
	public async Task<IActionResult> Index(GetAllCategoryLibraryItemsPagedQuery req, CancellationToken ct)
	{
		var categories = await mediator.Send(req, ct);
		return View(categories);
	}

	[Authorize(Roles = "Admin")]
	[HttpGet]
	public async Task<IActionResult> Create(string? suggestedTitle, Guid? suggestionId, Guid? parentId)
	{
		var cmd = new CreateCategoryLibraryItemCommand();
		if (!string.IsNullOrWhiteSpace(suggestedTitle))
			cmd.Title = suggestedTitle;
		ViewBag.SuggestionId = suggestionId;
        
		if (parentId.HasValue)
		{
			cmd.ParentId = parentId;
			var parentCat = await repoCategoryLibrary.GetByIdAsync(parentId.Value);
			if (parentCat != null)
			{
				ViewBag.ParentName = parentCat.Title;
			}
		}

		return View(cmd);
	}

	[Authorize(Roles = "Admin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateCategoryLibraryItemCommand cmd, Guid? suggestionId, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(cmd);

		await mediator.Send(cmd, ct);

		// Öneri var ise onayla
		if (suggestionId.HasValue)
			await mediator.Send(new ApproveCategorySuggestionCommand { Id = suggestionId.Value }, ct);

		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
	}

	[Authorize(Roles = "Admin,Owner")]
	[HttpGet]
	public async Task<IActionResult> Details(string slug, string? returnUrl, CancellationToken ct)
	{
		var categories = await mediator.Send(new GetCategoryLibraryItemBySlugQuery { Slug = slug }, ct);
		ViewData["ReturnUrl"] = returnUrl;
		return View(categories);
	}

	[Authorize(Roles = "Admin")]
	[HttpGet]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		var dto = await mediator.Send(new GetCategoryLibraryItemByIdQuery { CategoryId = id }, ct);
		if(dto is null) return NotFound();

		if (dto.ParentId.HasValue)
		{
			var parentCat = await repoCategoryLibrary.GetByIdAsync(dto.ParentId.Value, ct);
			if(parentCat != null)
			{
				ViewBag.ParentName = parentCat.Title;
			}
		}

		var cmd = mapper.Map<UpdateCategoryLibraryItemCommand>(dto);
		return View(cmd);
	}

	[Authorize(Roles = "Admin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(UpdateCategoryLibraryItemCommand cmd, CancellationToken ct)
	{
		if(!ModelState.IsValid)
		{
			return View(cmd);
		}

		await mediator.Send(cmd, ct);
		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
	}


	[Authorize(Roles = "Admin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteCategoryLibraryItemCommand { Id = id }, ct);
		return RedirectToAction(nameof(Index), new { role = RouteData.Values["role"] });
	}
}




