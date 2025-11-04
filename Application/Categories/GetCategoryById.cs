using Application.Abstractions.Repositories;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Categories;


public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto?>;

public class GetCategoryByIdHandler(ICategoryRepository repo, IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
	public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken ct)
	{
		var category = await repo.GetByIdAsync(request.CategoryId, ct);
		return mapper.Map<CategoryDto?>(category);
	}
}
