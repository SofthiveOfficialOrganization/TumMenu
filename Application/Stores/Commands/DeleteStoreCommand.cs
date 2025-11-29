using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.Stores.Commands;

public sealed record DeleteStoreCommand
(
    Guid Id
) : IRequest<Unit>, ITransactionalRequest;


public class DeleteStoreCommandHandler(
    IRepository<Store> repoStore
) : IRequestHandler<DeleteStoreCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStoreCommand req, CancellationToken ct)
    {
        var store = await repoStore.GetByIdAsync(req.Id, ct);
        if(store != null)
        {
            repoStore.SoftDelete(store);
        }
        return Unit.Value;
    }
}