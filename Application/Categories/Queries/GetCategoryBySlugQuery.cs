using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryDTO?>;

public class GetCategoryBySlugHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoryBySlugQuery, CategoryDTO?>
{
	public async Task<CategoryDTO?> Handle(GetCategoryBySlugQuery req, CancellationToken ct)
	{
		var category = (await repoCategory.Query()
			.Where(c => c.Slug == req.Slug)
			.FirstOrDefaultAsync(ct))
			.EnsureFound("Kategori bulunamadý.");

		return mapper.Map<CategoryDTO?>(category);
	}
}
