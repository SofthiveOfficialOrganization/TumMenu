using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Common.Helpers;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
		var menu = await repoMenu.Query()
			.Include(x => x.Categories)
			.ThenInclude(x => x.CategoryLibraryItem)
			.FirstOrDefaultAsync(x => x.Id == req.Id, ct);
			
		_ = menu ?? throw new NotFoundAppException("Menü bulunamadı.");
		
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
