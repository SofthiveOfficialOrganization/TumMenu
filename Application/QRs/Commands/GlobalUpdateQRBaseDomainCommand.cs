using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Commands;

public class GlobalUpdateQRBaseDomainCommand : IRequest<int>
{
    public string NewBaseDomain { get; set; } = string.Empty;
}

public class GlobalUpdateQRBaseDomainCommandHandler(IRepository<QRCode> repoQR) : IRequestHandler<GlobalUpdateQRBaseDomainCommand, int>
{
    public async Task<int> Handle(GlobalUpdateQRBaseDomainCommand req, CancellationToken ct)
    {
        // For now, we update all existing ones.
        var qrs = await repoQR.Query(tracked: true).ToListAsync(ct);
        foreach (var qr in qrs)
        {
            qr.BaseDomain = req.NewBaseDomain;
        }

        await repoQR.SaveChangesAsync(ct);
        return qrs.Count;
    }
}
