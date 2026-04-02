using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Application.QRs.Queries;

public enum QRStatsGranularity { Hourly, Daily, Weekly, Monthly, Yearly }

public record GetQRScanStatsQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<List<QRStatsDTO>>, IAuthorizedRequest;

public class QRStatsDTO
{
    public string Label { get; set; } = null!;
    public int Value { get; set; }
}

public class GetQRScanStatsQueryHandler(
    IRepository<QRDailyStats> repoStats,
    IRepository<QRScanEvent> repoScan,
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetQRScanStatsQuery, List<QRStatsDTO>>
{
    public async Task<List<QRStatsDTO>> Handle(GetQRScanStatsQuery req, CancellationToken ct)
    {
        var isAdmin = userContext.IsAdmin;
        
        // Find all QR IDs
        IQueryable<Store> qrIdsQuery = repoStore.Query(tracked: false)
            .Include(s => s.QRCode)
            .Where(s => s.QRCode != null);

        if (!isAdmin)
        {
            qrIdsQuery = qrIdsQuery
                .Include(s => s.Company)
                    .ThenInclude(c => c.Owner)
                .Where(s => s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userContext.UserId);
        }
        else if (req.CompanyId.HasValue)
        {
            qrIdsQuery = qrIdsQuery.Where(s => s.CompanyId == req.CompanyId.Value);
        }

        if (req.StoreId.HasValue)
        {
            qrIdsQuery = qrIdsQuery.Where(s => s.Id == req.StoreId.Value);
        }

        var qrIds = await qrIdsQuery
            .Select(s => s.QRCode!.Id)
            .ToListAsync(ct);

        if (qrIds.Count == 0) return new List<QRStatsDTO>();

        return req.Granularity switch
        {
            QRStatsGranularity.Hourly => await GetHourlyStats(qrIds, ct),
            QRStatsGranularity.Daily => await GetDailyStats(qrIds, ct),
            QRStatsGranularity.Weekly => await GetWeeklyStats(qrIds, ct),
            QRStatsGranularity.Monthly => await GetMonthlyStats(qrIds, ct),
            QRStatsGranularity.Yearly => await GetYearlyStats(qrIds, ct),
            _ => new List<QRStatsDTO>()
        };
    }

    private async Task<List<QRStatsDTO>> GetHourlyStats(List<Guid> qrIds, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(3).AddHours(-23); // Turkey Time
        var scans = await repoScan.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.ScannedAt >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 24; i++)
        {
            var hourDate = cutoff.AddHours(i);
            var count = scans.Count(s => s.ScannedAt.Year == hourDate.Year && s.ScannedAt.Month == hourDate.Month && s.ScannedAt.Day == hourDate.Day && s.ScannedAt.Hour == hourDate.Hour);
            result.Add(new QRStatsDTO
            {
                Label = hourDate.ToString("HH:00"),
                Value = count
            });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetDailyStats(List<Guid> qrIds, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3).AddDays(-29));
        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 30; i++)
        {
            var day = cutoff.AddDays(i);
            var count = stats.Where(s => s.Day == day).Sum(s => s.Scans);
            result.Add(new QRStatsDTO
            {
                Label = day.ToString("dd MMM", new CultureInfo("tr-TR")),
                Value = count
            });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetWeeklyStats(List<Guid> qrIds, CancellationToken ct)
    {
        var result = new List<QRStatsDTO>();
        var culture = new CultureInfo("tr-TR");
        var cal = culture.Calendar;

        // Get the start of the current week (Monday for tr-TR)
        var now = DateTime.UtcNow.AddHours(3);
        var diff = (7 + (now.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek)) % 7;
        var startOfCurrentWeek = now.AddDays(-1 * diff).Date;

        // Fetch data for the last 12 weeks
        var cutoffDate = DateOnly.FromDateTime(startOfCurrentWeek.AddDays(-11 * 7));
        
        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= cutoffDate)
            .ToListAsync(ct);

        for (int i = 0; i < 12; i++)
        {
            var weekStart = startOfCurrentWeek.AddDays(-(11 - i) * 7);
            var weekEnd = weekStart.AddDays(6);
            
            var weekStartOnly = DateOnly.FromDateTime(weekStart);
            var weekEndOnly = DateOnly.FromDateTime(weekEnd);

            var count = stats.Where(s => s.Day >= weekStartOnly && s.Day <= weekEndOnly).Sum(s => s.Scans);
            var weekNum = cal.GetWeekOfYear(weekStart, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);

            result.Add(new QRStatsDTO
            {
                Label = $"{weekNum}. Hafta",
                Value = count
            });
        }

        return result;
    }

    private async Task<List<QRStatsDTO>> GetMonthlyStats(List<Guid> qrIds, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3).AddMonths(-11));
        cutoff = new DateOnly(cutoff.Year, cutoff.Month, 1);

        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 12; i++)
        {
            var date = DateTime.UtcNow.AddHours(3).AddMonths(-11 + i);
            var count = stats.Where(s => s.Day.Year == date.Year && s.Day.Month == date.Month).Sum(s => s.Scans);
            result.Add(new QRStatsDTO
            {
                Label = date.ToString("MMMM", new CultureInfo("tr-TR")),
                Value = count
            });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetYearlyStats(List<Guid> qrIds, CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.AddHours(3).Year;
        var stats = await repoStats.Query(tracked: false)
            .Where(s => qrIds.Contains(s.QRCodeId) && s.Day.Year > currentYear - 5)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 4; i >= 0; i--)
        {
            var year = currentYear - i;
            var count = stats.Where(s => s.Day.Year == year).Sum(s => s.Scans);
            result.Add(new QRStatsDTO
            {
                Label = year.ToString(),
                Value = count
            });
        }
        return result;
    }
}
