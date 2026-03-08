using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Categories.Commands;

public record RecordCategoryViewCommand(
    Guid CategoryId,
    string UserAgent,
    string IpAddress
) : IRequest;

public class RecordCategoryViewCommandHandler(
    IRepository<CategoryViewEvent> repoEvent,
    IRepository<CategoryDailyStats> repoStats,
    IUnitOfWork uow
) : IRequestHandler<RecordCategoryViewCommand>
{
    public async Task Handle(RecordCategoryViewCommand req, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(3); // Turkey Time
        var today = DateOnly.FromDateTime(now);
        var ipAddress = string.IsNullOrWhiteSpace(req.IpAddress) ? "unknown" : req.IpAddress;
        var ipHash = HashIP(ipAddress);

        // 1. Create View Event
        var viewEvent = new CategoryViewEvent
        {
            CategoryId = req.CategoryId,
            ViewedAt = now,
            UserAgent = req.UserAgent ?? "unknown",
            IpHash = ipHash,
            DeviceType = DetectDeviceType(req.UserAgent ?? "")
        };

        await repoEvent.AddAsync(viewEvent, ct);

        // 2. Update Daily Stats
        var stats = await repoStats.Query(tracked: true)
            .FirstOrDefaultAsync(s => s.CategoryId == req.CategoryId && s.Day == today, ct);

        if (stats == null)
        {
            stats = new CategoryDailyStats
            {
                CategoryId = req.CategoryId,
                Day = today,
                Views = 1,
                UniqueIps = 1
            };
            await repoStats.AddAsync(stats, ct);
        }
        else
        {
            stats.Views++;
            
            // Check if this IP has viewed this category today
            var startOfToday = today.ToDateTime(TimeOnly.MinValue); // This is TR date start
            var alreadyViewedToday = await repoEvent.Query(tracked: false)
                .AnyAsync(v => v.CategoryId == req.CategoryId 
                            && v.IpHash == ipHash 
                            && v.ViewedAt >= startOfToday
                            && v.Id != viewEvent.Id, ct);

            if (!alreadyViewedToday)
            {
                stats.UniqueIps++;
            }
        }

        await uow.SaveChangesAsync(ct);
    }

    private string HashIP(string ip)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes);
    }

    private string DetectDeviceType(string ua)
    {
        ua = ua.ToLower();
        if (ua.Contains("mobi") || ua.Contains("android") || ua.Contains("iphone"))
            return "mobile";
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return "tablet";
        return "desktop";
    }
}
