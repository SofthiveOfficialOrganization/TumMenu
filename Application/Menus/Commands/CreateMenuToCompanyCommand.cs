using Application.Abstractions;
using Application.Common.Exceptions;
using Application.Menus.DTOs;
using Domain.Entities;
using FluentValidation;
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

public sealed class CreateMenuToCompanyCommandValidator : AbstractValidator<CreateMenuToCompanyCommand>
{
	public CreateMenuToCompanyCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Ana menü adı boş olamaz.")
			.MaximumLength(200).WithMessage("Ana menü adı en fazla 200 karakter olabilir.");

		RuleFor(x => x.CompanyId)
			.NotEmpty().WithMessage("Şirket seçilmelidir.");
	}
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
			Title = req.Title.Trim(),
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
