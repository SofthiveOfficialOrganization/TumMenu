using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Commands;

public class CreateMenuToStoreCommand : IRequest<MenuDTO>, ITransactionalRequest
{
	public string Title { get; set; } = string.Empty;
	public Guid StoreId { get; set; }
}

public class CreateMenuToStoreHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CreateMenuToStoreCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToStoreCommand req, CancellationToken ct)
	{
		var menu = mapper.Map<Menu>(req);
		await repoMenu.AddAsync(menu, ct);
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
