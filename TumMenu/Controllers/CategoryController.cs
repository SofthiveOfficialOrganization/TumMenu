using Application.Categories;
using Application.Categories.Commands;
using Application.Categories.DTOs;
using Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TumMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(IMediator mediator) : ControllerBase
{
	[HttpPost("[action]")]
	public async Task<ActionResult<CategoryDTO>> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
		=> Created(string.Empty, await mediator.Send(cmd, ct));

	[HttpGet("menu/{menuId:guid}")]
	public async Task<ActionResult<List<CategoryDTO>>> GetByMenu(Guid menuId, CancellationToken ct)
		=> Ok(await mediator.Send(new GetCategoriesByMenuIdQuery(menuId), ct));

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<CategoryDTO>> GetCategory(Guid id, CancellationToken ct)
	=> Ok(await mediator.Send(new GetCategoryByIdQuery(id), ct));
}
