using Application.Abstractions;
using Application.Categories.Commands;
using Application.Categories.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDTO?>;

public class GetCategoryByIdHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, CategoryDTO?>
{
	public async Task<CategoryDTO?> Handle(GetCategoryByIdQuery request, CancellationToken ct)
	{
		var category = await repoCategory.GetByIdAsync(request.CategoryId, ct);
		return mapper.Map<CategoryDTO?>(category);
	}
}
