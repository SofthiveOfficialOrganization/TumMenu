using Application.Abstractions;
using Application.Common.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Application.QRs.Queries;

public record QRRedirectResult(string Url, Guid QRCodeId);

public record ResolveQRQuery(string Key) : IRequest<QRRedirectResult>;

public class ResolveQRQueryHandler(
    IRepository<QRCode> repoQR,
    IMemoryCache cache
) : IRequestHandler<ResolveQRQuery, QRRedirectResult>
{
    private const string CacheKeyPrefix = "qr_resolve_v2_";

    public async Task<QRRedirectResult> Handle(ResolveQRQuery req, CancellationToken ct)
    {
        var cacheKey = $"{CacheKeyPrefix}{req.Key}";

        if (cache.TryGetValue(cacheKey, out QRRedirectResult? cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        var qr = await repoQR.Query(tracked: false)
            .Include(q => q.Store)
                .ThenInclude(s => s!.Company)
            .FirstOrDefaultAsync(q => q.PublicKey == req.Key, ct);

        if (qr == null || !qr.IsActive)
        {
            throw new Application.Common.Exceptions.NotFoundAppException("QR kod bulunamadı veya pasif.");
        }

        string targetUrl;

        switch (qr.ResolveMode)
        {
            case QRResolveMode.StaticUrl:
                targetUrl = qr.TargetUrl ?? "/";
                break;

            case QRResolveMode.StaticMenu:
                targetUrl = BuildStoreUrl(qr.Store);
                if (qr.MenuId.HasValue) targetUrl += $"?menuId={qr.MenuId}";
                break;

            case QRResolveMode.LatestActive:
            default:
                targetUrl = BuildStoreUrl(qr.Store);
                break;
        }

        if (targetUrl.Contains("?"))
            targetUrl += "&isQr=true";
        else
            targetUrl += "?isQr=true";

        var result = new QRRedirectResult(targetUrl, qr.Id);

        // Cache for 1 hour
        cache.Set(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }

    private string BuildStoreUrl(Store? store)
    {
        if (store == null || store.Company == null) return "/";
        
        return $"/{store.Company.Slug}/{store.Slug}";
    }
}
