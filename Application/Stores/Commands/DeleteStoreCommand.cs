using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stores.Commands;

public sealed record DeleteStoreCommand
(
    Guid Id
) : IRequest<Unit>, ITransactionalRequest;


public class DeleteStoreCommandHandler(
    IRepository<Store> repoStore,
    IRepository<Menu> repoMenu
) : IRequestHandler<DeleteStoreCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStoreCommand req, CancellationToken ct)
    {
        var store = await repoStore.GetByIdAsync(req.Id, ct);
        if(store != null)
        {
            repoStore.SoftDelete(store);
            
            // Soft delete menus associated with this store
            var menus = await repoMenu.Query(tracked: true)
                .Where(m => m.StoreId == store.Id)
                .ToListAsync(ct);
                
            foreach (var menu in menus)
            {
                repoMenu.SoftDelete(menu);
            }
        }
        return Unit.Value;
    }
}