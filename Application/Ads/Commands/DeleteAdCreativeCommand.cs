using Application.Abstractions;
using Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Commands;

public class DeleteAdCreativeCommand : IRequest<bool>, ITransactionalRequest
{
    public Guid Id { get; set; }
}

public class DeleteAdCreativeCommandHandler(IRepository<AdCreative> repo) : IRequestHandler<DeleteAdCreativeCommand, bool>
{
    public async Task<bool> Handle(DeleteAdCreativeCommand request, CancellationToken ct)
    {
        var creative = await repo.GetByIdAsync(request.Id, ct);
        if (creative == null) return false;
        repo.SoftDelete(creative);
        return true;
    }
}
