using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Commands;

public record CreateMenuToStoreCommand(
	string Name,
	Guid StoreId
) : IRequest<MenuDTO>, ITransactionalRequest;

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
