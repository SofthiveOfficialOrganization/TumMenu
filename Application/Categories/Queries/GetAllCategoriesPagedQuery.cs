using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public class GetAllCategoriesPagedQuery : PageRequest, IRequest<List<CategoryDTO>>
{
	public string? Search { get; set; }
}

public class GetAllCategoriesPagedHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetAllCategoriesPagedQuery, List<CategoryDTO>>
{
	public async Task<List<CategoryDTO>> Handle(GetAllCategoriesPagedQuery req, CancellationToken ct)
	{
		var categoryList = await repoCategory.GetPageListAsync(
			req,
			c =>
				string.IsNullOrEmpty(req.Search) ||
				c.Title.Contains(req.Search) ||
				c.Slug.Contains(req.Search),
			orderBy: c => c.OrderBy(c => c.SortOrder).ThenBy(c => c.Title),
			ct: ct
		);
		var categoryListDTO = mapper.Map<List<CategoryDTO>>(categoryList);
		return categoryListDTO;
	}
}
