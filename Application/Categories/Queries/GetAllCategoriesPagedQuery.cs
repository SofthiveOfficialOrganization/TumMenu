using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public record GetAllCategoriesPagedQuery() : PageRequest, IRequest<List<CategoryDTO>>;

public class GetAllCategoriesPagedHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetAllCategoriesPagedQuery, List<CategoryDTO>>
{
	public async Task<List<CategoryDTO>> Handle(GetAllCategoriesPagedQuery req, CancellationToken ct)
	{
		var categoryList = await repoCategory.GetPageListAsync(
			req,
			orderBy: c => c.OrderBy(c => c.SortOrder),
			ct: ct
		);
		var categoryListDTO = mapper.Map<List<CategoryDTO>>(categoryList);
		return categoryListDTO;
	}
}
