using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Application.QRs.Queries;

namespace Application.Categories.Queries;

public record CategoryAnalysisResult(List<QRStatsDTO> ChartData, List<TopCategoryDTO> TopCategories);

public class TopCategoryDTO
{
    public string Title { get; set; } = null!;
    public int Views { get; set; }
}

public record GetCategoryAnalysisQuery(
    QRStatsGranularity Granularity = QRStatsGranularity.Monthly, 
    Guid? StoreId = null,
    Guid? CompanyId = null
) : IRequest<CategoryAnalysisResult>, IAuthorizedRequest;

public class GetCategoryAnalysisQueryHandler(
    IRepository<CategoryDailyStats> repoStats,
    IRepository<CategoryViewEvent> repoEvent,
    IRepository<Store> repoStore,
    IUserContext userContext
) : IRequestHandler<GetCategoryAnalysisQuery, CategoryAnalysisResult>
{
    public async Task<CategoryAnalysisResult> Handle(GetCategoryAnalysisQuery req, CancellationToken ct)
    {
        var userId = userContext.UserId;
        if (string.IsNullOrEmpty(userId)) return new CategoryAnalysisResult([], []);

        var isAdmin = userContext.IsAdmin;
        
        IQueryable<Store> storesQuery = repoStore.Query(tracked: false);
        
        if (!isAdmin)
        {
            storesQuery = storesQuery
                .Include(s => s.Company)
                    .ThenInclude(c => c.Owner)
                .Where(s => s.Company != null && s.Company.Owner != null && s.Company.Owner.ApplicationUserId == userId);
        }
        else if (req.CompanyId.HasValue)
        {
            storesQuery = storesQuery.Where(s => s.CompanyId == req.CompanyId.Value);
        }

        if (req.StoreId.HasValue)
        {
            storesQuery = storesQuery.Where(s => s.Id == req.StoreId.Value);
        }

        var categoryIds = await storesQuery
            .SelectMany(s => s.Menus.Where(m => m.Status == MenuStatus.Active)
                .SelectMany(m => m.Categories.Where(c => c.IsActive)))
            .Select(c => c.Id)
            .Distinct()
            .ToListAsync(ct);

        if (categoryIds.Count == 0) return new CategoryAnalysisResult([], []);

        var chartData = req.Granularity switch
        {
            QRStatsGranularity.Hourly => await GetHourlyStats(categoryIds, ct),
            QRStatsGranularity.Daily => await GetDailyStats(categoryIds, ct),
            QRStatsGranularity.Weekly => await GetWeeklyStats(categoryIds, ct),
            QRStatsGranularity.Monthly => await GetMonthlyStats(categoryIds, ct),
            QRStatsGranularity.Yearly => await GetYearlyStats(categoryIds, ct),
            _ => []
        };

        var topCategories = await GetTopCategories(categoryIds, ct);

        return new CategoryAnalysisResult(chartData, topCategories);
    }

    private async Task<List<TopCategoryDTO>> GetTopCategories(List<Guid> ids, CancellationToken ct)
    {
        return await repoStats.Query(tracked: false)
            .Include(s => s.Category)
                .ThenInclude(c => c.CategoryLibraryItem)
            .Where(s => ids.Contains(s.CategoryId))
            .GroupBy(s => s.Category.CategoryLibraryItem.Title)
            .Select(g => new TopCategoryDTO { Title = g.Key, Views = g.Sum(s => s.Views) })
            .OrderByDescending(x => x.Views)
            .Take(10)
            .ToListAsync(ct);
    }

    private async Task<List<QRStatsDTO>> GetHourlyStats(List<Guid> ids, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(3).AddHours(-23); // Turkey Time
        var views = await repoEvent.Query(tracked: false)
            .Where(s => ids.Contains(s.CategoryId) && s.ViewedAt >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 24; i++)
        {
            var hourDate = cutoff.AddHours(i);
            var count = views.Count(s => s.ViewedAt.Year == hourDate.Year && s.ViewedAt.Month == hourDate.Month && s.ViewedAt.Day == hourDate.Day && s.ViewedAt.Hour == hourDate.Hour);
            result.Add(new QRStatsDTO { Label = hourDate.ToString("HH:00"), Value = count });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetDailyStats(List<Guid> ids, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3).AddDays(-29));
        var stats = await repoStats.Query(tracked: false)
            .Where(s => ids.Contains(s.CategoryId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 30; i++)
        {
            var day = cutoff.AddDays(i);
            var count = stats.Where(s => s.Day == day).Sum(s => s.Views);
            result.Add(new QRStatsDTO { Label = day.ToString("dd MMM", new CultureInfo("tr-TR")), Value = count });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetWeeklyStats(List<Guid> ids, CancellationToken ct)
    {
        var result = new List<QRStatsDTO>();
        var culture = new CultureInfo("tr-TR");
        var cal = culture.Calendar;
        var now = DateTime.UtcNow.AddHours(3);
        var diff = (7 + (now.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek)) % 7;
        var startOfCurrentWeek = now.AddDays(-1 * diff).Date;
        var cutoffDate = DateOnly.FromDateTime(startOfCurrentWeek.AddDays(-11 * 7));
        
        var stats = await repoStats.Query(tracked: false)
            .Where(s => ids.Contains(s.CategoryId) && s.Day >= cutoffDate)
            .ToListAsync(ct);

        for (int i = 0; i < 12; i++)
        {
            var weekStart = startOfCurrentWeek.AddDays(-(11 - i) * 7);
            var weekEnd = weekStart.AddDays(6);
            var weekStartOnly = DateOnly.FromDateTime(weekStart);
            var weekEndOnly = DateOnly.FromDateTime(weekEnd);

            var count = stats.Where(s => s.Day >= weekStartOnly && s.Day <= weekEndOnly).Sum(s => s.Views);
            var weekNum = cal.GetWeekOfYear(weekStart, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
            result.Add(new QRStatsDTO { Label = $"{weekNum}. Hafta", Value = count });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetMonthlyStats(List<Guid> ids, CancellationToken ct)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3).AddMonths(-11));
        cutoff = new DateOnly(cutoff.Year, cutoff.Month, 1);
        var stats = await repoStats.Query(tracked: false)
            .Where(s => ids.Contains(s.CategoryId) && s.Day >= cutoff)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 0; i < 12; i++)
        {
            var date = DateTime.UtcNow.AddHours(3).AddMonths(-11 + i);
            var count = stats.Where(s => s.Day.Year == date.Year && s.Day.Month == date.Month).Sum(s => s.Views);
            result.Add(new QRStatsDTO { Label = date.ToString("MMMM", new CultureInfo("tr-TR")), Value = count });
        }
        return result;
    }

    private async Task<List<QRStatsDTO>> GetYearlyStats(List<Guid> ids, CancellationToken ct)
    {
        var currentYear = DateTime.UtcNow.AddHours(3).Year;
        var stats = await repoStats.Query(tracked: false)
            .Where(s => ids.Contains(s.CategoryId) && s.Day.Year > currentYear - 5)
            .ToListAsync(ct);

        var result = new List<QRStatsDTO>();
        for (int i = 4; i >= 0; i--)
        {
            var year = currentYear - i;
            var count = stats.Where(s => s.Day.Year == year).Sum(s => s.Views);
            result.Add(new QRStatsDTO { Label = year.ToString(), Value = count });
        }
        return result;
    }
}
