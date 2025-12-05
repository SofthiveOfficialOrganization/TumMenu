using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public record GetCategoriesPagedByMenuIdQuery(Guid MenuId) : PageRequest, IRequest<PaginatedListDTO<CategoryDTO>>;

public class GetCategoriesByMenuIdHandler(
	IRepository<Category> repoCategory,
	IMapper mapper
) : IRequestHandler<GetCategoriesPagedByMenuIdQuery, PaginatedListDTO<CategoryDTO>>
{
	public async Task<PaginatedListDTO<CategoryDTO>> Handle(GetCategoriesPagedByMenuIdQuery req, CancellationToken ct)
	{
		var categoryList = await repoCategory.GetPageListAsync(
			req,
			c => c.MenuId == req.MenuId,
			orderBy: c => c.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
			ct: ct
			);
		var categoryListDTO = mapper.Map<PaginatedListDTO<CategoryDTO>>(categoryList);
		return categoryListDTO;
	}
}
