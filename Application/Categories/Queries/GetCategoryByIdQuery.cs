using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDTO>;

public sealed class GetCategoryByIdQueryHandler(
	IRepository<Category> repo,
	IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, CategoryDTO>
{
	public async Task<CategoryDTO> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
	{
		var category = await repo.Query()
			.Include(c => c.Menu)
			.Include(c => c.CategoryLibraryItem)
			.Include(c => c.Products) // fetch products
				.ThenInclude(p => p.Prices) // prices can be useful later
			.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

		if (category == null)
			throw new NotFoundAppException("Kategori bulunamadı.");

		return mapper.Map<CategoryDTO>(category);
	}
}
