using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries;

public class GetCategoryLibraryItemBySlugQuery : IRequest<CategoryLibraryItemDTO?>
{
	public string Slug { get; set; } = string.Empty;
}

public class GetCategoryBySlugHandler(
	IRepository<CategoryLibraryItem> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoryLibraryItemBySlugQuery, CategoryLibraryItemDTO?>
{
	public async Task<CategoryLibraryItemDTO?> Handle(GetCategoryLibraryItemBySlugQuery req, CancellationToken ct)
	{
		var categoryLibItem = (await repoCategory.Query()
			.Where(c => c.Slug == req.Slug)
			.FirstOrDefaultAsync(ct)).EnsureFound("Kategori bulunamadı.");

		return mapper.Map<CategoryLibraryItemDTO?>(categoryLibItem);
	}
}
