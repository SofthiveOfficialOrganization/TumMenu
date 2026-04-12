using Application.Abstractions;
using Application.Common.Base.DTOs;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetMenusPagedByCompanyQuery : PageRequest, IRequest<PaginatedListDTO<MenuDTO>>
{
	public Guid CompanyId { get; set; }
	public string? Search { get; set; }
}

public class GetMenusPagedByCompanyHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetMenusPagedByCompanyQuery, PaginatedListDTO<MenuDTO>>
{
	public async Task<PaginatedListDTO<MenuDTO>> Handle(GetMenusPagedByCompanyQuery req, CancellationToken ct)
	{
		var menus = await repoMenu.GetPageListAsync(
			request: req,
			expression: m =>
				m.CompanyId == req.CompanyId &&
				(string.IsNullOrEmpty(req.Search) || m.Title.Contains(req.Search)),
			include: m => m.Include(x => x.Categories).Include(x => x.Company),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			splitQuery: true,
			ct: ct
		);

		var menuDTOs = mapper.Map<PaginatedListDTO<MenuDTO>>(menus);
		return menuDTOs;
	}
}
