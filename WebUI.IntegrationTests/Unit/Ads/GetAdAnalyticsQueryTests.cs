using Application.Ads.Queries;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebUI.IntegrationTests.Unit.Ads;

public class GetAdAnalyticsQueryTests
{
    private readonly ApplicationDbContext _db;

    public GetAdAnalyticsQueryTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(opts);
    }

    private EfRepository<T> Repo<T>() where T : class => new(_db);

    [Fact]
    public async Task Handle_NoData_ReturnsZeroTotals()
    {
        var handler = new GetAdAnalyticsQueryHandler(
            Repo<AdImpression>(), Repo<AdClick>(), Repo<AdPlacement>());

        var result = await handler.Handle(new GetAdAnalyticsQuery(30), CancellationToken.None);

        result.TotalImpressions.Should().Be(0);
        result.TotalClicks.Should().Be(0);
        result.AverageCtr.Should().Be(0);
        result.ActivePlacementCount.Should().Be(0);
        result.DailyStats.Should().BeEmpty();
        result.PlacementStats.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithData_AggregatesCorrectly()
    {
        var slot = new AdSlot { Key = "header", IsActive = true, Description = "" };
        var creative = new AdCreative { Type = "image", Content = "img.png" };
        await _db.Set<AdSlot>().AddAsync(slot);
        await _db.Set<AdCreative>().AddAsync(creative);
        var placement = new AdPlacement
        {
            AdSlotId = slot.Id, AdCreativeId = creative.Id, IsActive = true
        };
        await _db.Set<AdPlacement>().AddAsync(placement);
        var now = DateTime.UtcNow;
        await _db.Set<AdImpression>().AddRangeAsync(
            new AdImpression { AdPlacementId = placement.Id, ShownAt = now, IpHash = "h1" },
            new AdImpression { AdPlacementId = placement.Id, ShownAt = now, IpHash = "h2" }
        );
        await _db.Set<AdClick>().AddAsync(
            new AdClick { AdPlacementId = placement.Id, ClickedAt = now, IpHash = "h1" }
        );
        await _db.SaveChangesAsync();

        var handler = new GetAdAnalyticsQueryHandler(
            Repo<AdImpression>(), Repo<AdClick>(), Repo<AdPlacement>());

        var result = await handler.Handle(new GetAdAnalyticsQuery(30), CancellationToken.None);

        result.TotalImpressions.Should().Be(2);
        result.TotalClicks.Should().Be(1);
        result.AverageCtr.Should().BeApproximately(50m, 0.01m);
        result.ActivePlacementCount.Should().Be(1);
        result.PlacementStats.Should().HaveCount(1);
        result.PlacementStats[0].SlotKey.Should().Be("header");
        result.PlacementStats[0].Impressions.Should().Be(2);
        result.PlacementStats[0].Clicks.Should().Be(1);
        result.PlacementStats[0].Ctr.Should().BeApproximately(50m, 0.01m);
        result.DailyStats.Should().HaveCount(1);
        result.DailyStats[0].Impressions.Should().Be(2);
        result.DailyStats[0].Clicks.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DaysFilter_ExcludesOldData()
    {
        var slot = new AdSlot { Key = "sidebar", IsActive = true, Description = "" };
        var creative = new AdCreative { Type = "image", Content = "old.png" };
        await _db.Set<AdSlot>().AddAsync(slot);
        await _db.Set<AdCreative>().AddAsync(creative);
        var placement = new AdPlacement { AdSlotId = slot.Id, AdCreativeId = creative.Id, IsActive = true };
        await _db.Set<AdPlacement>().AddAsync(placement);
        await _db.Set<AdImpression>().AddAsync(
            new AdImpression { AdPlacementId = placement.Id, ShownAt = DateTime.UtcNow.AddDays(-60), IpHash = "old" }
        );
        await _db.SaveChangesAsync();

        var handler = new GetAdAnalyticsQueryHandler(
            Repo<AdImpression>(), Repo<AdClick>(), Repo<AdPlacement>());

        var result = await handler.Handle(new GetAdAnalyticsQuery(30), CancellationToken.None);

        result.TotalImpressions.Should().Be(0);
        result.DailyStats.Should().BeEmpty();
    }
}
