using Application.Abstractions;
using Application.Abstractions.Repositories;
using MapsterMapper;
using MediatR;

namespace Application.Categories;

public record GetCategoriesByMenuIdQuery(Guid MenuId) : IRequest<List<CategoryDto>>;

public class GetCategoriesByMenuIdHandler(ICategoryRepository repo, IMapper mapper)
	: IRequestHandler<GetCategoriesByMenuIdQuery, List<CategoryDto>>
{
	public async Task<List<CategoryDto>> Handle(GetCategoriesByMenuIdQuery request, CancellationToken ct)
	{
		var list = await repo.GetByMenuAsync(request.MenuId, ct);
		return mapper.Map<List<CategoryDto>>(list);
	}
}
