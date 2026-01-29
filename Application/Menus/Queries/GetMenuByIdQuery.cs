using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Queries;

public class GetMenuByIdQuery : IRequest<MenuDTO>
{
	public Guid Id { get; set; }
}

public class GetMenuByIdHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<GetMenuByIdQuery, MenuDTO>
{
	public async Task<MenuDTO> Handle(GetMenuByIdQuery req, CancellationToken ct)
	{
		var menu = (await repoMenu.GetByIdAsync(req.Id, ct)).EnsureFound("Menü bulunamadı.");
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
