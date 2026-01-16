using Application.Abstractions;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public class GetMenusPagedByCompanyQuery : PageRequest, IRequest<MenuDTO>
{
	public Guid CompanyId { get; set; }
}

public class GetMenusPagedByCompanyHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetMenusPagedByCompanyQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetMenusPagedByCompanyQuery req, CancellationToken ct)
	{
		var menu = await repoMenu.GetPageListAsync(
			request: req,
			expression: m => m.Id == req.CompanyId,
			include: m => m.Include(m => m.Categories).Include(m => m.Medias),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			ct: ct
		);

		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}