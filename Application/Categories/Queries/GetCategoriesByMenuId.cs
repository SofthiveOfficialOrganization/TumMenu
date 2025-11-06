using Application.Abstractions;
using Application.Categories.Commands;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public record GetCategoriesByMenuIdQuery(Guid MenuId) : IRequest<List<CategoryDto>>;

public class GetCategoriesByMenuIdHandler(IRepository<Category> repo, IMapper mapper)
	: IRequestHandler<GetCategoriesByMenuIdQuery, List<CategoryDto>>
{
	public async Task<List<CategoryDto>> Handle(GetCategoriesByMenuIdQuery req, CancellationToken ct)
	{
		var list = await repo.Query()
			.Where(c => c.MenuId == req.MenuId)
			.OrderBy(c => c.SortOrder)
			.ToListAsync(ct);

		return mapper.Map<List<CategoryDto>>(list);
	}
}
