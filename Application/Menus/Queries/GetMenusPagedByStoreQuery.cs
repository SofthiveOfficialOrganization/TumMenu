using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public record GetMenusPagedByStoreQuery(
	Guid StoreId
) : PageRequest, IRequest<PaginatedListDTO<MenuDTO>>;

public class GetMenusPagedByStoreHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetMenusPagedByStoreQuery, PaginatedListDTO<MenuDTO>>
{
	public async Task<PaginatedListDTO<MenuDTO>> Handle(GetMenusPagedByStoreQuery req, CancellationToken ct)
	{
		var menus = await repoMenu.GetPageListAsync(
			request: req,
			expression: m => m.StoreId == req.StoreId,
			include: m => m.Include(m => m.Categories).Include(m => m.Images),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			ct: ct
		);

		var menuDTOs = mapper.Map<PaginatedListDTO<MenuDTO>>(menus);
		return menuDTOs;
	}
}