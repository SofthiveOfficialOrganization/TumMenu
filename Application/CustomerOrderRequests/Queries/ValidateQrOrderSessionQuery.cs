using Application.Common.Interfaces;
using Application.CustomerOrderRequests.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Queries;

public sealed record ValidateQrOrderSessionQuery(string? Token, Guid? StoreId = null)
    : IRequest<QrOrderSessionStatusDTO>;

public sealed class ValidateQrOrderSessionQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ValidateQrOrderSessionQuery, QrOrderSessionStatusDTO>
{
    public async Task<QrOrderSessionStatusDTO> Handle(ValidateQrOrderSessionQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new QrOrderSessionStatusDTO();
        }

        var tokenHash = OrderSessionToken.Hash(request.Token);
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));

        var session = await db.QrOrderSessions
            .Where(x => x.TokenHash == tokenHash && !x.IsRevoked && x.ExpiresAt > now)
            .Select(x => new
            {
                x.Id,
                x.StoreId,
                x.CompanyId,
                x.ExpiresAt
            })
            .FirstOrDefaultAsync(ct);

        if (session is null || (request.StoreId.HasValue && session.StoreId != request.StoreId.Value))
        {
            return new QrOrderSessionStatusDTO();
        }

        return new QrOrderSessionStatusDTO
        {
            IsValid = true,
            StoreId = session.StoreId,
            CompanyId = session.CompanyId,
            ExpiresAt = session.ExpiresAt
        };
    }
}
