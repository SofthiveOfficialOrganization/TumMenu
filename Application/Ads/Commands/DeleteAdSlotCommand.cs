using Application.Abstractions;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class DeleteAdSlotCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class DeleteAdSlotCommandHandler(IRepository<AdSlot> repo) : IRequestHandler<DeleteAdSlotCommand, bool>
{
    public async Task<bool> Handle(DeleteAdSlotCommand request, CancellationToken ct)
    {
        var slot = await repo.GetByIdAsync(request.Id, ct);
        if (slot == null) return false;
        repo.SoftDelete(slot);
        return true;
    }
}
