using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public class GetCategoryLibraryItemByIdQuery : IRequest<CategoryLibraryItemDTO?>
{
	public Guid CategoryId { get; set; }
}

public class GetCategoryByIdHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibraryItem,
	IMapper mapper
) : IRequestHandler<GetCategoryLibraryItemByIdQuery, CategoryLibraryItemDTO?>
{
	public async Task<CategoryLibraryItemDTO?> Handle(GetCategoryLibraryItemByIdQuery req, CancellationToken ct)
	{
		var categoryLibItem = (await repoCategoryLibraryItem.GetByIdAsync(req.CategoryId, ct)).EnsureFound("Kategori bulunamadı.");

		return mapper.Map<CategoryLibraryItemDTO?>(categoryLibItem);
	}
}
