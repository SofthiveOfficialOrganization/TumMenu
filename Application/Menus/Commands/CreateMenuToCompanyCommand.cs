using Application.Abstractions;
using Application.Common.Exceptions;
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
	IRepository<Company> repoCompany,
	IMapper mapper
) : IRequestHandler<CreateMenuToCompanyCommand, MenuDTO>
{
	public async Task<MenuDTO> Handle(CreateMenuToCompanyCommand req, CancellationToken ct)
	{
		var company = await repoCompany.Query(tracked: true)
			.FirstOrDefaultAsync(x => x.Id == req.CompanyId, ct);

		if (company is null)
			throw new NotFoundAppException("Şirket bulunamadı.");

		var menu = new Menu
		{
			Title = req.Title,
			CompanyId = req.CompanyId,
			Status = MenuStatus.MainMenu
		};

		await repoMenu.AddAsync(menu, ct);

		if (!company.DefaultMainMenuId.HasValue)
		{
			company.DefaultMainMenu = menu;
		}

		var menuDTO = mapper.Map<MenuDTO>(menu);
		menuDTO.IsDefaultCompanyMenu = company.DefaultMainMenuId == menu.Id || company.DefaultMainMenu == menu;
		return menuDTO;
	}
}
