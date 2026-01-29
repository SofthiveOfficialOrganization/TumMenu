using Application.Abstractions;
using Application.Categories.DTOs;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Categories.Queries;

public class GetCategoriesPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<CategoryDTO>>
{
}

public class GetCategoriesByCurrentOwnerHandler(
	IRepository<Category> repoCategory,
	IMapper mapper,
	IUserContext userContext
) : IRequestHandler<GetCategoriesPagedByCurrentOwnerQuery, PaginatedListDTO<CategoryDTO>>
{
	public async Task<PaginatedListDTO<CategoryDTO>> Handle(GetCategoriesPagedByCurrentOwnerQuery req, CancellationToken ct)
	{
		var userId = userContext.UserId;
		var categoryList = await repoCategory.GetPageListAsync(
			req,
			c => c.Menu.Company.Owner.ApplicationUserId == userId,
			orderBy: c => c.OrderBy(c => c.SortOrder).ThenBy(c => c.Name),
			ct: ct
			);
		var categoryListDTO = mapper.Map<PaginatedListDTO<CategoryDTO>>(categoryList);
		return categoryListDTO;
	}
}
