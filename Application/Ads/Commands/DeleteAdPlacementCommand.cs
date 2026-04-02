using Application.Abstractions;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class DeleteAdPlacementCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class DeleteAdPlacementCommandHandler(IRepository<AdPlacement> repo) : IRequestHandler<DeleteAdPlacementCommand, bool>
{
    public async Task<bool> Handle(DeleteAdPlacementCommand request, CancellationToken ct)
    {
        var placement = await repo.GetByIdAsync(request.Id, ct);
        if (placement == null) return false;
        repo.SoftDelete(placement);
        return true;
    }
}
