using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.CustomerOrderRequests.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomerOrderRequests.Commands;

public sealed record CreateQrOrderSessionCommand(
    Guid QRCodeId,
    string? RemoteIp,
    string? UserAgent
) : IRequest<QrOrderSessionDTO>;

public sealed class CreateQrOrderSessionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateQrOrderSessionCommand, QrOrderSessionDTO>
{
    public async Task<QrOrderSessionDTO> Handle(CreateQrOrderSessionCommand request, CancellationToken ct)
    {
        var qr = await db.QRCodes
            .Include(x => x.Store)
                .ThenInclude(x => x!.Company)
            .FirstOrDefaultAsync(x => x.Id == request.QRCodeId && x.IsActive, ct);

        if (qr?.Store?.Company is null)
        {
            throw new NotFoundAppException("QR kod için dükkan bulunamadı.");
        }

        var token = OrderSessionToken.Create();
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
        var session = new QrOrderSession
        {
            Id = Guid.NewGuid(),
            StoreId = qr.Store.Id,
            CompanyId = qr.Store.Company.Id,
            QRCodeId = qr.Id,
            TokenHash = OrderSessionToken.Hash(token),
            ExpiresAt = now.AddMinutes(30),
            LastUsedAt = now,
            CreatedIp = Trim(request.RemoteIp, 128),
            UserAgent = Trim(request.UserAgent, 512)
        };
        session.Created();

        await db.QrOrderSessions.AddAsync(session, ct);
        await db.SaveChangesAsync(ct);

        return new QrOrderSessionDTO
        {
            Id = session.Id,
            StoreId = session.StoreId,
            CompanyId = session.CompanyId,
            QRCodeId = session.QRCodeId,
            Token = token,
            ExpiresAt = session.ExpiresAt
        };
    }

    private static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
