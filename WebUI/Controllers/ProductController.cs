using Application.Products.Commands;
using Application.Products.DTOs;
using Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("[controller]")]
public class ProductController(IMediator mediator) : Controller
{
	[HttpGet]
	public async Task<IActionResult> Index([FromQuery] GetAllProductsPagedQuery req, CancellationToken ct)
	{
		var products = await mediator.Send(req, ct);
		return View(products);
	}

	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Details(Guid id, CancellationToken ct)
	{
		ProductDTO? product = await mediator.Send(new GetProductByIdQuery(id), ct);
		return View(product);
	}

	[HttpGet("[action]/{categoryId}")]
	public async Task<IActionResult> ByCategory(GetProductsPagedByCategoryIdQuery request, CancellationToken ct)
	{
		var products = await mediator.Send(request, ct);
		return View(products);
	}

	[Authorize(Policy = "Owner")]
	[HttpGet("[action]")]
	public IActionResult Create()
	{
		return View();
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CreateProductCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		var product = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = product.Id });
	}

	[Authorize(Policy = "Owner")]
	[HttpGet("[action]/{id}")]
	public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
	{
		var product = await mediator.Send(new GetProductByIdQuery(id), ct);
		return View(product);
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Update(UpdateProductCommand req, CancellationToken ct)
	{
		if(!ModelState.IsValid)
			return View(req);

		var product = await mediator.Send(req, ct);
		return RedirectToAction(nameof(Details), new { id = product.Id });
	}

	[Authorize(Policy = "Owner")]
	[HttpPost("[action]/{id}")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
	{
		await mediator.Send(new DeleteProductCommand(id), ct);
		return RedirectToAction(nameof(Index));
	}
}
