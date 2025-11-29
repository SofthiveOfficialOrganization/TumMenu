using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Queries;

public record GetMenuByIdQuery(
	Guid MenuId
) : IRequest<MenuDTO>;

public class GetMenuByIdHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetMenuByIdQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetMenuByIdQuery req, CancellationToken ct)
	{
		var menu = await repoMenu.GetByIdAsync(req.MenuId, ct);
		if(menu == null)
			throw new NotFoundAppException("Menü bulunamadı.");
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
