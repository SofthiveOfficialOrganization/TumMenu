using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.QRs.Commands;

public record GenerateQRCodeCommand(Guid StoreId) : IRequest<Guid>;

public class GenerateQRCodeCommandHandler(
    IRepository<QRCode> repoQR
) : IRequestHandler<GenerateQRCodeCommand, Guid>
{
    public async Task<Guid> Handle(GenerateQRCodeCommand req, CancellationToken ct)
    {
        // 1. Generate unique key
        string key = await GenerateUniqueKey(ct);

        // 2. Create QRCode
        var qr = new QRCode
        {
            PublicKey = key,
            StoreId = req.StoreId,
            ResolveMode = QRResolveMode.LatestActive,
            IsActive = true
        };

        await repoQR.AddAsync(qr, ct);
        // We don't save changes here if we assume it's part of a larger transaction 
        // that implements ITransactionalRequest. 
        // But for standalone use, we might need it.
        // The CreateStoreCommand is transactional.

        return qr.Id;
    }

    private async Task<string> GenerateUniqueKey(CancellationToken ct)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        
        while (true)
        {
            var key = new string(Enumerable.Repeat(chars, 7)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            bool exists = await repoQR.ExistsAsync(q => q.PublicKey == key, ct);
            if (!exists) return key;
        }
    }
}
