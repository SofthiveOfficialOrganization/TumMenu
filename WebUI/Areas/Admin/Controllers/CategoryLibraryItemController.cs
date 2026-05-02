using Application.Categories.Commands;
using Application.Categories.Queries;
using Application.CategorySuggestions.Commands;
using Application.Medias.Commands;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoryLibraryItemController(
	IMediator mediator, 
	IMapper mapper
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
	public IActionResult Create(string? suggestedTitle, Guid? suggestionId)
	{
		var cmd = new CreateCategoryLibraryItemCommand();
		if (!string.IsNullOrWhiteSpace(suggestedTitle))
			cmd.Title = suggestedTitle;
		ViewBag.SuggestionId = suggestionId;

		return View(cmd);
	}

	[Authorize(Roles = "Admin")]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create([FromForm] CreateCategoryLibraryItemCommand cmd, Guid? suggestionId, List<IFormFile>? photoFiles, CancellationToken ct)
	{
		if(!ModelState.IsValid)
		{
			ViewBag.SuggestionId = suggestionId;
			return View(cmd);
		}

		var category = await mediator.Send(cmd, ct);
		await UploadCategoryLibraryPhotosAsync(category.Id, photoFiles, category.Title, ct);

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

	private async Task UploadCategoryLibraryPhotosAsync(Guid categoryLibraryItemId, IEnumerable<IFormFile>? photoFiles, string? altText, CancellationToken ct)
	{
		if (photoFiles == null)
			return;

		var sortOrder = 0;
		foreach (var file in photoFiles.Where(f => f is { Length: > 0 }))
		{
			await mediator.Send(new UploadMediaCommand
			{
				File = file,
				ReferenceId = categoryLibraryItemId,
				Type = MediaRefType.CategoryLibraryItem,
				Slot = "default-gallery",
				AltText = altText,
				SortOrder = sortOrder++
			}, ct);
		}
	}
}


