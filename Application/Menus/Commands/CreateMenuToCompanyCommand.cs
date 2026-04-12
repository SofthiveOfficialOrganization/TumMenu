using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class CreateMenuToCompanyCommand : IRequest<MenuDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Ana menü oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public Guid CompanyId { get; set; }
}

public class CreateMenuToCompanyCommandHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<CreateMenuToCompanyCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToCompanyCommand req, CancellationToken ct)
	{
		// Company menus use MainMenu status — archive any existing ones
		var existingMainMenus = await repoMenu.Query(tracked: true)
			.Where(x => x.CompanyId == req.CompanyId && x.Status == MenuStatus.MainMenu)
			.ToListAsync(ct);

		foreach (var m in existingMainMenus) m.Status = MenuStatus.Archived;

		var menu = new Menu
		{
			Title = req.Title,
			CompanyId = req.CompanyId,
			Status = MenuStatus.MainMenu
		};

		await repoMenu.AddAsync(menu, ct);
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
