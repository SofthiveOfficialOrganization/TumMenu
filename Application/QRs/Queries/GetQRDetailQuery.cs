using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetQRDetailQuery(Guid StoreId) : IRequest<MyQRDTO?>;

public class GetQRDetailQueryHandler(
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetQRDetailQuery, MyQRDTO?>
{
    public async Task<MyQRDTO?> Handle(GetQRDetailQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId)) return null;

        var store = await repoStore.Query(tracked: false)
            .Include(s => s.Company)
                .ThenInclude(c => c.Owner)
            .Include(s => s.QRCode)
            .Where(s => s.Id == req.StoreId && s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userId)
            .FirstOrDefaultAsync(ct);

        if (store == null || store.QRCode == null || store.Company == null) return null;

        return new MyQRDTO
        {
            StoreId = store.Id,
            StoreTitle = store.Title,
            CompanyTitle = store.Company.Title,
            QRKey = store.QRCode.PublicKey,
            PublicUrl = $"/q/{store.QRCode.PublicKey}",
            BaseDomain = store.QRCode.BaseDomain
        };
    }
}
