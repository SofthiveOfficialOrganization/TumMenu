using Application.Abstractions;
using Application.Ads.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Ads.Queries;

public record GetAdAnalyticsQuery(int Days = 30) : IRequest<AdAnalyticsDTO>;

public class GetAdAnalyticsQueryHandler(
    IRepository<AdImpression> impressionRepo,
    IRepository<AdClick> clickRepo,
    IRepository<AdPlacement> placementRepo)
    : IRequestHandler<GetAdAnalyticsQuery, AdAnalyticsDTO>
{
    public async Task<AdAnalyticsDTO> Handle(GetAdAnalyticsQuery request, CancellationToken ct)
    {
        var from = DateTime.UtcNow.AddDays(-request.Days).Date;

        var impressions = await impressionRepo.Query()
            .Where(i => i.ShownAt >= from)
            .ToListAsync(ct);

        var clicks = await clickRepo.Query()
            .Where(c => c.ClickedAt >= from)
            .ToListAsync(ct);

        var placements = await placementRepo.Query()
            .Include(p => p.AdSlot)
            .Include(p => p.AdCreative)
            .ToListAsync(ct);

        var totalImpressions = impressions.Count;
        var totalClicks = clicks.Count;
        var activePlacementCount = placements.Count(p => p.IsActive);

        var impressionsByDay = impressions
            .GroupBy(i => DateOnly.FromDateTime(i.ShownAt))
            .ToDictionary(g => g.Key, g => g.Count());

        var clicksByDay = clicks
            .GroupBy(c => DateOnly.FromDateTime(c.ClickedAt))
            .ToDictionary(g => g.Key, g => g.Count());

        var allDays = impressionsByDay.Keys.Union(clicksByDay.Keys).OrderBy(d => d);
        var dailyStats = allDays.Select(d => new DailyStatDTO
        {
            Date = d,
            Impressions = impressionsByDay.GetValueOrDefault(d, 0),
            Clicks = clicksByDay.GetValueOrDefault(d, 0)
        }).ToList();

        var impressionsByPlacement = impressions
            .GroupBy(i => i.AdPlacementId)
            .ToDictionary(g => g.Key, g => g.Count());

        var clicksByPlacement = clicks
            .GroupBy(c => c.AdPlacementId)
            .ToDictionary(g => g.Key, g => g.Count());

        var placementStats = placements
            .Select(p =>
            {
                var imp = impressionsByPlacement.GetValueOrDefault(p.Id, 0);
                var clk = clicksByPlacement.GetValueOrDefault(p.Id, 0);
                var ctr = imp > 0 ? Math.Round((decimal)clk / imp * 100, 2) : 0m;
                return new PlacementStatDTO
                {
                    PlacementId = p.Id,
                    SlotKey = p.AdSlot.Key,
                    CreativeType = p.AdCreative.Type,
                    Impressions = imp,
                    Clicks = clk,
                    Ctr = ctr,
                    IsActive = p.IsActive
                };
            })
            .OrderByDescending(p => p.Impressions)
            .ToList();

        var averageCtr = placementStats.Count > 0
            ? Math.Round(placementStats.Average(p => p.Ctr), 2)
            : 0m;

        return new AdAnalyticsDTO
        {
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            AverageCtr = averageCtr,
            ActivePlacementCount = activePlacementCount,
            DailyStats = dailyStats,
            PlacementStats = placementStats
        };
    }
}
