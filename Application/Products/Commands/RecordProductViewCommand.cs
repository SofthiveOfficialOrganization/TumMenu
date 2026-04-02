using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.Products.Commands;

public class RecordProductViewCommand : IRequest
{
    public Guid ProductId { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}

public class RecordProductViewCommandHandler(
    IRepository<ProductViewEvent> repoEvent,
    IRepository<ProductDailyStats> repoStats,
    IUnitOfWork uow
) : IRequestHandler<RecordProductViewCommand>
{
    public async Task Handle(RecordProductViewCommand req, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(3); // Turkey Time
        var today = DateOnly.FromDateTime(now);
        var ipAddress = string.IsNullOrWhiteSpace(req.IpAddress) ? "unknown" : req.IpAddress;
        var ipHash = HashIP(ipAddress);

        // 1. Create View Event
        var viewEvent = new ProductViewEvent
        {
            ProductId = req.ProductId,
            ViewedAt = now,
            UserAgent = req.UserAgent ?? "unknown",
            IpHash = ipHash,
            DeviceType = DetectDeviceType(req.UserAgent ?? "")
        };

        await repoEvent.AddAsync(viewEvent, ct);

        // 2. Update Daily Stats
        var stats = await repoStats.Query(tracked: true)
            .FirstOrDefaultAsync(s => s.ProductId == req.ProductId && s.Day == today, ct);

        if (stats == null)
        {
            stats = new ProductDailyStats
            {
                ProductId = req.ProductId,
                Day = today,
                Views = 1,
                UniqueIps = 1
            };
            await repoStats.AddAsync(stats, ct);
        }
        else
        {
            stats.Views++;

            // Check if this IP has viewed this product today
            var startOfToday = today.ToDateTime(TimeOnly.MinValue); // This is TR date start
            var alreadyViewedToday = await repoEvent.Query(tracked: false)
                .AnyAsync(v => v.ProductId == req.ProductId
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
