using Application.Abstractions;
using Application.Common.Base.Page.RequestBase;
using Application.Common.Base.DTOs;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetAllMenusPagedQuery : PageRequest, IRequest<PaginatedListDTO<MenuDTO>>
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? StoreId { get; set; }
    public bool OnlyStoreMenus { get; set; }
    public bool OnlyCompanyMenus { get; set; }
}

public class GetAllMenusPagedHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetAllMenusPagedQuery, PaginatedListDTO<MenuDTO>>
{
	public async Task<PaginatedListDTO<MenuDTO>> Handle(GetAllMenusPagedQuery req, CancellationToken ct)
	{
		var menu = await repoMenu.GetPageListAsync(
			request: req,
			expression: m =>
				(string.IsNullOrEmpty(req.Search) || m.Title.Contains(req.Search)) &&
				(!req.CompanyId.HasValue || m.CompanyId == req.CompanyId.Value || (m.StoreId != null && m.Store!.CompanyId == req.CompanyId.Value)) &&
				(!req.StoreId.HasValue || m.StoreId == req.StoreId.Value) &&
				(!req.OnlyStoreMenus || m.StoreId != null) &&
				(!req.OnlyCompanyMenus || m.CompanyId != null),
			include: m => m.Include(m => m.Categories).Include(m => m.Medias)
				.Include(m => m.Store).Include(m => m.Company),
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
