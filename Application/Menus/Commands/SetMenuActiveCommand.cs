using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Menus.Commands;

public class SetMenuActiveCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class SetMenuActiveHandler(IRepository<Menu> repoMenu) : IRequestHandler<SetMenuActiveCommand, bool>
{
    public async Task<bool> Handle(SetMenuActiveCommand req, CancellationToken ct)
    {
        var menu = await repoMenu.Query(tracked: true)
            .FirstOrDefaultAsync(x => x.Id == req.Id, ct);

        if (menu == null) return false;

        if (menu.Status == MenuStatus.Active) return true; // Already active

        // Deactivate other menus in the same context
        if (menu.StoreId.HasValue)
        {
            var otherActiveMenus = await repoMenu.Query(tracked: true)
                .Where(x => x.StoreId == menu.StoreId && x.Status == MenuStatus.Active && x.Id != menu.Id)
                .ToListAsync(ct);

            foreach (var other in otherActiveMenus)
            {
                other.Status = MenuStatus.Inactive;
            }
        }
        else if (menu.CompanyId.HasValue)
        {
            var otherActiveMenus = await repoMenu.Query(tracked: true)
                .Where(x => x.CompanyId == menu.CompanyId && x.Status == MenuStatus.Active && x.Id != menu.Id)
                .ToListAsync(ct);

            foreach (var other in otherActiveMenus)
            {
                other.Status = MenuStatus.Inactive;
            }
        }

        menu.Status = MenuStatus.Active;
        // SaveChanges is handled by pipeline
        return true;
    }
}
