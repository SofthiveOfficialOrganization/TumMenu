using Application.Abstractions;
using Application.Common.Base.Page;
using Application.Categories.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public class GetAllCategoryLibraryItemsPagedQuery : PageRequest, IRequest<IPaginate<CategoryLibraryItemDTO>>
{
	public string? Search { get; set; }
}

public class GetAllCategoriesPagedHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibrary,
	IMapper mapper
) : IRequestHandler<GetAllCategoryLibraryItemsPagedQuery, IPaginate<CategoryLibraryItemDTO>>
{
	public async Task<IPaginate<CategoryLibraryItemDTO>> Handle(GetAllCategoryLibraryItemsPagedQuery req, CancellationToken ct)
	{
		var categoryLibItemList = await repoCategoryLibrary.GetPageListAsync(
			req,
			c =>
				string.IsNullOrEmpty(req.Search) ||
				c.Title.Contains(req.Search) ||
				c.Slug.Contains(req.Search),
			orderBy: c => c.OrderBy(c => c.Title),
			ct: ct
		);
		var categoryLibItemListDTO = mapper.Map<IPaginate<CategoryLibraryItemDTO>>(categoryLibItemList);
		return categoryLibItemListDTO;
	}
}
