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
			expression: m => string.IsNullOrEmpty(req.Search) || m.Title.Contains(req.Search),
			include: m => m.Include(m => m.Categories).Include(m => m.Medias)
				.Include(m => m.Store).Include(m => m.Company),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			splitQuery: true,
			ct: ct
		);

		var menuDTO = mapper.Map<PaginatedListDTO<MenuDTO>>(menu);

		return menuDTO;
	}
}