using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetStoresByCompanyQuery(Guid CompanyId) : IRequest<List<MyQRDTO>>, IAuthorizedRequest;

public class GetStoresByCompanyQueryHandler(
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetStoresByCompanyQuery, List<MyQRDTO>>
{
    public async Task<List<MyQRDTO>> Handle(GetStoresByCompanyQuery req, CancellationToken ct)
    {
        // Only admins can query by company specifically this way for now
        if (!userContext.IsAdmin)
            return new List<MyQRDTO>();

        var stores = await repoStore.Query(tracked: false)
            .Include(s => s.Company)
            .Include(s => s.QRCode)
            .Where(s => s.CompanyId == req.CompanyId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        return stores
            .Where(s => s.QRCode != null && s.Company != null)
            .Select(store => new MyQRDTO
            {
                StoreId = store.Id,
                StoreTitle = store.Title,
                CompanyTitle = store.Company!.Title,
                QRKey = store.QRCode!.PublicKey,
                PublicUrl = $"/q/{store.QRCode!.PublicKey}",
                BaseDomain = store.QRCode!.BaseDomain,
                MenuUrl = $"/{store.Company!.Slug}/{store.Slug}"
            })
            .ToList();
    }
}
