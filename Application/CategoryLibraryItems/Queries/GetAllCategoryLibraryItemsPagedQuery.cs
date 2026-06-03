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
	public List<Guid>? ExcludeIds { get; set; }
}

public class GetAllCategoriesPagedHandler(
	IRepository<CategoryLibraryItem> repoCategoryLibrary,
	IMapper mapper
) : IRequestHandler<GetAllCategoryLibraryItemsPagedQuery, IPaginate<CategoryLibraryItemDTO>>
{
	public async Task<IPaginate<CategoryLibraryItemDTO>> Handle(GetAllCategoryLibraryItemsPagedQuery req, CancellationToken ct)
	{
		var search = NormalizeSearch(req.Search);

		var categoryLibItemList = await repoCategoryLibrary.GetPageListAsync(
			req,
			c =>
				(req.ExcludeIds == null || !req.ExcludeIds.Contains(c.Id)) &&
				(string.IsNullOrEmpty(search) ||
				c.Title.Trim().ToLower()
					.Replace("ı", "i")
					.Replace("ğ", "g")
					.Replace("ü", "u")
					.Replace("ş", "s")
					.Replace("ö", "o")
					.Replace("ç", "c")
					.Contains(search) ||
				c.Slug.Trim().ToLower()
					.Replace("ı", "i")
					.Replace("ğ", "g")
					.Replace("ü", "u")
					.Replace("ş", "s")
					.Replace("ö", "o")
					.Replace("ç", "c")
					.Contains(search) ||
				(c.Description != null && c.Description.Trim().ToLower()
					.Replace("ı", "i")
					.Replace("ğ", "g")
					.Replace("ü", "u")
					.Replace("ş", "s")
					.Replace("ö", "o")
					.Replace("ç", "c")
					.Contains(search)) ||
				(c.IconKey != null && c.IconKey.Trim().ToLower()
					.Replace("ı", "i")
					.Replace("ğ", "g")
					.Replace("ü", "u")
					.Replace("ş", "s")
					.Replace("ö", "o")
					.Replace("ç", "c")
					.Contains(search))),
			orderBy: c => c.OrderBy(c => c.Title),
			ct: ct
		);
		var categoryLibItemListDTO = mapper.Map<IPaginate<CategoryLibraryItemDTO>>(categoryLibItemList);
		return categoryLibItemListDTO;
	}

	private static string? NormalizeSearch(string? value) =>
		value == null
			? null
			: value.Trim().ToLower()
				.Replace("ı", "i")
				.Replace("ğ", "g")
				.Replace("ü", "u")
				.Replace("ş", "s")
				.Replace("ö", "o")
				.Replace("ç", "c");
}
