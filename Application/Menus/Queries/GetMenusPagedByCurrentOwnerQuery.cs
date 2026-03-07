using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetMenusPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<MenuDTO>> { }

public class GetMenusPagedByCurrentOwnerHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper,
	IUserContext userContext
) : IRequestHandler<GetMenusPagedByCurrentOwnerQuery, PaginatedListDTO<MenuDTO>>
{
	public async Task<PaginatedListDTO<MenuDTO>> Handle(GetMenusPagedByCurrentOwnerQuery req, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;
		var menu = await repoMenu.GetPageListAsync(
			request: req,
			expression: m =>
				(m.CompanyId != null && m.Company!.Owner!.ApplicationUserId == applicationUserId) ||
				(m.StoreId != null && m.Store!.Company.Owner!.ApplicationUserId == applicationUserId),
			include: m => m.Include(x => x.Categories)
				.Include(x => x.Store).ThenInclude(x => x!.Company)
				.Include(x => x.Company),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			ct: ct
		);

		var menuDTO = mapper.Map<PaginatedListDTO<MenuDTO>>(menu);
		return menuDTO;
	}
}