using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class CreateMenuToStoreCommand : IRequest<MenuDTO>, ITransactionalRequest
{
	public string Title { get; set; } = string.Empty;
	public Guid StoreId { get; set; }
	public MenuStatus Status { get; set; } = MenuStatus.Inactive;
}

public class CreateMenuToStoreHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CreateMenuToStoreCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToStoreCommand req, CancellationToken ct)
	{
		// Logic:
		// 1. If req.Status is Active -> Deactivate all others for this Store
		// 2. If req.Status is Inactive -> Check if ANY menu exists. If count == 0 -> Force Active.

		bool anyMenuExists = await repoMenu.Query().AnyAsync(x => x.StoreId == req.StoreId, ct);

		if (req.Status == MenuStatus.Active)
		{
			// Deactivate others
			var activeMenus = await repoMenu.Query(tracked: true)
				.Where(x => x.StoreId == req.StoreId && x.Status == MenuStatus.Active)
				.ToListAsync(ct);
			
			foreach (var m in activeMenus) m.Status = MenuStatus.Inactive;
		}
		else
		{
			// If no menu exists, this must be active
			if (!anyMenuExists)
			{
				req.Status = MenuStatus.Active;
			}
		}

		var menu = mapper.Map<Menu>(req);
		await repoMenu.AddAsync(menu, ct);
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
