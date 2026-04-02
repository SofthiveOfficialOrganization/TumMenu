using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.Dashboard.Queries;

public record GetQRChartDataQuery(string Period) : IRequest<QRChartDataDto>, IAuthorizedRequest;

public record QRChartDataDto(List<string> Labels, List<QRChartDatasetDto> Datasets);

public record QRChartDatasetDto(string StoreTitle, List<int> Data);

public class GetQRChartDataQueryHandler(
    IRepository<Store> repoStore,
    IRepository<QRDailyStats> repoStats,
    IUserContext userContext
) : IRequestHandler<GetQRChartDataQuery, QRChartDataDto>
{
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public async Task<QRChartDataDto> Handle(GetQRChartDataQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        var isAdmin = userContext.IsAdmin;

        var storesQuery = repoStore.Query(tracked: false)
            .Include(s => s.QRCode)
            .Include(s => s.Company).ThenInclude(c => c.Owner)
            .Where(s => s.QRCode != null);

        if (!isAdmin)
            storesQuery = storesQuery.Where(s =>
                s.Company != null && s.Company.Owner != null &&
                s.Company.Owner.ApplicationUserId == userId);

        var stores = await storesQuery.ToListAsync(ct);
        if (stores.Count == 0)
            return new QRChartDataDto([], []);

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3));

        if (req.Period == "monthly")
            return await BuildMonthlyChart(stores, now, ct);

        return await BuildWeeklyChart(stores, now, ct);
    }

    private async Task<QRChartDataDto> BuildWeeklyChart(List<Store> stores, DateTimeOffset now, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(now.AddDays(-6).Date);
        var qrIds = stores.Select(s => s.QRCode!.Id).ToList();

        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var labels = Enumerable.Range(0, 7)
            .Select(i => DateOnly.FromDateTime(now.AddDays(-6 + i).Date))
            .ToList();

        var labelStrings = labels.Select(d => d.ToString("dd MMM", TrCulture)).ToList();

        var datasets = stores.Select(store => new QRChartDatasetDto(
            store.Title,
            labels.Select(day =>
                stats.Where(s => s.QRCodeId == store.QRCode!.Id && s.Day == day).Sum(s => s.Scans)
            ).ToList()
        )).ToList();

        return new QRChartDataDto(labelStrings, datasets);
    }

    private async Task<QRChartDataDto> BuildMonthlyChart(List<Store> stores, DateTimeOffset now, CancellationToken ct)
    {
        var cutoff = new DateOnly(now.AddMonths(-11).Year, now.AddMonths(-11).Month, 1);
        var qrIds = stores.Select(s => s.QRCode!.Id).ToList();

        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var months = Enumerable.Range(0, 12)
            .Select(i => now.AddMonths(-11 + i))
            .ToList();

        var labelStrings = months.Select(m => m.ToString("MMMM", TrCulture)).ToList();

        var datasets = stores.Select(store => new QRChartDatasetDto(
            store.Title,
            months.Select(m =>
                stats.Where(s => s.QRCodeId == store.QRCode!.Id && s.Day.Year == m.Year && s.Day.Month == m.Month)
                     .Sum(s => s.Scans)
            ).ToList()
        )).ToList();

        return new QRChartDataDto(labelStrings, datasets);
    }
}
