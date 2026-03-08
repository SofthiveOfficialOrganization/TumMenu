using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Queries;

public record GetAdRevenueSummaryQuery(DateTime? FromDate = null, DateTime? ToDate = null) : IRequest<AdRevenueSummaryDTO>;

public record AdRevenueSummaryDTO
{
    public decimal TotalRevenue { get; init; }
    public decimal ThisMonthRevenue { get; init; }
    public decimal LastMonthRevenue { get; init; }
    public string Currency { get; init; } = "TRY";
}

public class GetAdRevenueSummaryQueryHandler(IRepository<AdRevenueImport> repo) 
    : IRequestHandler<GetAdRevenueSummaryQuery, AdRevenueSummaryDTO>
{
    public async Task<AdRevenueSummaryDTO> Handle(GetAdRevenueSummaryQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var firstDayThisMonth = new DateOnly(now.Year, now.Month, 1);
        var firstDayLastMonth = firstDayThisMonth.AddMonths(-1);
        var lastDayLastMonth = firstDayThisMonth.AddDays(-1);

        var allImports = await repo.Query().ToListAsync(ct);

        var total = allImports.Sum(x => x.Revenue);
        var thisMonth = allImports.Where(x => x.Day >= firstDayThisMonth).Sum(x => x.Revenue);
        var lastMonth = allImports.Where(x => x.Day >= firstDayLastMonth && x.Day <= lastDayLastMonth).Sum(x => x.Revenue);

        return new AdRevenueSummaryDTO
        {
            TotalRevenue = total,
            ThisMonthRevenue = thisMonth,
            LastMonthRevenue = lastMonth,
            Currency = allImports.FirstOrDefault()?.Currency ?? "TRY"
        };
    }
}
