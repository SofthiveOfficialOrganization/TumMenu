using Application.Abstractions;
using Application.Common.Base.Page.RequestBase;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Queries;

public record GetAllMenusPagedQuery() : PageRequest, IRequest<MenuDTO>;

public class GetAllMenusPagedHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetAllMenusPagedQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetAllMenusPagedQuery req, CancellationToken ct)
	{
		var menu = await repoMenu.GetPageListAsync(
			request: req,
			include: m => m.Include(m => m.Categories).Include(m => m.Medias),
			orderBy: m => m.OrderByDescending(m => m.CreatedAt),
			ct: ct
		);

		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}