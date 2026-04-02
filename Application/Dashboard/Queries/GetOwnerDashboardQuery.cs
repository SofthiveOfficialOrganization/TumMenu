using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Dashboard.Queries;

public record GetOwnerDashboardQuery : IRequest<OwnerDashboardDto>, IAuthorizedRequest;

public record OwnerDashboardDto(
    int StoreCount,
    int MenuCount,
    int ProductCount,
    int CurrentMonthQRScans,
    double MonthOverMonthPercent,
    List<TopProductDashboardDto> TopProducts,
    List<RecentActivityDto> RecentActivity
);

public record TopProductDashboardDto(string Title, string StoreName, int Views);

public record RecentActivityDto(string Time, string Description, string StoreName);

public class GetOwnerDashboardQueryHandler(
    IRepository<Store> repoStore,
    IRepository<Menu> repoMenu,
    IRepository<Product> repoProduct,
    IRepository<QRDailyStats> repoQRStats,
    IRepository<ProductDailyStats> repoProductStats,
    IRepository<AuditLog> repoAuditLog,
    IUserContext userContext
) : IRequestHandler<GetOwnerDashboardQuery, OwnerDashboardDto>
{
    public async Task<OwnerDashboardDto> Handle(GetOwnerDashboardQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        var isAdmin = userContext.IsAdmin;

        // --- Owner's stores ---
        var storesQuery = repoStore.Query(tracked: false)
            .Include(s => s.QRCode)
            .Include(s => s.Company).ThenInclude(c => c.Owner);

        var stores = isAdmin
            ? await storesQuery.ToListAsync(ct)
            : await storesQuery
                .Where(s => s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userId)
                .ToListAsync(ct);

        var storeIds = stores.Select(s => s.Id).ToList();
        var companyIds = stores.Select(s => s.CompanyId).Distinct().ToList();

        // --- Counts ---
        int storeCount = stores.Count;

        int menuCount = await repoMenu.Query(tracked: false)
            .Where(m =>
                (m.CompanyId != null && companyIds.Contains(m.CompanyId.Value)) ||
                (m.StoreId != null && storeIds.Contains(m.StoreId.Value)))
            .CountAsync(ct);

        int productCount = await repoProduct.Query(tracked: false)
            .Where(p => storeIds.Contains(p.Category.Menu.Store!.Id) || companyIds.Contains(p.Category.Menu.Company!.Id))
            .CountAsync(ct);

        // --- QR scans ---
        var qrIds = stores.Where(s => s.QRCode != null).Select(s => s.QRCode!.Id).ToList();
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));
        var thisMonthStart = new DateOnly(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        int currentMonthQR = 0;
        int lastMonthQR = 0;

        if (qrIds.Count > 0)
        {
            var qrStats = await repoQRStats.Query(tracked: false)
                .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= lastMonthStart)
                .ToListAsync(ct);

            currentMonthQR = qrStats.Where(s => s.Day >= thisMonthStart).Sum(s => s.Scans);
            lastMonthQR = qrStats.Where(s => s.Day >= lastMonthStart && s.Day < thisMonthStart).Sum(s => s.Scans);
        }

        double momPercent = lastMonthQR == 0
            ? (currentMonthQR > 0 ? 100.0 : 0.0)
            : Math.Round((currentMonthQR - lastMonthQR) / (double)lastMonthQR * 100, 1);

        // --- Top 5 products this month ---
        List<TopProductDashboardDto> topProducts = [];
        if (storeIds.Count > 0)
        {
            var rawStats = await repoProductStats.Query(tracked: false)
                .Where(s => s.Day >= thisMonthStart &&
                            s.Product.Category.Menu.StoreId != null &&
                            storeIds.Contains(s.Product.Category.Menu.Store!.Id))
                .Select(s => new { s.Product.Title, StoreName = s.Product.Category.Menu.Store!.Title, s.Views })
                .ToListAsync(ct);

            topProducts = rawStats
                .GroupBy(s => new { s.Title, s.StoreName })
                .Select(g => new TopProductDashboardDto(g.Key.Title, g.Key.StoreName, g.Sum(s => s.Views)))
                .OrderByDescending(x => x.Views)
                .Take(5)
                .ToList();
        }

        // --- Recent activity: owner sees their own recent actions from AuditLogs ---
        var recentActivity = await repoAuditLog.Query(tracked: false)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentActivityDto(
                a.CreatedAt.ToOffset(TimeSpan.FromHours(3)).ToString("HH:mm"),
                a.Action,
                a.Entity))
            .ToListAsync(ct);

        return new OwnerDashboardDto(
            storeCount,
            menuCount,
            productCount,
            currentMonthQR,
            momPercent,
            topProducts,
            recentActivity
        );
    }
}
