using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MapsterMapper;
using MediatR;

namespace Application.Menus.Commands;

public class DeleteMenuCommand : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
	public Guid Id { get; set; }
	public string ActionName => "Menü silindi";
	public Guid EntityId => Id;
}

public class DeleteMenuCommandHandler(
	IRepository<Menu> repoMenu,
	IMapper mapper
) : IRequestHandler<DeleteMenuCommand, Unit>
{
	public async Task<Unit> Handle(DeleteMenuCommand req, CancellationToken ct)
	{
		var menu = await repoMenu.GetByIdAsync(req.Id, ct);
		if(menu == null)
			throw new NotFoundAppException("Menü bulunamadı");
		repoMenu.SoftDelete(menu);
		return Unit.Value;
	}
}