using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class CreateMenuToCompanyCommand : IRequest<MenuDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Menü oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public Guid CompanyId { get; set; }
	public MenuStatus Status { get; set; } = MenuStatus.Inactive;
}

public class CreateMenuToCompanyCommandHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CreateMenuToCompanyCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToCompanyCommand req, CancellationToken ct)
	{
		bool anyMenuExists = await repoMenu.Query().AnyAsync(x => x.CompanyId == req.CompanyId, ct);

		if (req.Status == MenuStatus.Active)
		{
			var activeMenus = await repoMenu.Query(tracked: true)
				.Where(x => x.CompanyId == req.CompanyId && x.Status == MenuStatus.Active)
				.ToListAsync(ct);
			
			foreach (var m in activeMenus) m.Status = MenuStatus.Inactive;
		}
		else
		{
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