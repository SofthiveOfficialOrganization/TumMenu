using Application.Categories;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TumMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController(IMediator mediator) : ControllerBase
{
	[HttpPost("[action]")]
	public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryCommand cmd, CancellationToken ct)
		=> Created(string.Empty, await mediator.Send(cmd, ct));

	[HttpGet("menu/{menuId:guid}")]
	public async Task<ActionResult<List<CategoryDto>>> GetByMenu(Guid menuId, CancellationToken ct)
		=> Ok(await mediator.Send(new GetCategoriesByMenuIdQuery(menuId), ct));

	[HttpGet("api/categories/{id:guid}")]
	public async Task<ActionResult<CategoryDto>> GetOne(Guid id, CancellationToken ct)
	=> Ok(await mediator.Send(new GetCategoryByIdQuery(id), ct));
}
