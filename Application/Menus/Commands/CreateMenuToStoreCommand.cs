using Application.Abstractions;
using Application.Menus.DTOs;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class CreateMenuToStoreCommand : IRequest<MenuDTO>, ITransactionalRequest, IAuditableCommand
{
	public string ActionName => "Menü oluşturuldu";
	public string Title { get; set; } = string.Empty;
	public Guid StoreId { get; set; }
	public MenuStatus Status { get; set; } = MenuStatus.Inactive;
}

public sealed class CreateMenuToStoreCommandValidator : AbstractValidator<CreateMenuToStoreCommand>
{
	public CreateMenuToStoreCommandValidator()
	{
		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Menü adı boş olamaz.")
			.MaximumLength(200).WithMessage("Menü adı en fazla 200 karakter olabilir.");

		RuleFor(x => x.StoreId)
			.NotEmpty().WithMessage("Dükkan seçilmelidir.");

		RuleFor(x => x.Status)
			.Must(status => status is MenuStatus.Active or MenuStatus.Inactive or MenuStatus.Draft or MenuStatus.Archived)
			.WithMessage("Geçersiz menü durumu.");
	}
}

public class CreateMenuToStoreCommandHandler(
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
		menu.Title = req.Title.Trim();
		await repoMenu.AddAsync(menu, ct);
		var menuDTO = mapper.Map<MenuDTO>(menu);
		return menuDTO;
	}
}
