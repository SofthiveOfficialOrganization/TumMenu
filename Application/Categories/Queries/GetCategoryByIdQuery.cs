using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryDTO?>
{
	public Guid CategoryId { get; set; }
}

public class GetCategoryByIdHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoryByIdQuery, CategoryDTO?>
{
	public async Task<CategoryDTO?> Handle(GetCategoryByIdQuery req, CancellationToken ct)
	{
		var category = (await repoCategory.GetByIdAsync(req.CategoryId, ct)).EnsureFound("Kategori bulunamadı.");

		return mapper.Map<CategoryDTO?>(category);
	}
}
