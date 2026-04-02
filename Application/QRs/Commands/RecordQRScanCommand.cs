using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Application.QRs.Commands;

public class RecordQRScanCommand : IRequest
{
    public Guid QRCodeId { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Referrer { get; set; }
}

public class RecordQRScanCommandHandler(
    IRepository<QRScanEvent> repoScan,
    IRepository<QRDailyStats> repoStats,
    IUnitOfWork uow
) : IRequestHandler<RecordQRScanCommand>
{
    public async Task Handle(RecordQRScanCommand req, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(3); // Turkey Time
        var today = DateOnly.FromDateTime(now);
        var ipAddress = string.IsNullOrWhiteSpace(req.IpAddress) ? "unknown" : req.IpAddress;
        var ipHash = HashIP(ipAddress);

        // 1. Create Scan Event
        var scanEvent = new QRScanEvent
        {
            QRCodeId = req.QRCodeId,
            ScannedAt = now,
            UserAgent = req.UserAgent ?? "unknown",
            IpHash = ipHash,
            Referrer = req.Referrer,
            DeviceType = DetectDeviceType(req.UserAgent ?? "")
        };

        await repoScan.AddAsync(scanEvent, ct);

        // 2. Update Daily Stats (Simple increment or unique check)
        var stats = await repoStats.Query(tracked: true)
            .FirstOrDefaultAsync(s => s.QRCodeId == req.QRCodeId && s.Day == today, ct);

        if (stats == null)
        {
            stats = new QRDailyStats
            {
                QRCodeId = req.QRCodeId,
                Day = today,
                Scans = 1,
                UniqueIps = 1
            };
            await repoStats.AddAsync(stats, ct);
        }
        else
        {
            stats.Scans++;

            // Check if this IP has scanned today for this QR
            var startOfToday = today.ToDateTime(TimeOnly.MinValue); // This is TR date start
            var alreadyScannedToday = await repoScan.Query(tracked: false)
                .AnyAsync(s => s.QRCodeId == req.QRCodeId
                            && s.IpHash == ipHash
                            && s.ScannedAt >= startOfToday
                            && s.Id != scanEvent.Id, ct);

            if (!alreadyScannedToday)
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
