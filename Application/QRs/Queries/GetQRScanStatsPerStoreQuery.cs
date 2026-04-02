using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.QRs.Queries;

public record GetQRScanStatsPerStoreQuery(Guid? CompanyId = null) : IRequest<List<QRStoreStatsDTO>>, IAuthorizedRequest;

public class QRStoreStatsDTO
{
    public Guid StoreId { get; set; }
    public string StoreTitle { get; set; } = null!;
    public int TotalScans { get; set; }
}

public class GetQRScanStatsPerStoreQueryHandler(
    IRepository<QRDailyStats> repoStats,
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetQRScanStatsPerStoreQuery, List<QRStoreStatsDTO>>
{
    public async Task<List<QRStoreStatsDTO>> Handle(GetQRScanStatsPerStoreQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId)) return new List<QRStoreStatsDTO>();

        var isAdmin = userContext.IsAdmin;
        
        IQueryable<Store> storesQuery = repoStore.Query(tracked: false)
            .Include(s => s.QRCode);

        if (!isAdmin)
        {
            storesQuery = storesQuery
                .Include(s => s.Company)
                    .ThenInclude(c => c.Owner)
                .Where(s => s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userId && s.QRCode != null);
        }
        else
        {
            if (req.CompanyId.HasValue)
            {
                storesQuery = storesQuery.Where(s => s.CompanyId == req.CompanyId.Value && s.QRCode != null);
            }
            else
            {
                storesQuery = storesQuery.Where(s => s.QRCode != null);
            }
        }

        var stores = await storesQuery.ToListAsync(ct);

        if (stores.Count == 0) return new List<QRStoreStatsDTO>();

        var qrIds = stores.Select(s => s.QRCode!.Id).ToList();

        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId))
            .ToListAsync(ct);

        var result = stores.Select(store => new QRStoreStatsDTO
        {
            StoreId = store.Id,
            StoreTitle = store.Title,
            TotalScans = stats.Where(s => s.QRCodeId == store.QRCode!.Id).Sum(s => s.Scans)
        })
        .OrderByDescending(r => r.TotalScans)
        .ToList();

        return result;
    }
}
