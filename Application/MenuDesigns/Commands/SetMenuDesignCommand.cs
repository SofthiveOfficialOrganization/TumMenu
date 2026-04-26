using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.MenuDesigns.Commands;

public sealed record SetMenuDesignCommand(
    Guid MenuId,
    Guid? MenuDesignId
) : IRequest<Unit>, ITransactionalRequest, IEntityAuditableCommand
{
    public string ActionName => "Menü tasarımı güncellendi";
    public Guid EntityId => MenuId;
}

public class SetMenuDesignCommandHandler(
    IRepository<Menu> repoMenu
) : IRequestHandler<SetMenuDesignCommand, Unit>
{
    public async Task<Unit> Handle(SetMenuDesignCommand req, CancellationToken ct)
    {
        var menu = await repoMenu.GetByIdAsync(req.MenuId, ct);
        if (menu is null)
            throw new KeyNotFoundException("Menü bulunamadı.");

        menu.MenuDesignId = req.MenuDesignId;
        repoMenu.Update(menu);

        return Unit.Value;
    }
}
