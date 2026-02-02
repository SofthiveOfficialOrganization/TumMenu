using Application.Abstractions;
using Domain.Entities;
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
	string? Name
) : IRequest<Menu>, ITransactionalRequest;

public class UpdateMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<UpdateMenuCommand, Menu>
{
	public async Task<Menu> Handle(UpdateMenuCommand req, CancellationToken ct)
	{
		var menu = await repoMenu.GetByIdAsync(req.Id, ct);
		if(menu is null)
			throw new KeyNotFoundException("Menü bulunamadı.");
		if(req.Name is not null)
			menu.Title = req.Name;

		repoMenu.Update(menu);
		return menu;
	}
}