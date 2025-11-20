using Application.Abstractions;
using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public record GetCategoriesByMenuIdQuery(Guid MenuId) : IRequest<List<CategoryDTO>>;

public class GetCategoriesByMenuIdHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoriesByMenuIdQuery, List<CategoryDTO>>
{
	public async Task<List<CategoryDTO>> Handle(GetCategoriesByMenuIdQuery req, CancellationToken ct)
	{
		var list = await repoCategory.Query()
			.Where(c => c.MenuId == req.MenuId)
			.OrderBy(c => c.SortOrder)
			.ToListAsync(ct);
		var categories = mapper.Map<List<CategoryDTO>>(list);
		return categories;
	}
}
