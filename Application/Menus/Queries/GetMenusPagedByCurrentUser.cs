using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetMenusPagedByCurrentUserQuery : PageRequest, IRequest<PaginatedListDTO<MenuDTO>>
{
}

public class GetMenusPagedByCurrentUserHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper,
	IUserContext userContext
) : IRequestHandler<GetMenusPagedByCurrentUserQuery, PaginatedListDTO<MenuDTO>>
{
	public async Task<PaginatedListDTO<MenuDTO>> Handle(GetMenusPagedByCurrentUserQuery req, CancellationToken ct)
	{
		var applicationUserId = userContext.UserId;
		var menu = await repoMenu.GetPageListAsync(
			request: req,
			expression: m => m.Company.Owner.ApplicationUserId == applicationUserId,
			include: m => m.Include(m => m.Categories).Include(m => m.Medias),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			ct: ct
		);

		var menuDTO = mapper.Map<PaginatedListDTO<MenuDTO>>(menu);
		return menuDTO;
	}
}