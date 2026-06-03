using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Menus.Commands;

public sealed record UpdateMenuCommand(
	Guid Id,
	string? Title
) : IRequest<Menu>, ITransactionalRequest, IEntityAuditableCommand
{
	public string ActionName => "Menü güncellendi";
	public Guid EntityId => Id;
}

public sealed class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
{
	public UpdateMenuCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("Menü bulunamadı.");

		RuleFor(x => x.Title)
			.NotEmpty().WithMessage("Menü adı boş olamaz.")
			.MaximumLength(200).WithMessage("Menü adı en fazla 200 karakter olabilir.");
	}
}

public class UpdateMenuCommandHandler(
	IRepository<Menu> repoMenu
) : IRequestHandler<UpdateMenuCommand, Menu>
{
	public async Task<Menu> Handle(UpdateMenuCommand req, CancellationToken ct)
	{
		var menu = await repoMenu.GetByIdAsync(req.Id, ct);
		if(menu is null)
			throw new NotFoundAppException("Menü bulunamadı.");
		if(req.Title is not null)
			menu.Title = req.Title.Trim();

		repoMenu.Update(menu);
		return menu;
	}
}
