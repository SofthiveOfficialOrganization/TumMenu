using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetMenusPagedByCurrentOwnerQuery : PageRequest, IRequest<PaginatedListDTO<MenuDTO>> 
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public bool OnlyStoreMenus { get; set; }
    public bool OnlyCompanyMenus { get; set; }
}

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
				((m.CompanyId != null && m.Company!.Owner!.ApplicationUserId == applicationUserId) ||
				(m.StoreId != null && m.Store!.Company.Owner!.ApplicationUserId == applicationUserId)) &&
                (string.IsNullOrEmpty(req.Search) || m.Title.Contains(req.Search)) &&
                (!req.CompanyId.HasValue || m.CompanyId == req.CompanyId.Value) &&
                (!req.StoreId.HasValue || m.StoreId == req.StoreId.Value) &&
                (!req.OnlyStoreMenus || m.StoreId != null) &&
                (!req.OnlyCompanyMenus || m.CompanyId != null),
			include: m => m.Include(x => x.Categories)
				.Include(x => x.Store).ThenInclude(x => x!.Company)
				.Include(x => x.Company),
			orderBy: m => m
				.OrderByDescending(x => x.Company != null && x.Company.DefaultMainMenuId == x.Id)
				.ThenByDescending(x => x.CreatedAt),
			splitQuery: true,
			ct: ct
		);

		var menuDTO = mapper.Map<PaginatedListDTO<MenuDTO>>(menu);
		return menuDTO;
	}
}
