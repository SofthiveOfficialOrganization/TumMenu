using Application.Abstractions;
using Application.Categories.Commands;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;
public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto?>;

public class GetCategoryByIdHandler(IRepository<Category> repo, IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
	public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken ct)
	{
		var category = await repo.GetByIdAsync(request.CategoryId, ct);
		return mapper.Map<CategoryDto?>(category);
	}
}
